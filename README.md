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

## Requirements

- Docker and Docker Compose
- A Torbox account with API key

## Quick Start

1. Clone the repository

2. Create a `.env` file:
```
TORBOX_API_KEY=your_api_key_here
```

3. Run with Docker Compose:
```
docker compose up -d
```

4. Point your media server library at the `./media` folder

The service syncs every 5 minutes by default.

## Configuration

Environment variables can be set in `docker-compose.yml` or via `.env`:

| Variable | Description | Default |
|----------|-------------|---------|
| `Providers__Torbox__ApiKey` | Your Torbox API key | Required |
| `App__ServiceBaseUrl` | Base URL for stream redirects | `http://localhost:9847` |
| `App__SyncIntervalSeconds` | Sync interval in seconds | `300` |

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
