# Prompt 29 — "Autotaalglas" theme (corporate brand, licht-eerst)

Brand palette (all EIGHT colors must be used, each with a defined role):
`#002575` (primary navy — THE primary), `#003b87` (deep blue), `#005fc5` (bright blue), `#00b5e2` (cyan), `#e4002b` (brand red), `#3fb400` (green), `#e6e6e6` (light gray), `#ffffff` (white).

Light is the hero variant (corporate white/blue); dark is a derived "navy night". Internal theme name "autotaalglas". Token values only per docs/THEMING.md; parity, bleed-audit and a11y guard tests must pass.

---

Copy below into Claude Code in the repo root:

---

Add theme `autotaalglas-light` (hero) plus `autotaalglas-dark` to Agterhuis.Ui following docs/THEMING.md.

## 1. Role mapping (all eight brand colors)

- **Primary (interactive)**: `#002575` base — build the full 50–950 scale around it with `#003b87` as the 600/700-class step and `#005fc5` as the 400/hover-class step (verify the exact ordering by lightness). Primary fills get white text (measure: white on `#002575` passes easily).
- **Accent (scarce, ≤5%)**: brand red `#e4002b` — active nav edge, primary CTA, focus rings, selected states, brand mark. `--agt-on-accent`: MEASURE white vs near-black on `#e4002b` (white is borderline ~4:1) — for TEXT on red either darken the red fill variant until white passes, or use large/bold text only; decide by measurement and document it.
- **Info**: cyan `#00b5e2` — with dark text on cyan fills (white on cyan fails).
- **Success**: green `#3fb400` — dark text on green fills (measure; `#3fb400` with white is borderline).
- **Danger vs accent — THE collision here**: danger and brand red share the hue. Resolve explicitly: danger uses a distinct darker red (e.g. `#b3001f`-class derived token) AND always icon+text; verify danger buttons vs red accent CTAs side-by-side on the buttons page.
- **Warning**: no brand color exists — derive a compatible amber (`#e8a13d`-class) as a theme token; note in THEMING.md that warning is derived, not brand.
- **Neutrals**: `#ffffff` canvas/surfaces, `#e6e6e6` as the border/divider anchor and surface-tint base (derive the gray steps around it); text = navy-black derived from `#002575` (e.g. headings `#0d1b3d`-class), body dark gray-blue, muted mid gray-blue — all AA-measured.

## 2. `autotaalglas-light` (hero)

Canvas white with the faintest cool tint, white surfaces, `#e6e6e6`-anchored borders; headings deep navy; interaction `#002575`, hover `#003b87`; nav active = light blue tint bg (primary alpha-8) + red left edge + navy text; links `#005fc5` (verify 4.5:1 on white — likely needs the darker `#003b87` for small text; measure). Corporate-clean personality: modest radius (6px), soft cool shadows, atmosphere = barely-there cool top-light, glass subtle on floating layers only. Chart series: `#002575`, `#e4002b`, `#00b5e2`, `#005fc5`, `#3fb400`, derived amber — blue/red lead (color-blind-safe pair); document order.

## 3. `autotaalglas-dark` ("navy night")

Canvas `#0a1228`-class (derived from the navy, not pure black), surfaces stepping `#0e1832` → `#132043` → `#1c2b56`-class; text near-white with blue-gray secondary/muted; primary brightens to `#4d8fe0`-class for links/interactive text (navy `#002575` fails on dark — interactive TEXT must be the bright step, fills may stay deep navy with white text); accent red stays `#e4002b` with the measured on-accent; cyan/green/danger re-measured against dark surfaces. Glow = cool blue; scrollbars, focus, chips, header, hero, nav idle (transparent!) — full token set.

## 4. Implementation

- `wwwroot/css/themes/agt-theme.autotaalglas.css` scoped to `[data-agt-theme="autotaalglas-light"]` / `"autotaalglas-dark"`; colors only in theme scopes.
- Register in `AgtUiOptions.AvailableThemes` + AgtThemeSwitcher (display name "Autotaalglas"); default variant = light; Home hero tagline (e.g. "Corporate blauw met één rood signaal.").
- Full parity incl. personality tokens; WCAG pairs into docs/A11Y-CONTRAST.md. Explicit measurements required: white-on-red, white-on-green, white-on-cyan (all borderline), links on white, bright-blue text on navy-night surfaces, `#e6e6e6` borders ≥ 3:1 against white where they carry component boundaries (they won't — use a darker derived border token for functional boundaries and keep `#e6e6e6` for subtle dividers; document).

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity, bleed, catalog smoke under both variants). Walk Home, Buttons (danger vs red accent side-by-side!), forms, DataGrid, pickers/popups, navigation and the Werkorders showcase in both variants; switch from other families — no leftovers. Report: the palette table mapping all eight brand colors to their roles with final hex values and contrast ratios, plus where the red budget went.
