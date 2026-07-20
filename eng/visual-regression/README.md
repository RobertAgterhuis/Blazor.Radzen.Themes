# Visual Regression

This harness compares a fixed set of demo and showcase routes against committed PNG baselines.

## Commands

```bash
npm run vr:test
npm run vr:approve
```

`vr:test` compares the current render against the baselines in `baselines/`. `vr:approve` refreshes the committed baselines.

## What it covers

- Home, buttons, data grid, and form routes
- One showcase route and one blog route
- Designer start screen and canvas
- Desktop and tablet viewports
- Themes: plum-dark, hoth-light, imperial-dark, autotaalglas-light, volt-dark