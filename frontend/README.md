# Frontend Extension

This directory contains a browser extension that augments the ESO Logs website with additional subclass information. The extension communicates with the backend API to retrieve skill line data.

## Development

```bash
npm install
npm run dev
```

Running `npm run dev` uses Vite to watch the source files and rebuild the extension into the `dist` directory.


Pre-built extension packages are available on the [releases page](https://github.com/YourHopelessness/SubclassesTrackerAPI/releases).

## Loading the extension

See the [installation guide](../docs/extension_installation.md) for instructions on loading the build as an unpacked extension in Chrome or Firefox.
