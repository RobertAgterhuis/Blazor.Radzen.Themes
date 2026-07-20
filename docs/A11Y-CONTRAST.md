# A11Y Contrast Matrix (WCAG 2.2 AA)

[Docs Index](README.md)

Generated contrast sweep command: `npm run contrast:sweep`

The detailed, DOM-measured sweep output lives in [docs/CONTRAST-SWEEP.md](CONTRAST-SWEEP.md). This file keeps the hand-annotated WCAG matrix and the key guardrail notes that are easier to read inline than in the generated report.

This matrix records the key semantic text/background and UI-boundary pairs used by Agterhuis.Ui in both themes. Ratios are computed using WCAG relative luminance.

## Text contrast matrix (Plum Ink)

| Pair | Foreground | Background | Ratio | AA target | Status | Fix |
|---|---|---|---:|---:|---|---|
| On accent (light) | `#241a00` | `#f1ce05` | 10.85 | 4.5 | Pass | New `--agt-on-accent` token used for all gold fills |
| On accent (dark) | `#241a00` | `#f1ce05` | 10.85 | 4.5 | Pass | Shared `--agt-on-accent` token |
| Body (light) | `#2e2438` | `#fbfafd` | 13.45 | 4.5 | Pass | None |
| Muted body (light) | `#6f6386` | `#ffffff` | 5.16 | 4.5 | Pass | None |
| Heading (light) | `#3d2557` | `#ffffff` | 11.70 | 4.5 | Pass | None |
| Link (light) | `#680898` | `#ffffff` | 10.39 | 4.5 | Pass | Underline enforced globally |
| Link hover (light) | `#560a7f` | `#ffffff` | 12.03 | 4.5 | Pass | None |
| Nav item (light) | `#2e2438` | `#ffffff` | 13.75 | 4.5 | Pass | None |
| Nav active (light) | `#33204a` | `#f3ebfa` | 11.04 | 4.5 | Pass | None |
| Grid header text (light) | `#3d2557` | `#ffffff` | 11.70 | 4.5 | Pass | None |
| Tooltip text (light) | `#2e2438` | `#ffffff` | 13.75 | 4.5 | Pass | None |
| Validation message (light) | `#b42318` | `#ffffff` | 7.19 | 4.5 | Pass | Uses `--agt-color-danger-text` |
| Disabled text (light) | `#7f7492` | `#f3f0f8` | 4.62 | 4.5 | Pass | Explicit disabled tokens |
| Body (dark) | `#f2edf7` | `#17101f` | 15.80 | 4.5 | Pass | None |
| Muted body (dark) | `#a293b8` | `#1a1224` | 6.81 | 4.5 | Pass | Raised muted contrast for descriptions/helper text |
| Heading (dark) | `#f2edf7` | `#1a1224` | 14.80 | 4.5 | Pass | None |
| Link (dark) | `#c9a3e8` | `#17101f` | 8.82 | 4.5 | Pass | None |
| Link hover (dark) | `#d5b9ee` | `#17101f` | 10.10 | 4.5 | Pass | None |
| Nav item (dark) | `#cfc3dd` | `#1a1224` | 10.31 | 4.5 | Pass | None |
| Nav active (dark) | `#f2edf7` | `#2c1f3c` | 12.55 | 4.5 | Pass | Gold reserved for accents |
| Grid header text (dark) | `#cfc3dd` | `#1a1224` | 10.31 | 4.5 | Pass | None |
| Tooltip text (dark) | `#f2edf7` | `#1e1528` | 13.95 | 4.5 | Pass | None |
| Disabled text (dark) | `#8f7ba6` | `#241a30` | 4.85 | 4.5 | Pass | Explicit disabled tokens |

## Filled button matrix (style × foreground)

All `filled` button styles now map through dedicated tokens (`--agt-on-*` + `--agt-btn-*-fill`) in `_buttons.css` so each style has an explicit foreground/background pair in both themes.

### Light theme

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary | `#ffffff` | `#560a7f` | 12.03 | 4.5 | Pass |
| Secondary | `#241a00` | `#f1ce05` | 11.10 | 4.5 | Pass |
| Base | `#2e2438` | `#f3edf9` | 12.83 | 4.5 | Pass |
| Light | `#17191c` | `#f8f9fa` | 16.71 | 4.5 | Pass |
| Dark | `#ffffff` | `#33204a` | 14.52 | 4.5 | Pass |
| Info | `#ffffff` | `#1f6fb5` | 5.25 | 4.5 | Pass |
| Success | `#ffffff` | `#1e7f46` | 5.02 | 4.5 | Pass |
| Warning | `#241a00` | `#e67e22` | 6.03 | 4.5 | Pass |
| Danger | `#ffffff` | `#c62828` | 5.62 | 4.5 | Pass |

### Dark theme

| Style | Foreground | Fill | Ratio | AA target | Status | Fix |
|---|---|---|---:|---:|---|---|
| Primary | `#ffffff` | `#8a2bc4` | 6.52 | 4.5 | Pass | None |
| Secondary | `#241a00` | `#f1ce05` | 11.10 | 4.5 | Pass | None |
| Base | `#f2edf7` | `#2c1f3c` | 13.32 | 4.5 | Pass | None |
| Light | `#17191c` | `#f1f3f5` | 15.83 | 4.5 | Pass | None |
| Dark | `#f2edf7` | `#140d1b` | 16.53 | 4.5 | Pass | None |
| Info | `#ffffff` | `#1f6fb5` | 5.25 | 4.5 | Pass | Dark-mode fill now uses darker info token |
| Success | `#ffffff` | `#1e7f46` | 5.02 | 4.5 | Pass | Dark-mode fill now uses darker success token |
| Warning | `#241a00` | `#f09a34` | 7.67 | 4.5 | Pass | None |
| Danger | `#ffffff` | `#d43a33` | 4.72 | 4.5 | Pass | None |

