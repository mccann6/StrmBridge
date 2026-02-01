# StrmBridge

A lightweight service that syncs your debrid provider library to `.strm` files for use with media servers like Plex, Jellyfin, or Emby.

## How It Works

StrmBridge periodically polls your debrid provider's API, fetches your library of cached content, and creates `.strm` files organized in a standard media server folder structure:

```
media/
  Movies/
    Movie Name (2024)/
      Movie Name (2024).strm
  TV Shows/
    Show Name/
      Season 01/
        Show Name - S01E01.strm
```

When a media player opens a `.strm` file, it calls back to StrmBridge which redirects to the actual streaming URL. This keeps your API key server-side rather than embedded in the files.

## Supported Providers

- Torbox
- Real-Debrid

## Requirements

- Docker and Docker Compose
- An account with at least one supported debrid provider

## Quick Start

1. Clone the repository

2. Create a `.env` file with your provider API keys:
```
TORBOX_API_KEY=your_torbox_api_key
REALDEBRID_API_KEY=your_realdebrid_api_key
```

3. Run with Docker Compose:
```
docker compose up -d
```

4. Point your media server library at the `./media` folder

The service syncs every 5 minutes by default.

## Configuration

Environment variables can be set in `docker-compose.yml` or via `.env`:

### General Settings

| Variable | Description | Default |
|----------|-------------|---------|
| `App__ServiceBaseUrl` | Base URL for stream redirects | `http://localhost:9847` |
| `App__SyncIntervalSeconds` | Sync interval in seconds | `300` |

### Torbox

| Variable | Description | Default |
|----------|-------------|---------|
| `Providers__Torbox__ApiKey` | Your Torbox API key | Required |
| `Providers__Torbox__IsEnabled` | Enable/disable provider | `false` |

### Real-Debrid

| Variable | Description | Default |
|----------|-------------|---------|
| `Providers__RealDebrid__ApiKey` | Your Real-Debrid API key | Required |
| `Providers__RealDebrid__IsEnabled` | Enable/disable provider | `false` |

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/status` | Service status and last sync info |
| `POST /api/sync` | Trigger a manual sync |
| `GET /api/stream/{provider}/{torrentId}/{fileId}` | Stream redirect (used by .strm files) |

## Building from Source

```
cd StrmBridge
dotnet build
dotnet run
```

## License

MIT
