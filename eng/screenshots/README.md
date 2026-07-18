# Screenshot Automation

This folder contains the deterministic screenshot pipeline used for GitHub-facing documentation assets.

## What it does

- Starts the demo app on http://127.0.0.1:5079 (or reuses an already running instance on that port).
- Uses Playwright Chromium with viewport 1600x900.
- Sets the theme through localStorage before navigation.
- Waits for:
  - network idle,
  - a page-specific selector,
  - a 500ms settle delay (fonts/count-up stabilization).
- Captures PNG screenshots to docs/assets.
- Runs a blank-guard check (fails when >95% pixels are near-identical luminance).
- Optimizes PNG output and enforces <= 300KB file size per capture.

## Run

```bash
npm run screenshots
```

## Prerequisites

```bash
npm install
npm run a11y:install
```

The `a11y:install` command installs Playwright browsers and is reused for screenshots.
