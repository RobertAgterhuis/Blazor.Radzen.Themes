# Prompt 31 — Drie extra Autotaalglas-families: contrast, portal, mono

Three additional theme families derived from the same eight Autotaalglas brand colors, each its own family with light+dark variants (NO third axis — the family/variant architecture stays as is, so parity, switcher and guard tests just work). Requires prompt 29 (autotaalglas base family) to be landed; reuse its role mapping and measured values as the starting point.

Brand colors: `#002575` `#003b87` `#005fc5` `#00b5e2` `#e4002b` `#3fb400` `#e6e6e6` `#ffffff`.

---

Copy below into Claude Code in the repo root:

---

Add three theme families to Agterhuis.Ui per docs/THEMING.md: `autotaalglas-contrast`, `autotaalglas-portal`, `autotaalglas-mono` — each `-light` and `-dark`. Full token parity; bleed audit clean; all guard tests green.

## 1. `autotaalglas-contrast` — toegankelijkheidsvariant (AAA-gericht)

Purpose: demonstrable high-contrast variant for WCAG/EN 301 549 contexts.
- Light: pure white canvas, text `#0d1b3d`-class near-black navy; ALL text ≥ 7:1 (AAA) where feasible, minimum 4.5:1 for large text; interactive `#002575` only (drop the lighter blues for text roles); borders: strong 2px on inputs/controls (≥3:1 always), no `#e6e6e6`-only boundaries.
- Dark: near-black navy canvas, near-white text at AAA ratios; interactive text at the bright step only.
- Personality: NO glass, NO atmosphere, NO glow, shadows minimal (borders carry elevation); focus ring 3px double-ring (inner accent, outer canvas) so it survives any background; underline links always; radius modest.
- Accent red only where it passes the stricter ratios — otherwise the darkened red text-variant from prompt 29.
- Integration: document (THEMING.md) how a consumer auto-switches to this family under `@media (prefers-contrast: more)` and offer it via `AgtUiOptions`; the switcher shows it with an accessibility hint in the option row.
- Add AAA columns to docs/A11Y-CONTRAST.md for this family.

## 2. `autotaalglas-portal` — klantgerichte variant (cyaan-voorwaarts)

Purpose: friendlier customer-facing register (afspraak maken, status volgen) vs. the corporate employee look.
- Light (hero): white canvas with a subtle cool-cyan tinted atmosphere; PRIMARY shifts to bright blue `#005fc5` (interactive), with cyan `#00b5e2` promoted to a supporting highlight role (section accents, illustrations, selected tints — dark text on cyan, always); navy `#002575` recedes to headings/footer anchor; accent stays brand red (≤5%, CTA/focus/active) — measure text-on-red per prompt 29.
- Dark: derived cyan-tinted navy night; interactive text at bright blue/cyan steps (measured).
- Personality: rounder radius (10–12px cards), softer larger shadows, slightly more generous spacing tokens if the token set allows (density stays a structural token — only adjust if per-theme spacing personality tokens already exist; do NOT introduce a new axis for this), friendlier display font pairing from the bundled set.
- Semantic set unchanged (green success, derived amber warning, darkened red danger — the danger/accent side-by-side check applies here too since red remains accent).
- Info-vs-primary collision NEW in this family: info was cyan, but cyan is now decorative — shift info to a measured distinct blue-gray or keep cyan for info and constrain decorative cyan usage; decide, document, and verify the buttons/alerts pages show a visible difference.

## 3. `autotaalglas-mono` — monochroom blauw (rapportage/zakelijk)

Purpose: the Imperial-recipe applied to the brand — blues and grays only; red exists ONLY as danger.
- Light: white canvas, gray steps anchored on `#e6e6e6` (plus the darker functional border token), primary `#002575`, hierarchy through navy/blue/gray steps; NO accent color — the "accent" token maps to `#003b87` deep blue (nav edge, CTA, focus) so parity holds without red.
- Dark: navy-night canvas; same monochrome discipline at measured bright steps.
- Danger = the darkened brand red (icon+text rule mandatory — it is now the ONLY red on screen, side-by-side check against primary CTA still required); success/warning/info keep semantic hues but desaturated one step so they whisper rather than pop (measure AA).
- Chart series: monochrome-first (navy, bright blue, cyan muted, gray steps) with semantic hues reserved for semantic series; document color-blind ordering (mono charts rely on markers/patterns — enable per the 1.4.1 rule).
- Personality: sharpest of the three — Imperial-like radius (4px), hard cool shadows, no atmosphere motion.

## 4. Implementation (all three)

- One css file per family under `wwwroot/css/themes/`, colors only in theme scopes.
- Register in `AgtUiOptions.AvailableThemes` + switcher (display names "Autotaalglas Contrast", "Autotaalglas Portal", "Autotaalglas Mono"); Home hero taglines per family; swatch strips show each family's own colors.
- Full parity incl. personality tokens (parity test enforces); WCAG pairs (and AAA for contrast-family) into docs/A11Y-CONTRAST.md; THEMING.md gets a short "brand sub-families" section explaining when to pick which (medewerker / klant / toegankelijkheid / rapportage).

## 5. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (parity, bleed, catalog smoke per new family dark variant + light spot-checks). Walk Buttons (danger checks per family), forms, DataGrid, pickers/popups, navigation, Home hero and the Werkorders showcase in all three families, both variants. Switch across all four Autotaalglas families in sequence — each must be recognizably different at a glance (contrast = stark, portal = fris cyaan, mono = staalblauw) while unmistakably the same brand. Report palette tables + contrast ratios per family and the decisions made on the flagged collisions (portal info-vs-cyan, mono danger-vs-primary).
