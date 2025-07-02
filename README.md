# SubclassesTrackerExtension

This repository contains an ASP.NET Core API for retrieving and processing
ESO Logs data. It interacts with the official ESO Logs GraphQL API to fetch
reports, fights, players and buff information for Elder Scrolls Online.

## Features

- OAuth authentication flow to obtain access tokens for the ESO Logs API.
- GraphQL client powered by Strawberry Shake.
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
    "LocalCallBackOAuthUri": "https://localhost:7192/auth/callback",
    "AuthEndpoint": "https://www.esologs.com/oauth/authorize",
    "TokenEndpoint": "https://www.esologs.com/oauth/token",
    "SkillLinesDb": "Lines/skillTree.db"
  }
}
```
You can create your own OAuth client [https://www.esologs.com/api/clients](here) 
![image](https://github.com/user-attachments/assets/5c596b6c-2d00-42c2-96cf-57a0d1725d5b)

## Usage

Once running, the following routes are available under `/api/skill`:

- `GET /getFights?logId=<log>` – list fight identifiers for a log
- `GET /getPlayers?logId=<log>` – list players and roles for a log
- `GET /getPlayerBuffs?logId=<log>&playerId=<id>` – buff data for a player
- `GET /getAllReports` – fetch recent reports and their fights

See the [documentation](docs/API.md) for more details.

