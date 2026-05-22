import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30000,
  retries: 0,
  workers: 8,
  use: {
    baseURL: 'http://localhost:3000',
    headless: true,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { browserName: 'chromium' },
    },
  ],
  webServer: [
    {
      command: 'cd ../back && dotnet run',
      port: 5000,
      reuseExistingServer: true,
      timeout: 30000,
    },
    {
      command: 'cd ../front && npm start',
      port: 3000,
      reuseExistingServer: true,
      timeout: 30000,
    },
  ],
});