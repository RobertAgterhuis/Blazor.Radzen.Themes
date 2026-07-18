# Contrast Sweep

Measured DOM-level contrast audit for the demo app.

Run it from the repo root with:

```bash
npm run contrast:sweep
```

The script starts the demo if needed, iterates the theme variants and route inventory, writes `docs/CONTRAST-SWEEP.md`, and exits non-zero when unallowlisted violations remain.