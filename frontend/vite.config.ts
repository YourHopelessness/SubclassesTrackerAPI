import { defineConfig } from 'vite';
import webExtension from 'vite-plugin-web-extension';
import fs from 'fs';

export default defineConfig({
  plugins: [
    //@ts-ignore
    webExtension({
      manifest: () => JSON.parse(fs.readFileSync('src/manifest.json', 'utf8')),
      watchFilePaths: ['src/manifest.json']
    }),
  ],
  build: {
    target: 'chrome114', 
  },
});