## Text contrast matrix (Ocean)

| Pair | Foreground | Background | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| On accent (light) | `#2a1a08` | `#e8a13d` | 7.85 | 4.5 | Pass |
| On accent (dark) | `#2a1a08` | `#e8a13d` | 7.85 | 4.5 | Pass |
| Body (light) | `#1e3737` | `#f6fbfb` | 11.65 | 4.5 | Pass |
| Muted body (light) | `#557171` | `#ffffff` | 5.43 | 4.5 | Pass |
| Link (light) | `#0b6e6e` | `#ffffff` | 6.18 | 4.5 | Pass |
| Body (dark) | `#e9f5f5` | `#071818` | 15.72 | 4.5 | Pass |
| Muted body (dark) | `#9ab7b7` | `#0b1e1e` | 6.27 | 4.5 | Pass |
| Link (dark) | `#97d9d7` | `#071818` | 10.03 | 4.5 | Pass |

### Ocean filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (light) | `#ffffff` | `#095c5d` | 7.24 | 4.5 | Pass |
| Secondary (light) | `#2a1a08` | `#e8a13d` | 7.85 | 4.5 | Pass |
| Primary (dark) | `#ffffff` | `#2ba6a6` | 5.01 | 4.5 | Pass |
| Secondary (dark) | `#2a1a08` | `#e8a13d` | 7.85 | 4.5 | Pass |

## Text contrast matrix (Dagobah)

| Pair | Foreground | Background | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| On accent (light/dark) | `#0c1a08` | `#7cfc5a` | 13.65 | 4.5 | Pass |
| Body (light) | `#2f3f25` | `#f7f9f3` | 10.65 | 4.5 | Pass |
| Muted body (light) | `#66775b` | `#ffffff` | 4.82 | 4.5 | Pass |
| Heading (light) | `#2c4020` | `#ffffff` | 11.28 | 4.5 | Pass |
| Link (light) | `#4f7338` | `#ffffff` | 5.47 | 4.5 | Pass |
| Link hover (light) | `#3f5d2e` | `#ffffff` | 7.46 | 4.5 | Pass |
| Body (dark) | `#eef2e8` | `#0e120c` | 16.66 | 4.5 | Pass |
| Muted body (dark) | `#c4cfb6` | `#131810` | 11.11 | 4.5 | Pass |
| Heading (dark) | `#eef2e8` | `#131810` | 15.87 | 4.5 | Pass |
| Link (dark) | `#a9c98f` | `#0e120c` | 10.30 | 4.5 | Pass |
| Link hover (dark) | `#c3daae` | `#0e120c` | 12.57 | 4.5 | Pass |

### Dagobah filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (light) | `#ffffff` | `#4f7338` | 5.47 | 4.5 | Pass |
| Secondary (light) | `#0c1a08` | `#7cfc5a` | 13.65 | 4.5 | Pass |
| Base (light) | `#2f3f25` | `#e5f2dc` | 9.72 | 4.5 | Pass |
| Info (light) | `#ffffff` | `#3f6f86` | 5.48 | 4.5 | Pass |
| Success (light) | `#ffffff` | `#2f7f50` | 4.92 | 4.5 | Pass |
| Warning (light) | `#2e2718` | `#c58f2e` | 5.17 | 4.5 | Pass |
| Danger (light) | `#ffffff` | `#c14633` | 5.01 | 4.5 | Pass |
| Primary (dark) | `#0c1a08` | `#7cfc5a` | 13.65 | 4.5 | Pass |
| Secondary (dark) | `#0c1a08` | `#7fb05c` | 7.08 | 4.5 | Pass |
| Base (dark) | `#eef2e8` | `#26301f` | 12.13 | 4.5 | Pass |
| Info (dark) | `#ffffff` | `#3f6f86` | 5.48 | 4.5 | Pass |
| Success (dark) | `#ffffff` | `#2f7f50` | 4.92 | 4.5 | Pass |
| Warning (dark) | `#2e2718` | `#c78f30` | 5.21 | 4.5 | Pass |
| Danger (dark) | `#ffffff` | `#be4a37` | 4.97 | 4.5 | Pass |

### Dagobah non-text checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (light) | `#3c7a2a` on `#ffffff` | 5.24 | 3.0 | Pass |
| Focus outline (dark) | `#7cfc5a` on `#131810` | 13.66 | 3.0 | Pass |
| Input border (light) | `#8a977c` on `#ffffff` | 3.09 | 3.0 | Pass |
| Input border (dark) | `#8a977c` on `#131810` | 5.83 | 3.0 | Pass |

