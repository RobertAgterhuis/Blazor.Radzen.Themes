# Prompt 15 — "Tatooine" theme (woestijn, twin-suns licht + nacht-donker)

A Star Wars-inspired theme on Tatooine's palette: sun-bleached sand, warm dune neutrals, desert amber as primary, twin-suns glow orange-red as the scarce accent, moisture-farm rust for warm details. Internal theme name "tatooine" — no copyrighted names/assets/fonts in shipped code. Token values only, per docs/THEMING.md; parity test must pass.

---

Copy below into Claude Code in the repo root:

---

Add theme `tatooine-light` plus `tatooine-dark` to Agterhuis.Ui following docs/THEMING.md. Like Hoth, the LIGHT variant is first-class here (Tatooine is a daylight desert); dark is "desert night".

## 1. Palette — `tatooine-light` (twin suns, hero variant)

- Canvas: warm sand `#faf6ee`; surfaces: off-white parchment `#fffdf8`; hover/active tint `#f3ead9`; hairline border `#e6dcc8`, strong `#c9b998`.
- Neutrals (dune-tinted): headings deep umber `#3d2f1e`, body `#463829`, secondary `#6e5d47`, muted `#95836a`.
- Primary (desert amber, interactive): base `#b0761d`, hover `#96631a`, deep `#7a5013`, soft tint `#e8cfa0`. Build the 50–950 scale. NB: amber text on sand is a contrast trap — the interactive TEXT variant must be `#8a5c14`+ and verified ≥ 4.5:1 on `#faf6ee`.
- Accent (twin-suns glow, scarce — ≤5% budget): vivid orange-red `--agt-color-accent-400: #e8622c`-class, muted `#c2521f`; `--agt-on-accent: #ffffff` if `#e8622c` passes with white — otherwise darken the fill until it does, or use `#240d02` dark text; decide by measurement, not taste. Used for: active nav edge, primary CTA, focus rings, selected states, brand mark.
- Warm secondary (rust/canyon, sparing): `#a34a2a` for subtle highlights; text on rust = white (verify).
- Semantic — this palette is one big amber/orange collision course, so be strict: success cool green `#2e7d4f` (reads clearly against sand), danger a DEEP red `#b3261e` clearly darker/redder than the accent orange (side-by-side check on the buttons page), warning must differ from BOTH primary amber and accent orange — use a yellow `#e0c23a`-class with dark text and always icon+text, info cool blue `#33658a` (the only cool color, it will pop — that's fine, info should). All AA-checked.
- Chart series: desert amber, twin-suns accent, info blue, deep umber, rust, cool green — blue early in the order for color-blind safety; document the order.

## 2. Palette — `tatooine-dark` (desert night)

- Canvas `--agt-surface-0: #171209` warm near-black; `surface-1: #1c160c`, `surface-2: #221a0f`, `surface-3: #302614` (hover/active).
- Neutrals: text primary `#f4efe4`, secondary `#d8ccb4`, muted `#a08c6c`; borders `#302614`/`#4a3b22`.
- Primary brightens for dark: base `#d99a34`, hover `#e6ab48`, deep `#b0761d`; accent stays `#e8622c`-class with the measured on-accent.
- Same semantic set re-verified against the dark surfaces (deep red danger will need lightening here — measure).
- Glow: warm ember glow; glass: smoked amber glass (floating layers only).

## 3. Implementation

- `wwwroot/css/themes/agt-theme.tatooine.css` scoped to `[data-agt-theme="tatooine-light"]` / `"tatooine-dark"`; colors ONLY inside theme scopes.
- Register in `AgtUiOptions.AvailableThemes` and AgtThemeSwitcher (display name "Tatooine"); default variant for this family = light.
- Full token parity (parity test passes): focus rings, glow, glass, scrollbars, on-accent, chart series.
- WCAG 2.2 AA: all pairs into docs/A11Y-CONTRAST.md. Warm-palette traps to measure explicitly: amber interactive text on sand, white vs dark text on the accent, gold-ish warning on parchment surfaces, muted text `#95836a` on `#faf6ee`.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity + catalog smoke under tatooine-light AND tatooine-dark). Switch through all theme families on Text Inputs, Buttons, DataGrid, Pickers (open popups), Navigation — instant, complete re-theme, no leftovers from other themes. Specifically check: primary vs accent vs warning vs danger all instantly distinguishable on one screen; tatooine-light has no cream-on-cream boundaries (every surface edge visible). Report the palette table with final hex values and contrast ratios.
