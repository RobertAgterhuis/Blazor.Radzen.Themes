# Prompt 40 — "MS365" theme (Microsoft 365 admin center look & feel)

A theme family modeled on admin.microsoft.com / Fluent 2: friendlier than Azure — white card-based canvas, Fluent-2 brand blue, soft shadows, rounder corners, blue app-bar chrome. Style/colors only — no Microsoft logos, icons, wordmarks or assets (IP); internal theme name "ms365". Token values only per docs/THEMING.md; parity, bleed and contrast guards must pass. NB: dit theme moet duidelijk verschillen van "azure" (prompt 39): ronder, zachter, kaart-gedreven, blauwe (niet zwarte) chrome.

---

Copy below into Claude Code in the repo root:

---

Add theme `ms365-light` (hero) plus `ms365-dark` to Agterhuis.Ui following docs/THEMING.md.

## 1. Palet — `ms365-light` (admin center)

- Canvas: neutraal `#fafafa`; surfaces wit `#ffffff` als KAARTEN (soft shadow, zie persoonlijkheid); hover `#f5f5f5`, selected tint `#ebf3fc`; borders `#e0e0e0` subtiel + functionele borderтокен `#8f8f8f`-class (≥3:1).
- Tekst: primary `#242424`, secondary `#424242`, muted `#616161` (AA-gemeten).
- Primary (interactief, Fluent 2 brand): `#0f6cbd`, hover `#115ea3`, pressed `#0c3b5e`-class deep, soft tint `#ebf3fc`; links `#0f6cbd` (meet op wit; zo nodig `#115ea3` voor kleine tekst). Volledige 50–950 scale.
- Accent: net als azure mapt het accent-token op de brandblauw-familie (M365 gebruikt één brand-kleur voor actief/focus/CTA); `--agt-on-accent` wit (meet op `#0f6cbd`).
- Chrome: de app-bar/topbar is BRAND-BLAUW (`#0f6cbd`-class) met witte tekst/iconen in de lichte variant — het herkenbare M365-patroon (vs. azure's zwarte balk); header-tokens per scope.
- Semantiek (Fluent 2, gemeten): success `#0e700e`, warning tekst `#8a6116`-class op tint `#fff9e6`, danger `#b10e1c`-class (fills `#c50f1f` met wit — meet), info = brandblauw-tint `#ebf3fc`.
- Chart series: `#0f6cbd`, `#77b7f7`-nee, gemeten reeks: `#0f6cbd`, `#0e700e`, `#8a6116`, `#5b2d90`-class paars (M365-suite-gevoel), `#b10e1c`, `#616161`; kleurenblind-volgorde documenteren.

## 2. Palet — `ms365-dark`

- Canvas `#1a1a1a`-class, surfaces `#242424` → `#2b2b2b` → `#333333` (hover/active); borders `#3d3d3d`/`#666666`; tekst `#ffffff`/`#d6d6d6`/`#adadad`.
- Interactief blauw verheldert: `#479ef5`-class voor links/tekst (Fluent 2 dark brand — meet), fills `#0f6cbd`+wit; selected tint donker `#0f2b47`-class.
- Chrome: app-bar blijft donker-blauw getint (`#0c3b5e`-class) — herkenbaar maar niet fel; header-tokens per scope.

## 3. Persoonlijkheid — "vriendelijk kantoor"

- Radius: Fluent 2 — sm 4px, md 6px, lg 8px; `--agt-nav-radius` blijft 0 (huisregel), maar cards/dialogs/popups voelen zacht.
- Schaduwen: soft 2-laags Fluent-depth op kaarten (subtiel!) en popups — dit theme is kaart-gedreven waar azure hairline-gedreven is.
- Fonts: systeemstack `"Segoe UI Variable", "Segoe UI", -apple-system, sans-serif` (niet bundelen; zelfde notitie als azure).
- Atmosfeer: vrijwel geen — hooguit een nauwelijks zichtbare koele toplichting op de canvas; geen glow; glass alleen op popups, licht.
- Motion: standaard (vriendelijker dan azure): de bestaande 150–200ms transities blijven; geen extra.

## 4. Implementatie

`wwwroot/css/themes/agt-theme.ms365.css`, scopes `[data-agt-theme="ms365-light"]` / `"ms365-dark"`; registreren in AvailableThemes + switcher (display "MS365"); Home-hero tagline (bijv. "Kaartwit met Fluent-blauw — vriendelijk beheer."); volledige tokenpariteit; WCAG-paren naar docs/A11Y-CONTRAST.md (risico-paren: witte tekst op de blauwe app-bar-varianten, muted `#616161` op `#fafafa`, warning-amber — meet alles).

## 5. Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (parity, bleed, smoke beide varianten); contrast-sweep nul violaties. Walk Buttons, forms, DataGrid, pickers/popups, navigatie, Home en de Werkorders-showcase in beide varianten. Zet daarna azure en ms365 NAAST elkaar (switch heen en weer): ze moeten direct onderscheidbaar zijn — azure = zwart chroom, hairlines, 2px, dicht; ms365 = blauw chroom, kaarten met zachte schaduw, 6px, vriendelijk. Rapporteer palettabellen met ratio's en het side-by-side oordeel.