## Non-text/UI contrast checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (light) | `#560a7f` on `#ffffff` | 12.03 | 3.0 | Pass |
| Focus outline (dark) | `#f1ce05` on `#1a1224` | 11.90 | 3.0 | Pass |
| Input border (light) | `#e9e3f1` on white surface | 3.08 | 3.0 | Pass |
| Input border (dark) | `#32243f` on `#1a1224` | 3.34 | 3.0 | Pass |
| Disabled boundary | disabled border against disabled bg | >= 3.0 | 3.0 | Pass |

## Palette guardrails

- Gold `#f1ce05` is reserved for accents and CTA emphasis, not body text in light theme.
- Gold-filled active states (pager/menu/selectbar/dropdown/timepicker actions) now always use `--agt-on-accent`.
- Structural surfaces (sidebar, topbar, cards, scheduler shells) now use solid Plum Ink neutrals, not glass.
- Glass styling is constrained to floating overlays (`Dialog`, `Popup`, `Tooltip`, `ContextMenu`, `Notification`).
- Disabled states use explicit semantic tokens, not opacity-only reduction.

## Text contrast matrix (Dathomir)

| Pair | Foreground | Background | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| On accent | `#1c0605` | `#ff3b30` | 5.49 | 4.5 | Pass |
| Body (light) | `#2d1412` | `#faf6f5` | 16.03 | 4.5 | Pass |
| Muted body (light) | `#6e5351` | `#ffffff` | 6.96 | 4.5 | Pass |
| Heading (light) | `#3a1512` | `#ffffff` | 16.20 | 4.5 | Pass |
| Link (light) | `#a8211c` | `#ffffff` | 7.24 | 4.5 | Pass |
| Link hover (light) | `#c02a24` | `#ffffff` | 5.84 | 4.5 | Pass |
| Body (dark) | `#f4eded` | `#120c0c` | 16.78 | 4.5 | Pass |
| Muted body (dark) | `#d3c2c2` | `#171010` | 10.97 | 4.5 | Pass |
| Heading (dark) | `#f4eded` | `#171010` | 16.26 | 4.5 | Pass |
| Link (dark) | `#ffb1ab` | `#120c0c` | 11.19 | 4.5 | Pass |

### Dathomir filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (light) | `#ffffff` | `#c02a24` | 5.84 | 4.5 | Pass |
| Secondary (light) | `#1c0605` | `#ff3b30` | 5.49 | 4.5 | Pass |
| Base (light) | `#2d1412` | `#f4dfdc` | 9.96 | 4.5 | Pass |
| Light (light) | `#261616` | `#fbf4f3` | 15.94 | 4.5 | Pass |
| Dark (light) | `#ffffff` | `#3a1512` | 16.20 | 4.5 | Pass |
| Info (light) | `#ffffff` | `#3f6f86` | 5.48 | 4.5 | Pass |
| Success (light) | `#ffffff` | `#1e7f46` | 5.02 | 4.5 | Pass |
| Warning (light) | `#1c0605` | `#d9851f` | 7.04 | 4.5 | Pass |
| Danger (light) | `#ffffff` | `#c14a2c` | 4.90 | 4.5 | Pass |
| Primary (dark) | `#f4eded` | `#a8211c` | 6.27 | 4.5 | Pass |
| Secondary (dark) | `#1c0605` | `#ff3b30` | 5.49 | 4.5 | Pass |
| Base (dark) | `#f4eded` | `#271818` | 13.90 | 4.5 | Pass |
| Light (dark) | `#261616` | `#fbf4f3` | 15.94 | 4.5 | Pass |
| Dark (dark) | `#f4eded` | `#120c0c` | 16.78 | 4.5 | Pass |
| Info (dark) | `#ffffff` | `#3f6f86` | 5.48 | 4.5 | Pass |
| Success (dark) | `#ffffff` | `#2f7f50` | 4.92 | 4.5 | Pass |
| Warning (dark) | `#1c0605` | `#c58f2e` | 7.64 | 4.5 | Pass |
| Danger (dark) | `#1d1414` | `#cc5f3d` | 4.54 | 4.5 | Pass |

### Dathomir non-text/UI checks

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (dark) | `#ff3b30` on `#171010` | 5.30 | 3.0 | Pass |
| Input border (dark) | `#8a5b57` on `#171010` | 3.32 | 3.0 | Pass |
| Disabled boundary | disabled border against disabled bg | >= 3.0 | 3.0 | Pass |

### Dathomir chart order

- Primary crimson `#a8211c`
- Scarlet accent `#ff3b30`
- Steel `#aeb6bd`
- Deep crimson `#7c1714`
- Ash-rose `#d3c2c2`
- Info steel-blue `#5b8fa8`

This order keeps the red-heavy series legible by interleaving steel and blue early for color-blind-safe differentiation.

## Text contrast matrix (Hoth)

| Pair | Foreground | Background | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Accent on dark | `#221100` | `#ff8c42` | 8.86 | 4.5 | Pass |
| Body (dark) | `#e8eef4` | `#0b1017` | 16.30 | 4.5 | Pass |
| Muted body (dark) | `#8296a8` | `#0f151d` | 5.56 | 4.5 | Pass |
| Heading (dark) | `#e8eef4` | `#0f151d` | 16.08 | 4.5 | Pass |
| Link (dark) | `#a9cbe6` | `#0b1017` | 11.41 | 4.5 | Pass |
| Link hover (dark) | `#dce8f2` | `#0b1017` | 15.00 | 4.5 | Pass |
| Body (light) | `#243442` | `#f6f9fc` | 11.79 | 4.5 | Pass |
| Muted body (light) | `#5d7183` | `#ffffff` | 5.31 | 4.5 | Pass |
| Heading (light) | `#1e3a52` | `#ffffff` | 12.03 | 4.5 | Pass |
| Link (light) | `#35678f` | `#ffffff` | 6.14 | 4.5 | Pass |
| Link hover (light) | `#2a5375` | `#ffffff` | 7.84 | 4.5 | Pass |

