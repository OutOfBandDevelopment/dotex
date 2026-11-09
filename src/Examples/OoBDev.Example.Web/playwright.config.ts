import { defineConfig } from '@playwright/test';

/**
 * End-to-end journeys. By default the SPA runs on the Vite dev server with the API mocked, so the run
 * (and the screenshots it writes into the user guide) is deterministic and needs no containers.
 * Set E2E_BASE_URL (for example http://localhost:8088) to run the same journeys against the compose stack.
 */
// E2E_PORT moves the mocked run off 5173 when another dev server (for example the Aspire host's) already holds it and uses real sign-in.
const port = process.env.E2E_PORT ?? '5173';
const baseURL = process.env.E2E_BASE_URL ?? `http://localhost:${port}`;

export default defineConfig({
  testDir: 'e2e',
  timeout: 180_000,
  testMatch: '**/*.spec.ts',
  fullyParallel: false,
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL,
    viewport: { width: 1280, height: 800 },
    colorScheme: 'light',
    trace: 'retain-on-failure',
  },
  webServer: process.env.E2E_BASE_URL
    ? undefined
    : { command: `npm run dev -- --strictPort --port ${port}`, url: baseURL, reuseExistingServer: true, timeout: 60_000 },
});
