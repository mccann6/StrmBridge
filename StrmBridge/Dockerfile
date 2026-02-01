# Build stage - compile .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StrmBridge.csproj", "."]
RUN dotnet restore "./StrmBridge.csproj"
COPY . .
RUN dotnet build "./StrmBridge.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StrmBridge.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - simple .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 9847
ENV ASPNETCORE_HTTP_PORTS=9847

# Create directories for media and data
RUN mkdir -p /app/media /app/data

# Copy our .NET app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "StrmBridge.dll"]