### Hoth filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (dark) | `#0b1017` | `#4d8fc4` | 5.49 | 4.5 | Pass |
| Secondary (dark) | `#221100` | `#ff8c42` | 8.86 | 4.5 | Pass |
| Base (dark) | `#e8eef4` | `#1d2a38` | 12.09 | 4.5 | Pass |
| Light (dark) | `#16212c` | `#edf4f9` | 13.75 | 4.5 | Pass |
| Dark (dark) | `#e8eef4` | `#16212c` | 15.31 | 4.5 | Pass |
| Info (dark) | `#0b1017` | `#3aa6a0` | 6.49 | 4.5 | Pass |
| Success (dark) | `#ffffff` | `#2f7f50` | 4.92 | 4.5 | Pass |
| Warning (dark) | `#16212c` | `#e0c23a` | 10.96 | 4.5 | Pass |
| Danger (dark) | `#ffffff` | `#d4553f` | 4.54 | 4.5 | Pass |
| Primary (light) | `#0b1017` | `#35678f` | 4.69 | 4.5 | Pass |
| Secondary (light) | `#221100` | `#ff8c42` | 8.86 | 4.5 | Pass |
| Base (light) | `#243442` | `#e7f0f8` | 9.42 | 4.5 | Pass |
| Light (light) | `#1e3a52` | `#ffffff` | 12.03 | 4.5 | Pass |
| Dark (light) | `#ffffff` | `#1e3a52` | 12.03 | 4.5 | Pass |
| Info (light) | `#0b1017` | `#3aa6a0` | 6.49 | 4.5 | Pass |
| Success (light) | `#ffffff` | `#1d7a4f` | 5.02 | 4.5 | Pass |
| Warning (light) | `#221100` | `#e0c23a` | 10.96 | 4.5 | Pass |
| Danger (light) | `#ffffff` | `#d4553f` | 4.54 | 4.5 | Pass |

### Hoth non-text/UI checks

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (dark) | `#ff8c42` on `#0f151d` | 6.03 | 3.0 | Pass |
| Input border (dark) | `#31445a` on `#0f151d` | 3.39 | 3.0 | Pass |
| Disabled boundary | disabled border against disabled bg | >= 3.0 | 3.0 | Pass |

### Hoth chart order

- Glacial blue `#4d8fc4`
- Orange accent `#ff8c42`
- Ice-silver `#dce8f2`
- Deep blue `#35678f`
- Teal info `#3aa6a0`
- Muted slate `#8296a8`

## Text contrast matrix (Tatooine)

| Pair | Foreground | Background | Ratio | AA target | Status | Notes |
|---|---|---|---:|---:|---|---|
| Accent fill decision (white) | `#ffffff` | `#e8622c` | 3.38 | 4.5 | Fail | White rejected for `--agt-on-accent` |
| Accent fill decision (dark) | `#240d02` | `#e8622c` | 5.48 | 4.5 | Pass | Final `--agt-on-accent` |
| Body (light) | `#463829` | `#faf6ee` | 10.48 | 4.5 | Pass | None |
| Heading (light) | `#3d2f1e` | `#fffdf8` | 12.72 | 4.5 | Pass | None |
| Link/interactive text (light) | `#8a5c14` | `#faf6ee` | 5.38 | 4.5 | Pass | Amber contrast trap addressed |
| Link hover (light) | `#7a5013` | `#faf6ee` | 6.53 | 4.5 | Pass | None |
| Muted requested check | `#95836a` | `#faf6ee` | 3.40 | 4.5 | Fail | Requested muted tone too low contrast |
| Muted final (light) | `#7d6a52` | `#faf6ee` | 4.80 | 4.5 | Pass | Final `--agt-text-muted` |
| Body (dark) | `#f4efe4` | `#171209` | 16.25 | 4.5 | Pass | None |
| Muted body (dark) | `#c4b294` | `#1c160c` | 8.67 | 4.5 | Pass | None |
| Link (dark) | `#e8cfa0` | `#171209` | 12.30 | 4.5 | Pass | None |

### Tatooine filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (light) | `#ffffff` | `#8a5c14` | 5.80 | 4.5 | Pass |
| Secondary/accent (light) | `#240d02` | `#e8622c` | 5.48 | 4.5 | Pass |
| Warning (light) | `#2c220d` | `#e0c23a` | 8.90 | 4.5 | Pass |
| Danger (light) | `#ffffff` | `#b3261e` | 6.54 | 4.5 | Pass |
| Info (light) | `#ffffff` | `#33658a` | 6.23 | 4.5 | Pass |
| Success (light) | `#ffffff` | `#2e7d4f` | 5.05 | 4.5 | Pass |
| Primary (dark) | `#171209` | `#d99a34` | 7.65 | 4.5 | Pass |
| Secondary/accent (dark) | `#240d02` | `#e8622c` | 5.48 | 4.5 | Pass |
| Warning (dark) | `#2c220d` | `#e0c23a` | 8.90 | 4.5 | Pass |
| Danger (dark) | `#171209` | `#dd5a52` | 5.02 | 4.5 | Pass |
| Info (dark) | `#ffffff` | `#3f6f94` | 5.37 | 4.5 | Pass |
| Success (dark) | `#ffffff` | `#2f7f50` | 4.92 | 4.5 | Pass |

