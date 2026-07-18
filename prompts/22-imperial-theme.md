# Prompt 22 — "Imperial" theme (monochroom gunmetal, koud signaalrood)

A Star Wars-inspired theme on the Empire's palette: near-monochrome gunmetal grays, steel white, and ONE cold signal red as the scarce accent. This is the most business-ready theme of the set — restrained, precise, high-contrast. Internal theme name "imperial" — no copyrighted names/assets/fonts in shipped code. Token values only, per docs/THEMING.md; parity, token-bleed and a11y guard tests must pass.

---

Copy below into Claude Code in the repo root:

---

Add theme `imperial-dark` plus `imperial-light` to Agterhuis.Ui following docs/THEMING.md. Full token parity with the other families. Character: monochrome discipline — this theme has NO colorful primary; hierarchy comes from grays, and the single red accent lands harder because of it.

## 1. Palette — `imperial-dark` (hero variant)

- Canvas/surfaces (cold gunmetal, blue-leaning): `--agt-surface-0: #0d0f12`, `surface-1: #12151a`, `surface-2: #171b21`, `surface-3: #232933` (hover/active).
- Neutrals (steel): text primary `#eef1f4`, secondary `#c3cad2`, muted `#8b95a1`, hairline border `#252b33`, strong `#3a434f`.
- Primary (interactive — steel, NOT a hue): base `#9aa7b5`, hover `#b4c0cd`, deep `#6e7b8a`, soft tint `#d5dde5`. Interactive elements read as polished metal: primary buttons are steel-filled with `#0d0f12` dark text (measure — steel is light, so dark text on it). Links/interactive text: `#b4c0cd`-class, verified ≥ 4.5:1 on surface-0.
- Accent (imperial signal red, VERY scarce — this theme's budget is ≤3%, tighter than the others): `--agt-color-accent-400: #e5231b`-class cold red, muted `#b81d16`; `--agt-on-accent`: measure white vs `#1f0503` dark text on the final red and pick by contrast. Used ONLY for: active nav edge, the single primary CTA, focus rings, selected states, brand mark. Nothing decorative.
- Semantic — danger vs accent is THE collision here: danger must be visibly warmer/different from signal red — use `#ff7a45`-class orange-red AND rely on the icon+text rule; verify side-by-side on the buttons page. Success `#3fa66a`, warning `#d9a13b`, info `#5b8fa8` — the only hues in the theme besides red; they will pop, which is correct.
- Chart series: monochrome-first — steel `#9aa7b5`, signal red, light steel `#d5dde5`, deep steel `#6e7b8a`, info blue, warning amber. Lead with steel/red (strong color-blind-safe pair); document the order.

## 2. Palette — `imperial-light` ("bridge white", first-class)

- Canvas `#f5f6f8` cold white, surfaces `#ffffff`, borders `#dde2e8`/`#b9c2cc`; headings `#14181d`, body `#22282f`, secondary `#4d5763`, muted `#77828e`.
- Interactive: `#3d4956` charcoal-steel (fills get white text — measure), hover `#2c353f`; nav active = `#e8ecf0` bg + red left edge + `#14181d` text.
- Accent red on white: fills/edges with the measured on-accent; red as TEXT needs `#c11f18`+ (verify ≥ 4.5:1).
- Monochrome-light failure mode is flatness: every surface boundary needs a visible border or cool shadow; hover/selected states must be clearly distinguishable using gray steps alone (test on the datagrid).

## 3. Personality tokens (make it feel Imperial, not just gray)

- Fonts: sharp, technical — reuse the condensed display font (Barlow Condensed) with wide letter-spacing for headings; body stays the neutral sans.
- Shape: near-square — radius-sm 2px, radius-md 4px, radius-lg 6px (the sharpest family).
- Shadows: hard 2-layer cool shadows, tighter than other themes (machined, not soft).
- Atmosphere: `--agt-canvas-backdrop` = faint cold top-light with a barely-visible horizontal scanline-free gradient (NO texture/noise); glow = cold white-blue, used only on focus; glass = smoked gray glass on floating layers only.
- Ambient motion layer: a single slow cold light-sweep across the canvas (120s), off on calm routes as usual.

## 4. Implementation

- `wwwroot/css/themes/agt-theme.imperial.css` scoped to `[data-agt-theme="imperial-dark"]` / `"imperial-light"`; colors ONLY inside theme scopes (token-bleed audit must stay clean).
- Register in `AgtUiOptions.AvailableThemes` and AgtThemeSwitcher (display name "Imperial"); add the family tagline to the Home hero dictionary (e.g. "Monochroom staal met één koud signaalrood.").
- Full token set incl. header background, hero, nav idle (transparent!), chips, scrollbars, on-accent, focus, chart series, glow, glass, personality tokens — parity test enforces.
- WCAG 2.2 AA: all pairs into docs/A11Y-CONTRAST.md. Monochrome traps to measure explicitly: steel-on-steel button borders (≥3:1 against adjacent surface), muted text on surface-0, disabled states distinguishable from idle without color.

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity, bleed audit, catalog smoke under imperial-dark AND imperial-light). Walk Home, Buttons (danger vs accent side-by-side!), Text Inputs, DataGrid (gray-step hover/selected clearly visible), Pickers (popups themed), Navigation in both variants; switch from other families and back — no leftover hues, no flat white-on-white or gray-on-gray boundaries. Report the palette table with final hex values and contrast ratios, and where the ≤3% red budget went.
