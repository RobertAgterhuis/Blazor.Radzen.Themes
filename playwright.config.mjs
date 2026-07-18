import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/a11y",
  timeout: 60000,
  retries: 0,
  use: {
    baseURL: process.env.AGT_DEMO_URL ?? "http://127.0.0.1:5079",
    browserName: "chromium",
    headless: true
  },
  reporter: [["list"], ["html", { open: "never", outputFolder: "artifacts/a11y-report" }]]
});