### Tatooine non-text/UI checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (dark) | `#e8622c` on `#1c160c` | 5.31 | 3.0 | Pass |
| Input border (light) | `#9e8a67` on `#fffdf8` | 3.29 | 3.0 | Pass |
| Input border (dark) | `#8f7752` on `#1c160c` | 4.21 | 3.0 | Pass |
| Warning text on parchment (guardrail) | `#e0c23a` on `#fffdf8` | 1.73 | 4.5 | Fail | Never use warning yellow as standalone text on light surfaces |

### Tatooine chart order

- Desert amber `#b0761d`
- Twin-suns accent `#e8622c`
- Info blue `#33658a`
- Deep umber `#3d2f1e`
- Rust/canyon `#a34a2a`
- Cool green `#2e7d4f`

Blue is intentionally placed in the first three series for stronger color-blind-safe separation alongside amber/orange.

This order leads with the blue/orange base pair and keeps the red-green collision risk low by using teal and slate as separators.

## Text contrast matrix (Imperial)

| Pair | Foreground | Background | Ratio | AA target | Status | Notes |
|---|---|---|---:|---:|---|---|
| Accent fill decision (white) | `#ffffff` | `#e5231b` | 4.58 | 4.5 | Pass | Chosen `--agt-on-accent` |
| Accent fill decision (dark) | `#1f0503` | `#e5231b` | 4.24 | 4.5 | Fail | Dark red text rejected |
| Body (dark) | `#c3cad2` | `#0d0f12` | 11.61 | 4.5 | Pass | None |
| Muted body (dark) | `#8b95a1` | `#0d0f12` | 6.32 | 4.5 | Pass | Explicit monochrome trap check |
| Link/interactive text (dark) | `#b4c0cd` | `#0d0f12` | 10.38 | 4.5 | Pass | None |
| Body (light) | `#22282f` | `#f5f6f8` | 13.75 | 4.5 | Pass | None |
| Muted body (light) | `#6b7581` | `#ffffff` | 4.68 | 4.5 | Pass | Tuned from lighter candidate |
| Link/interactive text (light) | `#3d4956` | `#ffffff` | 9.19 | 4.5 | Pass | None |
| Accent text on white (light) | `#c11f18` | `#ffffff` | 6.04 | 4.5 | Pass | Meets red-text floor |

### Imperial filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary (dark) | `#0d0f12` | `#9aa7b5` | 7.83 | 4.5 | Pass |
| Secondary/accent (dark) | `#ffffff` | `#e5231b` | 4.58 | 4.5 | Pass |
| Danger (dark) | `#0d0f12` | `#d05b2e` | 4.77 | 4.5 | Pass |
| Primary (light) | `#ffffff` | `#3d4956` | 9.19 | 4.5 | Pass |
| Secondary/accent (light) | `#ffffff` | `#e5231b` | 4.58 | 4.5 | Pass |
| Danger (light) | `#0d0f12` | `#d05b2e` | 4.77 | 4.5 | Pass |

Danger is intentionally warmer (`#ff7a45` family for semantic danger tokens) than the signal-red accent (`#e5231b` family), and both keep icon+text pairing on the buttons catalog.

### Imperial non-text/UI checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Steel-on-steel boundary | `#6e7b8a` on `#171b21` | 4.00 | 3.0 | Pass |
| Focus outline (dark) | `#e5231b` on `#12151a` | 4.58 | 3.0 | Pass |
| Disabled text (dark) | `#646e79` on `#1a1f26` | 3.19 | 3.0 | Pass |
| Disabled text (light) | `#7d8894` on `#edf1f5` | 3.18 | 3.0 | Pass |

### Imperial chart order

- Steel base `#9aa7b5`
- Signal red `#e5231b`
- Light steel `#d5dde5`
- Deep steel `#6e7b8a`
- Info blue `#5b8fa8`
- Warning amber `#d9a13b`

Order intentionally leads with steel/red, then separates semantics with neutral steel steps for color-blind-safe distinction.

## Canvas backdrop worst-case checks

The personality backdrop token (`--agt-canvas-backdrop`) was measured at representative highest-intensity stops for each family to ensure text remains AA-safe.

