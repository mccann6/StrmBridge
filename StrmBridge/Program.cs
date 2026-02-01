using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StrmBridge.Api.Debrid;
using StrmBridge.Api.Debrid.Torbox;
using StrmBridge.Configuration;
using StrmBridge.Configuration.Interfaces;
using StrmBridge.Configuration.Providers;
using StrmBridge.Data;
using StrmBridge.Providers;
using StrmBridge.Providers.Torbox;
using StrmBridge.Sync;
using StrmBridge.Sync.Naming;

namespace StrmBridge;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<AppSettings>(
            builder.Configuration.GetSection(AppSettings.SectionName));

        builder.Services.Configure<TorboxSettings>(
            builder.Configuration.GetSection(TorboxSettings.SectionName));

        builder.Services.AddSingleton<IAppSettings>(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        builder.Services.AddDbContext<LinkerDbContext>((sp, options) =>
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
        builder.Services.AddScoped<IDebridApiClient, TorboxApiClient>();
        builder.Services.AddScoped<IDebridProvider, TorboxProvider>();
        builder.Services.AddSingleton<IMediaNamingStrategy, MediaServerNamingStrategy>();
        builder.Services.AddScoped<IStrmFileManager, StrmFileManager>();
        builder.Services.AddScoped<ISyncEngine, SyncEngine>();
        builder.Services.AddHostedService<SyncBackgroundService>();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LinkerDbContext>();
            db.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
