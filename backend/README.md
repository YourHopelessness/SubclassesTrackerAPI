# SubclassesTrackerExtension

This repository contains an ASP.NET Core API for retrieving and processing
ESO Logs data. It interacts with the official ESO Logs GraphQL API to fetch
reports, fights, players and buff information for Elder Scrolls Online.

The API is also deployed on a remote host, so running it locally is optional if you only need to consume the service.

## Features

- OAuth authentication flow to obtain access tokens for the ESO Logs API.
- Endpoints for retrieving fights, players and buffs for a given log.
- Background service for collecting logs over time.
- Tools for generating Excel reports with skill line statistics.

## Building

The project targets **.NET 9.0**. Make sure the appropriate .NET SDK is
installed and run:

```bash
 dotnet restore
 dotnet build
```

## Configuration

Application settings are stored in `appsettings.json`. Important values are
nested under the `LinesConfig` section:

```json
{
  "LinesConfig": {
    "ClientId": "<ESO Logs OAuth client id>",
    "TrialStartTimeSlice": 0,
    "TokenFilePath": "./Saves/token.json",
    "EsoLogsApiUrl": "https://www.esologs.com/api/v2/",
    "AuthEndpoint": "https://www.esologs.com/oauth/authorize",
    "TokenEndpoint": "https://www.esologs.com/oauth/token",
    "SkillLinesDb": "Lines/skillTree.db"
  }
}
```
YOU NEED to create your own OAuth client [here](https://www.esologs.com/api/clients)
![image](https://github.com/user-attachments/assets/5c596b6c-2d00-42c2-96cf-57a0d1725d5b)

## Usage

Once running, the API exposes multiple route groups.

### `/api/skill`
* `GET /getFights?logId=<log>` – list fight identifiers for a log
* `GET /getPlayers?logId=<log>` – list players and roles for a log
* `GET /getPlayerBuffs?logId=<log>&playerId=<id>` – buff data for a player
* `GET /getPlayersLines?logId=<log>&fightId=<fight>&bossId=<boss>&wipes=<n>` – skill lines for players
* `GET /getAllReports` – fetch recent reports and their fights

### `/api/job`
* `POST /create?jobType=<type>` – queue a new job
* `GET /{id}` – job status
* `GET /{id}/result` – download job result
* `GET /getAll` – list all jobs

### `/api/oauth`
* `GET /url?clientId=<id>&redirectUrl=<url>` – OAuth redirect URL
* `POST /exchange` – exchange code for tokens
* `POST /refresh` – refresh access token

### `/api/healthcheck`
* `GET /api/healthcheck` – check service status

See the [documentation](docs/API.md) for more details.

Pre-built extension packages are available on the [releases page](https://github.com/YourHopelessness/SubclassesTrackerAPI/releases). See [../docs/extension_installation.md](../docs/extension_installation.md) for installation instructions.
