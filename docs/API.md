# API Endpoints

The API exposes several routes for accessing ESO Logs data. All routes are
prefixed with `/api/skill`.

## `GET /getFights`
Returns a list of fight identifiers for the specified log.

Query parameters:
- `logId` – identifier of the log in ESO Logs.

## `GET /getPlayers`
Returns players and their roles for a log. Requires the list of fight IDs
internally.

Query parameters:
- `logId` – log identifier.

## `GET /getPlayerBuffs`
Retrieves buff information for a specific player.

Query parameters:
- `logId` – log identifier.
- `playerId` – player identifier from the log.

## `GET /getAllReports`
Fetches reports for known zones and difficulties and lists their fights.