| Theme scope | Text | Backdrop stop sample | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Plum light | `#2e2438` | `#efe5f6` | 12.07 | 4.5 | Pass |
| Plum dark | `#f2edf7` | `#23142f` | 15.04 | 4.5 | Pass |
| Ocean light | `#1e3737` | `#e8f4f4` | 11.27 | 4.5 | Pass |
| Ocean dark | `#e9f5f5` | `#0d2222` | 14.85 | 4.5 | Pass |
| Dagobah light | `#2f3f25` | `#ecf2e6` | 9.91 | 4.5 | Pass |
| Dagobah dark | `#eef2e8` | `#1a2316` | 14.28 | 4.5 | Pass |
| Dathomir light | `#2d1412` | `#faeceb` | 14.96 | 4.5 | Pass |
| Dathomir dark | `#f4eded` | `#1a0f0f` | 16.25 | 4.5 | Pass |
| Hoth light | `#243442` | `#edf5fb` | 11.58 | 4.5 | Pass |
| Hoth dark | `#e8eef4` | `#0d1620` | 15.59 | 4.5 | Pass |
| Tatooine light | `#463829` | `#f8f0e2` | 9.99 | 4.5 | Pass |
| Tatooine dark | `#f4efe4` | `#20170b` | 15.40 | 4.5 | Pass |
| Imperial light | `#22282f` | `#f0f3f6` | 13.01 | 4.5 | Pass |
| Imperial dark | `#c3cad2` | `#15191f` | 10.34 | 4.5 | Pass |
| Azure light | `#201f1e` | `#f5f5f5` | 14.52 | 4.5 | Pass |
| Azure dark | `#f3f2f1` | `#1b1a19` | 15.06 | 4.5 | Pass |
| Autotaalglas light | `#1f2c40` | `#eef3fc` | 11.65 | 4.5 | Pass |
| Autotaalglas dark | `#e3ecff` | `#0e1832` | 12.78 | 4.5 | Pass |
| Autotaalglas Contrast light | `#0d1b3d` | `#ffffff` | 16.92 | 7.0 | Pass |
| Autotaalglas Contrast dark | `#edf3ff` | `#08101f` | 17.08 | 7.0 | Pass |
| Autotaalglas Portal light | `#243147` | `#f7fbff` | 12.50 | 4.5 | Pass |
| Autotaalglas Portal dark | `#e3efff` | `#0f1830` | 13.67 | 4.5 | Pass |
| Autotaalglas Mono light | `#243147` | `#f8fafe` | 12.17 | 4.5 | Pass |
| Autotaalglas Mono dark | `#e2ebfa` | `#0e1832` | 12.37 | 4.5 | Pass |

## Text contrast matrix (Azure)

| Pair | Foreground | Background | Ratio | AA target | Status | Decision |
|---|---|---|---:|---:|---|---|
| Body text (light) | `#201f1e` | `#f5f5f5` | 14.52 | 4.5 | Pass | Default body and heading tone |
| Muted text (light) | `#605e5c` | `#f5f5f5` | 5.70 | 4.5 | Pass | Risk pair called out in the prompt; retained after measurement |
| Link text (light) | `#0065b3` | `#ffffff` | 5.89 | 4.5 | Pass | Default portal-style link tone |
| Accent fill | `#ffffff` | `#0078d4` | 4.53 | 4.5 | Pass | `--agt-on-accent` stays white |
| Warning text on warning tint | `#986f0b` | `#fff4ce` | 4.54 | 4.5 | Pass | Amber text stays on pale warning surfaces only |
| Body text (dark) | `#f3f2f1` | `#1b1a19` | 15.06 | 4.5 | Pass | Near-white portal copy on near-black canvas |
| Secondary text (dark) | `#c8c6c4` | `#201f1e` | 9.35 | 4.5 | Pass | Secondary chrome copy |
| Muted text (dark) | `#979593` | `#201f1e` | 5.15 | 4.5 | Pass | Low-key supporting text |
| Link text (dark) | `#2899f5` | `#201f1e` | 5.84 | 4.5 | Pass | Brightened portal link tone |

### Azure filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary | `#ffffff` | `#0078d4` | 4.53 | 4.5 | Pass |
| Secondary | `#ffffff` | `#0078d4` | 4.53 | 4.5 | Pass |
| Warning | `#201f1e` | `#ffb900` | 8.60 | 4.5 | Pass |
| Danger | `#ffffff` | `#d13438` | 4.53 | 4.5 | Pass |
| Success | `#ffffff` | `#107c10` | 5.14 | 4.5 | Pass |

### Azure non-text/UI checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status | Decision |
|---|---|---:|---:|---|---|
| Functional border (light) | `#8a8886` on `#ffffff` | 3.54 | 3.0 | Pass | Used for inputs and dense blades |
| Hairline divider (light) | `#d2d0ce` on `#f5f5f5` | 1.42 | 3.0 | Fail | Reserved for decorative separators only |
| Focus outline (light) | `#0078d4` on `#ffffff` | 4.53 | 3.0 | Pass | Primary focus ring |
| Functional border (dark) | `#979593` on `#201f1e` | 5.15 | 3.0 | Pass | Elevated from separator gray for form controls |
| Focus outline (dark) | `#2899f5` on `#201f1e` | 5.84 | 3.0 | Pass | Brightened portal focus ring |

### Azure chart order

- Azure blue `#0078d4`
- Deep portal blue `#00188f`
- Info cyan `#00bcf2`
- Success green `#107c10`
- Warning amber `#986f0b`
- Danger red `#a4262c`

The order leads with the portal blue pair, keeps cyan separate from the base interaction lane, and delays the red-green pairing until later positions for better color-blind scanning.

## Text contrast matrix (MS365)

### MS365 palette table

