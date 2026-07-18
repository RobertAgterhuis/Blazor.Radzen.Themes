# Prompt 14 — "Hoth" theme (ijsplaneet, donker + sneeuw-licht)

A Star Wars-inspired theme on Hoth's palette: polar-night canvas, ice-blue neutrals, glacial blue as primary, snowspeeder/rebel-pilot orange as the scarce accent, ice-silver for cool highlights. Internal theme name "hoth" — no copyrighted names/assets/fonts in shipped code. Token values only, per docs/THEMING.md. Requires the parity/scoping fixes from prompt 13 to be in place.

---

Copy below into Claude Code in the repo root:

---

Add theme `hoth-dark` plus `hoth-light` to Agterhuis.Ui following docs/THEMING.md. Full token parity with the other themes (the parity test must pass). Note: unlike the other themes, the LIGHT variant is a first-class citizen here (Hoth is a snowfield) — both variants get equal polish.

## 1. Palette — `hoth-dark` (polar night)

- Canvas/surfaces: `--agt-surface-0: #0b1017`, `surface-1: #0f151d`, `surface-2: #131b25`, `surface-3: #1d2a38` (hover/active).
- Neutrals (ice-tinted): text primary `#e8eef4`, secondary `#c0cdd8`, muted `#8296a8`, hairline border `#1e2a36`, strong `#31445a`.
- Primary (glacial blue, interactive): base `#4d8fc4`, hover `#5ea1d6`, deep `#35678f`, soft tint `#a9cbe6`. Build the 50–950 scale around these anchors.
- Accent (rebel-pilot orange, scarce — ≤5% budget): vivid `--agt-color-accent-400: #ff8c42`-class, muted `#d97a35`; `--agt-on-accent: #221100` (near-black — never white on vivid orange). Used for: active nav edge, primary CTA, focus rings, selected states, brand mark.
- Cool secondary (ice-silver, sparing): `#dce8f2` for subtle highlights (page-title underline, badge variant); text on ice-silver `#16212c`.
- Semantic — two collision risks in this palette, resolve both visibly: warning must not look like the orange accent (push warning toward yellow `#e0c23a`-class and verify side-by-side with accent on the buttons page); info must not look like primary blue (use a clearly distinct teal `#3aa6a0`-class). Success `#3fa66a`, danger `#d4553f`. All AA-checked on the dark surfaces.
- Chart series: glacial blue, orange accent, ice-silver, deep blue, teal info, muted slate — verify distinguishability, document a color-blind-safe order (blue/orange is a strong protan/deutan-safe base pair — lead with it).

## 2. Palette — `hoth-light` (snowfield, first-class)

- Canvas `#f6f9fc` with the faintest cool tint, white surfaces, ice borders `#dfe8f0`/`#b9cad8`; headings deep arctic `#1e3a52`; body `#243442`; muted `#5d7183`.
- Interaction `#35678f`, hover `#2a5375`; nav active = ice tint `#e7f0f8` bg + orange left edge + dark text.
- Accent orange as edges/fills only with `--agt-on-accent`; orange as TEXT on white needs the darker `#b35a1f`+ (verify ≥ 4.5:1).
- Snow-specific care: white-on-white is the failure mode here — every surface boundary needs a visible border or shadow (reuse the purple-tinted-shadow recipe from plum-light but cool-tinted), and disabled states must stay perceivable on white.

## 3. Implementation

- `wwwroot/css/themes/agt-theme.hoth.css` scoped to `[data-agt-theme="hoth-dark"]` / `"hoth-light"`; colors ONLY inside theme scopes.
- Register in `AgtUiOptions.AvailableThemes` and AgtThemeSwitcher (display name "Hoth").
- Full token set: focus rings, glow (cold blue glow), glass (frosted ice glass — this theme wears glassmorphism well, but same floating-layers-only discipline), scrollbars, on-accent.
- WCAG 2.2 AA: all pairs into docs/A11Y-CONTRAST.md.

## 4. Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (token-parity test + catalog smoke tests under hoth-dark AND hoth-light). Switch Plum → Dagobah → Hoth on Text Inputs, Buttons, DataGrid, Pickers (open popups), Navigation: everything re-themes instantly, no leftover purple/gold/green. Specifically check: warning vs accent-orange distinguishable, info vs primary-blue distinguishable, hoth-light has no white-on-white surfaces. Report the palette table with final hex values and contrast ratios.
