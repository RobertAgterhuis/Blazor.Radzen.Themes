# Prompt 12 — "Dathomir" theme (donker, Darth Maul-kleuren)

A dark Star Wars-inspired theme on Darth Maul's palette: black-on-black surfaces with a red undertone, ash-gray neutrals, deep crimson as primary, red-lightsaber scarlet as the scarce accent, steel for cool details. Internal theme name "dathomir" — no copyrighted names/assets/fonts in shipped code. Token values only, per docs/THEMING.md.

---

Copy below into Claude Code in the repo root:

---

Add theme `dathomir-dark` (plus a serviceable `dathomir-light`) to Agterhuis.Ui following docs/THEMING.md. Full token parity with plum/dagobah.

## 1. Palette (dark = hero variant)

- Canvas/surfaces (black with faint red undertone): `--agt-surface-0: #120c0c`, `surface-1: #171010`, `surface-2: #1c1313`, `surface-3: #2a1a1a` (hover/active).
- Neutrals (ash, red-tinted): text primary `#f4eded`, secondary `#d3c2c2`, muted `#96807f`, hairline border `#302020`, strong `#4a3232`.
- Primary (deep crimson, interactive): base `#a8211c`, hover `#c02a24`, deep `#7c1714`, soft tint `#d98f8a`. Build the 50–950 scale around these anchors.
- Accent (red lightsaber, scarce — ≤5% budget): vivid scarlet `--agt-color-accent-400: #ff3b30`-class (tune against the dark canvas), muted `#d4362c`; `--agt-on-accent: #1c0605` (near-black — never white on vivid scarlet). Used for: active nav edge, primary CTA, focus rings, selected states, brand mark.
- Cool secondary (saber-hilt steel, sparing): `#aeb6bd` for subtle highlights (page-title underline, badge variant); text on steel `#1d2226`.
- Semantic — CRITICAL in a red theme: danger must stay distinguishable from primary/accent. Make danger visibly different in hue AND always paired with an icon/text (WCAG 1.4.1 rule already applies): danger `#ff8a5c`-class hot orange-red, clearly warmer than crimson — verify side-by-side on the buttons page. Success `#3fa66a`, warning `#d9a13b`, info `#5b8fa8`. All AA-checked on the dark surfaces.
- Chart series: crimson, scarlet accent, steel, deep crimson, muted ash-rose, info steel-blue — check distinguishability (a red-heavy series set is risky for deuteranopia: include steel and info-blue early in the order) and document a color-blind-safe order.

## 2. Light variant (`dathomir-light`)

Paper with crimson ink: canvas `#faf6f5`, white surfaces, ash borders, headings near-black maroon `#3a1512`, interaction `#a8211c`, accent as edges/fills only with `--agt-on-accent`... for text-on-white use `#b3271f`+ (verify ≥ 4.5:1).

## 3. Implementation

- `wwwroot/css/themes/agt-theme.dathomir.css` scoped to `[data-agt-theme="dathomir-dark"]` / `"dathomir-light"`.
- Register in `AgtUiOptions.AvailableThemes` and AgtThemeSwitcher (display name "Dathomir").
- Full token parity (run the missing-token check): focus rings, glow (deep red glow), glass (smoked red-black glass), scrollbars, on-accent.
- WCAG 2.2 AA: all pairs into docs/A11Y-CONTRAST.md. Extra attention: red-on-dark contrast is deceptive — vivid red reads darker than it measures; verify crimson interactive text on surface-0 actually passes, otherwise lighten the interactive text variant.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (catalog smoke tests under dathomir-dark). Walk buttons, forms, datagrid, pickers/popups, navigation, charts in both variants; specifically verify: danger vs primary buttons are instantly distinguishable, validation errors don't drown in the red theme, and nothing still shows purple/gold/green from other themes (hard-coded value → fix at source). Report the palette table with final hex values and contrast ratios.
