# StrmBridge

A lightweight service that syncs your debrid provider library to `.strm` files for use with media servers like Jellyfin.

## How It Works

StrmBridge periodically polls your debrid provider's API, fetches your library of cached content, and creates `.strm` files organized in a standard media server folder structure:

```
media/
  Movies/
    Movie Name (2024)/
      Movie Name (2024) - 1080p WEB-DL.strm
      Movie Name (2024) - 2160p BluRay.strm
  TV Shows/
    Show Name/
      Season 01/
        Show Name - S01E01 - 720p HDTV.strm
```

Files include quality and source information when available, allowing Jellyfin to display multiple versions of the same title.

When a media player opens a `.strm` file, it calls back to StrmBridge which redirects to the actual streaming URL. This keeps your API key server-side rather than embedded in the files.

## Supported Providers

- Torbox
- Real-Debrid

## Unraid

StrmBridge is available in **Community Apps** — search for `strmbridge`.

Key settings to configure:

- **Service Base URL** — Set this to the URL of the service as reachable from your network (e.g. `http://192.168.1.50:9847`). This is written into `.strm` files so your media server can reach StrmBridge.
- **Strm Files Output** — Where `.strm` files are created. Point your Jellyfin library at this path.
- **API Key(s)** — Enter your Torbox and/or Real-Debrid API key and enable the provider.

## Docker Compose

1. Create a `.env` file with your provider API keys:
```
TORBOX_API_KEY=your_torbox_api_key
REALDEBRID_API_KEY=your_realdebrid_api_key
```

2. Run with Docker Compose:
```
docker compose up -d
```

3. Point your media server library at the `./media` folder

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
