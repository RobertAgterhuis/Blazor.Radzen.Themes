# Example Scan

Runs a Playwright-based scan over catalog and wrapper component pages and validates every DemoExample preview panel.

Checks per example:
- at least one component root marker (`rz-` or `agt-` class)
- rendered preview height >= 40px
- no visible error text
- not visually empty
- popup components (dropdown/datepicker) open when triggered

Command:

```bash
npm run example:scan
```

Optional flags:

```bash
node eng/example-scan/example-scan.mjs --theme=autotaalglas-dark --max-routes=25
```