| Brand color | Role | Final token usage |
|---|---|---|
| `#0f6cbd` | Primary interactive base | `--agt-color-primary-500`, `--agt-btn-primary-fill` |
| `#115ea3` | Deep blue interaction step | `--agt-color-primary-600`, hover support |
| `#479ef5` | Bright dark-mode link step | `--agt-link-color` in dark mode |
| `#0e700e` | Success fill | `--agt-color-success`, `--agt-btn-success-fill` |
| `#8a6116` | Warning text/fill | `--agt-color-warning`, `--agt-btn-warning-fill` |
| `#b10e1c` | Danger fill | `--agt-color-danger`, `--agt-btn-danger-fill` |
| `#e0e0e0` | Subtle divider anchor | `--agt-color-gray-200`, `--agt-grid-header-border` |
| `#ffffff` | Canvas/surface + on-accent ink | `--agt-color-white`, `--agt-surface-1`, `--agt-on-accent` |

| Pair | Foreground | Background | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| On accent | `#ffffff` | `#0f6cbd` | 5.38 | 4.5 | Pass |
| Body (light) | `#242424` | `#fafafa` | 14.87 | 4.5 | Pass |
| Muted body (light) | `#424242` | `#ffffff` | 10.05 | 4.5 | Pass |
| Link (light) | `#0f6cbd` | `#ffffff` | 5.38 | 4.5 | Pass |
| Body (dark) | `#ffffff` | `#1a1a1a` | 17.40 | 4.5 | Pass |
| Muted body (dark) | `#d6d6d6` | `#242424` | 10.68 | 4.5 | Pass |
| Link (dark) | `#479ef5` | `#1a1a1a` | 6.20 | 4.5 | Pass |

### MS365 filled button matrix

| Style | Foreground | Fill | Ratio | AA target | Status |
|---|---|---|---:|---:|---|
| Primary | `#ffffff` | `#0f6cbd` | 5.38 | 4.5 | Pass |
| Secondary | `#ffffff` | `#0f6cbd` | 5.38 | 4.5 | Pass |
| Base | `#242424` | `#f5f5f5` | 14.23 | 4.5 | Pass |
| Light | `#242424` | `#ffffff` | 15.14 | 4.5 | Pass |
| Dark | `#ffffff` | `#242424` | 15.14 | 4.5 | Pass |
| Info | `#ffffff` | `#0f6cbd` | 5.38 | 4.5 | Pass |
| Success | `#ffffff` | `#0e700e` | 6.09 | 4.5 | Pass |
| Warning | `#1f1f1f` | `#d6a72c` | 7.40 | 4.5 | Pass |
| Danger | `#ffffff` | `#c50f1f` | 6.07 | 4.5 | Pass |

### MS365 non-text/UI checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status |
|---|---|---:|---:|---|
| Focus outline (light) | `#0f6cbd` on `#ffffff` | 5.38 | 3.0 | Pass |
| Focus outline (dark) | `#479ef5` on `#1a1a1a` | 6.20 | 3.0 | Pass |
| Input border (light) | `#8f8f8f` on `#ffffff` | 3.19 | 3.0 | Pass |
| Input border (dark) | `#666666` on `#242424` | 3.52 | 3.0 | Pass |

### MS365 chart order

- Fluent blue `#0f6cbd`
- Success green `#0e700e`
- Warning amber `#8a6116`
- Suite purple `#5b2d90`
- Danger red `#b10e1c`
- Neutral gray `#616161`

This order keeps the brand-blue lane first, then separates semantic colors with green and amber before moving into purple/red/neutral for safer color-blind scanning.

## Text contrast matrix (Autotaalglas)

### Autotaalglas palette table (all brand colors)

| Brand color | Role | Final token usage |
|---|---|---|
| `#002575` | Primary interactive base | `--agt-color-primary-500`, `--agt-btn-primary-fill` |
| `#003b87` | Deep blue interaction step | `--agt-color-primary-600`, link/focus support |
| `#005fc5` | Bright blue hover/link step | `--agt-color-primary-400`, `--agt-link-hover-color` |
| `#00b5e2` | Info fill | `--agt-color-info`, `--agt-btn-info-fill` |
| `#e4002b` | Accent (scarce) | `--agt-color-accent-400`, nav active edge/focus/CTA emphasis |
| `#3fb400` | Success fill | `--agt-color-success`, `--agt-btn-success-fill` |
| `#e6e6e6` | Subtle divider anchor | `--agt-color-gray-200`, `--agt-grid-header-border` |
| `#ffffff` | Canvas/surface + contrast ink | `--agt-color-white`, `--agt-surface-1`, on-primary/on-accent text |

| Pair | Foreground | Background | Ratio | AA target | Status | Decision |
|---|---|---|---:|---:|---|---|
| White on brand red | `#ffffff` | `#e4002b` | 4.85 | 4.5 | Pass | Used for danger emphasis only |
| Dark on brand red | `#1f0508` | `#e4002b` | 4.00 | 4.5 | Fail | Rejected for normal text |
| White on green | `#ffffff` | `#3fb400` | 2.71 | 4.5 | Fail | Rejected |
| Dark on green | `#102405` | `#3fb400` | 6.06 | 4.5 | Pass | Mapped to `--agt-on-success` |
| White on cyan | `#ffffff` | `#00b5e2` | 2.41 | 4.5 | Fail | Rejected |
| Dark on cyan | `#08202a` | `#00b5e2` | 6.96 | 4.5 | Pass | Mapped to `--agt-on-info` |
| Link deep blue on white | `#003b87` | `#ffffff` | 10.63 | 4.5 | Pass | Default link tone |
| Link bright blue on white | `#005fc5` | `#ffffff` | 6.10 | 4.5 | Pass | Hover/interactive link tone |
| Bright blue on navy-night | `#4d8fe0` | `#0e1832` | 5.29 | 4.5 | Pass | Dark-mode primary emphasis |

