using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StrmBridge.Api.Debrid;
using StrmBridge.Api.Debrid.RealDebrid;
using StrmBridge.Api.Debrid.Torbox;
using StrmBridge.Configuration;
using StrmBridge.Configuration.Interfaces;
using StrmBridge.Configuration.Providers;
using StrmBridge.Data;
using StrmBridge.Providers;
using StrmBridge.Providers.RealDebrid;
using StrmBridge.Providers.Torbox;
using StrmBridge.Sync;
using StrmBridge.Sync.Naming;

namespace StrmBridge;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.SingleLine = true;
        });

        builder.Services.Configure<AppSettings>(
            builder.Configuration.GetSection(AppSettings.SectionName));

        builder.Services.Configure<TorboxSettings>(
            builder.Configuration.GetSection(TorboxSettings.SectionName));

        builder.Services.Configure<RealDebridSettings>(
            builder.Configuration.GetSection(RealDebridSettings.SectionName));

        builder.Services.AddSingleton<IAppSettings>(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        builder.Services.AddDbContext<StrmBridgeDbContext>((sp, options) =>
        {
            var appSettings = sp.GetRequiredService<IAppSettings>();
            var dbPath = appSettings.DatabasePath;
            
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }
            
            options.UseSqlite($"Data Source={dbPath}");
        });

        builder.Services.AddScoped<IMediaItemRepository, MediaItemRepository>();
        builder.Services.AddHttpClient<TorboxApiClient>();
        builder.Services.AddHttpClient<RealDebridApiClient>();
        builder.Services.AddScoped<IDebridApiClient, TorboxApiClient>();
        builder.Services.AddScoped<IDebridApiClient, RealDebridApiClient>();
        builder.Services.AddScoped<IDebridProvider, TorboxProvider>();
        builder.Services.AddScoped<IDebridProvider, RealDebridProvider>();
        builder.Services.AddSingleton<IMediaNamingStrategy, MediaServerNamingStrategy>();
        builder.Services.AddScoped<IStrmFileManager, StrmFileManager>();
        builder.Services.AddScoped<ISyncEngine, SyncEngine>();
        builder.Services.AddHostedService<SyncBackgroundService>();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StrmBridgeDbContext>();
            db.Database.EnsureCreated();
        }

        ValidateProviderConfiguration(app.Services, app.Logger);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void ValidateProviderConfiguration(IServiceProvider services, ILogger logger)
    {
        var torboxSettings = services.GetRequiredService<IOptions<TorboxSettings>>().Value;
        var realDebridSettings = services.GetRequiredService<IOptions<RealDebridSettings>>().Value;

        if (torboxSettings.IsEnabled && string.IsNullOrEmpty(torboxSettings.ApiKey))
        {
            logger.LogWarning("Torbox is enabled but no API key is configured. Provider will be skipped.");
        }

        if (realDebridSettings.IsEnabled && string.IsNullOrEmpty(realDebridSettings.ApiKey))
        {
            logger.LogWarning("Real-Debrid is enabled but no API key is configured. Provider will be skipped.");
        }

        var enabledProviders = new List<string>();
        if (torboxSettings.IsEnabled && !string.IsNullOrEmpty(torboxSettings.ApiKey))
            enabledProviders.Add("Torbox");
        if (realDebridSettings.IsEnabled && !string.IsNullOrEmpty(realDebridSettings.ApiKey))
            enabledProviders.Add("Real-Debrid");

        if (enabledProviders.Count == 0)
        {
            logger.LogWarning("No debrid providers are configured. Add an API key to enable syncing.");
        }
        else
        {
            logger.LogInformation("Enabled providers: {Providers}", string.Join(", ", enabledProviders));
        }
    }
}
