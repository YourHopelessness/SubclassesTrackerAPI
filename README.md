# Subclasses Tracker

This repository contains two projects:

- **backend/** – ASP.NET Core API (`SubclassesTrackerExtension`) that queries the official ESO Logs GraphQL API and exposes a simplified REST API for collecting skill line data.
- **frontend/** – browser extension built with Vite and TypeScript. It injects additional subclass information into the ESO Logs website using data from the backend API.

The backend and extension can be used together or separately. See the individual READMEs in each directory for full details.

## Quick start

1. Clone the repository and install the dependencies for each project.
2. *(Optional)* run the backend locally. The API is already hosted remotely so this step can be skipped:

```bash
cd backend
 dotnet restore
 dotnet run
```

Alternatively, skip this step and use the API instance already running on the remote host.

3. Download the latest browser extension from the [releases page](https://github.com/YourHopelessness/SubclassesTrackerAPI/releases).

## Loading the extension unpacked

After downloading the extension you can load it directly in your browser without packaging:

- **Chrome / Chromium**: open `chrome://extensions`, enable *Developer mode* and click **Load unpacked**. Select the unzipped extension folder.
- **Firefox**: open `about:debugging#/runtime/this-firefox`, choose **Load Temporary Add-on** and select the `manifest.json` file from the unzipped extension folder.

For more details see [`docs/extension_installation.md`](docs/extension_installation.md).

## Documentation

Additional documentation is stored under the `docs/` folder and within each subproject.