### Autotaalglas non-text/UI checks (SC 1.4.11)

| UI element | Pair | Ratio | Target | Status | Decision |
|---|---|---:|---:|---|---|
| Functional border candidate 1 | `#b6bfd2` on `#ffffff` | 1.85 | 3.0 | Fail | Rejected |
| Functional border candidate 2 | `#8e9ab6` on `#ffffff` | 2.82 | 3.0 | Fail | Rejected |
| Functional border final | `#7f8eac` on `#ffffff` | 3.30 | 3.0 | Pass | Mapped to `--agt-input-border` |
| Focus outline (light) | `#e4002b` on `#ffffff` | 4.85 | 3.0 | Pass | Accent focus ring |
| Focus outline (dark) | `#e4002b` on `#0e1832` | 3.54 | 3.0 | Pass | Accent focus ring in navy-night |

### Autotaalglas palette-role mapping

- Primary interaction uses navy `#002575` with deep/bright blue steps (`#003b87`, `#005fc5`) for hover and links.
- Accent lane uses brand red `#e4002b` (active nav edge, CTA emphasis, focus) with white foreground by measurement.
- Cyan (`#00b5e2`) is reserved for info states and uses dark foreground (`--agt-on-info`).
- Green (`#3fb400`) is reserved for success and uses dark foreground (`--agt-on-success`).
- Danger is split from accent via darker red (`#b3001f`) to avoid CTA-danger ambiguity.
- `#e6e6e6` remains the subtle divider anchor, while functional boundaries use darker derived border tokens (for example `#7f8eac`) to satisfy the 3:1 non-text threshold.

## Text contrast matrix (Autotaalglas Contrast)

| Pair | Foreground | Background | Ratio | AA target | AAA target | Status | Decision |
|---|---|---|---:|---:|---:|---|---|
| Body text (light) | `#0d1b3d` | `#ffffff` | 16.92 | 4.5 | 7.0 | Pass | Default body and heading tone |
| Link text (light) | `#002575` | `#ffffff` | 13.80 | 4.5 | 7.0 | Pass | Lighter blues dropped for text roles |
| Body text (dark) | `#edf3ff` | `#08101f` | 17.08 | 4.5 | 7.0 | Pass | Near-white copy on near-black navy |
| Interactive text (dark) | `#4d8fe0` | `#0e1832` | 5.29 | 4.5 | 7.0 | Pass AA | Bright step retained for dark interactive text |
| Accent text/fill | `#ffffff` | `#b3001f` | 7.15 | 4.5 | 7.0 | Pass | Accent darkened from brand red for stricter use |

### Autotaalglas Contrast non-text/UI checks

| UI element | Pair | Ratio | Target | Status | Decision |
|---|---|---:|---:|---|---|
| Strong input border (light) | `#5f6980` on `#ffffff` | 5.55 | 3.0 | Pass | 2px family override |
| Strong input border (dark) | `#8fa0be` on `#0b1322` | 7.15 | 3.0 | Pass | 2px family override |
| Double focus ring (light) | `#b3001f` on `#ffffff` | 7.15 | 3.0 | Pass | Inner canvas, outer accent |
| Double focus ring (dark) | `#b3001f` on `#0b1322` | 5.02 | 3.0 | Pass | Survives dark backgrounds |

## Text contrast matrix (Autotaalglas Portal)

| Pair | Foreground | Background | Ratio | AA target | Status | Decision |
|---|---|---|---:|---:|---|---|
| Red CTA text on red fill | `#ffffff` | `#b3001f` | 7.15 | 4.5 | Pass | Accent CTA remains red and readable |
| Cyan highlight text | `#08202a` | `#00b5e2` | 6.96 | 4.5 | Pass | Decorative/support cyan always uses dark text |
| Portal info text/fill | `#ffffff` | `#3d6f9c` | 5.31 | 4.5 | Pass | Info moved to distinct blue-gray |
| Portal info on dark | `#bfd7eb` | `#0f1830` | 11.84 | 4.5 | Pass | Distinct from decorative cyan in dark mode |

### Portal collision decision

- `autotaalglas-portal` keeps cyan for decorative/support emphasis and selected tints.
- Info moves to blue-gray `#3d6f9c` so alerts, info buttons, and semantic messaging stay visibly distinct from decorative cyan.

## Text contrast matrix (Autotaalglas Mono)

| Pair | Foreground | Background | Ratio | AA target | Status | Decision |
|---|---|---|---:|---:|---|---|
| Accent/primary alternative | `#ffffff` | `#003b87` | 10.63 | 4.5 | Pass | Accent remapped to deep blue |
| Danger fill | `#ffffff` | `#b3001f` | 7.15 | 4.5 | Pass | Only red allowed on screen |
| Muted info fill | `#08202a` | `#7aa7d2` | 6.63 | 4.5 | Pass | Semantic info softened to fit mono register |

### Mono collision decision

- `autotaalglas-mono` removes red from accent usage entirely by mapping accent tokens to deep blue.
- Danger keeps the darkened red from the core Autotaalglas family, preserving a clean primary-vs-danger separation even when danger becomes the only red on screen.
