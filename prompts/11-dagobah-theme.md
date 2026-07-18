# Prompt 11 — "Dagobah" theme (donker, Yoda-kleuren)

A dark Star Wars-inspired theme built on Yoda's palette: swamp-dark canvas, sage-green neutrals, Yoda-skin green as primary, lightsaber green as the scarce accent, robe-beige for warm highlights. Uses the multi-theme recipe from THEMING.md (token values only — no component CSS, no copyrighted names/assets/fonts in shipped code; the internal theme name is "dagobah").

---

Copy below into Claude Code in the repo root:

---

Add a new theme `dagobah-dark` (plus a serviceable `dagobah-light`) to Agterhuis.Ui following docs/THEMING.md. Token values only; every token the plum theme defines must exist here too.

## 1. Palette (dark = the hero variant)

- Canvas/surfaces (swamp night): `--agt-surface-0: #0e120c`, `surface-1: #131810`, `surface-2: #171d14`, `surface-3: #222b1c` (hover/active).
- Neutrals (sage-tinted, replaces plum neutrals): text primary `#eef2e8`, secondary `#c4cfb6`, muted `#8a977c`, hairline border `#26301f`, strong border `#3a4830`.
- Primary (Yoda-skin green, interactive): base `#6f9c4f`, hover `#7fb05c`, deep `#4f7338`, soft tint `#a9c98f`. Build the full 50–950 scale around these anchors.
- Accent (lightsaber green, scarce — same ≤5% budget as gold in plum): `--agt-color-accent-400: #7CFC5A`-class vivid green (tune so it's vivid but not neon-blurry on dark), muted `#5dbd45`; `--agt-on-accent: #0c1a08` (near-black green — NEVER white on lightsaber green). Used for: active nav edge, primary CTA fill, focus rings, selected states, brand mark.
- Warm secondary (robe beige, sparing): `#d8c9a3` for subtle highlights (e.g. page-title underline, badge variant); text on beige = `#2e2718`.
- Semantic: success can NOT be the accent green (would collide) — use a distinct `#3fa66a`; warning amber `#d9a13b`; danger `#d4553f` (Sith-red flavored but AA-checked); info steel `#5b8fa8`. Verify all against the swamp surfaces.
- Chart series: primary green, lightsaber accent, beige, deep green, muted sage, info steel — check distinguishability and document a color-blind-safe order.

## 2. Light variant (`dagobah-light`)

Paper with green ink: canvas `#f7f9f3`, white surfaces, sage borders, headings deep green `#2c4020`, interaction `#4f7338`, accent used as edges/fills with `--agt-on-accent` dark text (vivid green text on white fails AA — text-use fallback `#3c7a2a`+, verify ratio).

## 3. Implementation

- `wwwroot/css/themes/agt-theme.dagobah.css` scoped to `[data-agt-theme="dagobah-dark"]` and `[data-agt-theme="dagobah-light"]`.
- Register in `AgtUiOptions.AvailableThemes` and the AgtThemeSwitcher (display name "Dagobah").
- Full token parity with plum (run the missing-token check from prompt 10); focus rings, glow tokens (subtle green glow replaces purple), glass tokens (swamp-green glass), scrollbars, on-accent — all defined.
- WCAG 2.2 AA: add every pair to docs/A11Y-CONTRAST.md; the vivid accent gets dark text everywhere.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (catalog smoke tests also run under dagobah-dark). Walk key pages (buttons, forms, datagrid, pickers/popups, navigation, charts) in both dagobah variants; anything that still shows purple or gold indicates a hard-coded value or missing token — fix at the source. Report the palette table with final hex values and contrast ratios.
