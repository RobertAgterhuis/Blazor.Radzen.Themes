# Prompt 39 — "Azure" theme (Azure Portal look & feel)

A theme family modeled on the Microsoft Azure Portal: dense tool-like UI, light "blade" canvas with hairline borders, Azure blue as interactive color, near-black chrome in dark mode, sharp corners, Segoe UI. Style/colors only — no Microsoft logos, icons or assets (IP); internal theme name "azure". Token values only per docs/THEMING.md; parity, bleed and contrast guards must pass.

---

Copy below into Claude Code in the repo root:

---

Add theme `azure-light` (hero — the portal is light-first) plus `azure-dark` to Agterhuis.Ui following docs/THEMING.md.

## 1. Palet — `azure-light` (portal-blades)

- Canvas: blade-gray `#f5f5f5`-class; surfaces white `#ffffff`; hover/selected tint `#f3f2f1`; hairline borders `#e1dfdd`, strong `#d2d0ce` — plus a darker functional border token (≥3:1) for control boundaries (`#8a8886`-class), Fluent-conform.
- Tekst: primary `#201f1e`, secondary `#484644`, muted `#605e5c` (AA-gemeten op de canvas- en surface-tinten).
- Primary (interactief): Azure-blue `#0078d4`, hover `#106ebe`, pressed/deep `#005a9e`, soft tint `#deecf9`; links `#0065b3` (meet 4.5:1 op wit). Build the full 50–950 scale around these anchors.
- Accent: in dit theme is het accent GEEN aparte kleur — map het accent-token op dezelfde Azure-blue familie (actieve nav-edge, focus, selectie, CTA allemaal blauw, zoals de portal zelf); `--agt-on-accent` = wit (meet op `#0078d4`).
- Semantiek (Fluent-waarden, AA-gemeten): success `#107c10`, warning `#797673`-nee — warning fill `#fff4ce` met tekst `#797673`? Gebruik de gangbare Fluent-set: success `#107c10`, warning `#986f0b`-class amber-tekst met `#fff4ce` tint-vlakken, danger `#a4262c` (fills `#d13438` met witte tekst — meet), info = de blauwfamilie met eigen tintvlak `#deecf9`.
- Chart series: `#0078d4`, `#40e0d0`-nee — gebruik een gemeten portal-achtige reeks: `#0078d4`, `#00188f`, `#00bcf2`, `#107c10`, `#986f0b`, `#a4262c`; kleurenblind-volgorde documenteren.

## 2. Palet — `azure-dark` (portal dark mode)

- Canvas `#1b1a19`-class near-black, surfaces `#201f1e` → `#252423` → `#323130` (hover/active); borders `#3b3a39`/`#605e5c`; tekst `#f3f2f1` / secondary `#c8c6c4` / muted `#979593`.
- Interactief blauw verheldert: `#2899f5`-class voor links/tekst (meet op de donkere surfaces), fills mogen `#0078d4` blijven met witte tekst; focus ring licht blauw.
- Chrome: de header/topbar is in BEIDE varianten donker (near-black met witte merknaam) — dat is het herkenbare portal-patroon; definieer aparte header-tokens zodat de lichte variant tóch een donkere balk heeft (de header-token bestaat al sinds de header-bleed fix — hier bewust donker in beide scopes).

## 3. Persoonlijkheid — "tool, geen brochure"

- Radius: scherp — sm 2px, md 2px, lg 4px; `--agt-nav-radius` blijft 0.
- Schaduwen: minimaal; elevatie via hairlines, alleen popups/dialogs een subtiele Fluent-depth (2-laags, laag).
- Fonts: systeemstack `"Segoe UI Variable", "Segoe UI", -apple-system, sans-serif` (NIET bundelen — Segoe is propriëtair; systeemstack met nette fallback, documenteer in THIRD-PARTY-NOTICES dat er niets gebundeld is voor dit theme).
- Dichtheid: dit theme oogt het best compact — maar dichtheid blijft een aparte as; verander er niets aan, noteer alleen in THEMING.md dat azure + compact de "portal-feel" geeft.
- Atmosfeer: GEEN gradients/glow/ambient — vlak en zakelijk; glass alleen minimaal op popups of helemaal uit (kies, documenteer); motion sober (150ms, geen spring-overshoot in dit theme als de motion-tokens per theme instelbaar zijn — anders laten).

## 4. Implementatie

`wwwroot/css/themes/agt-theme.azure.css`, scopes `[data-agt-theme="azure-light"]` / `"azure-dark"`; registreren in AvailableThemes + switcher (display "Azure"); Home-hero tagline (bijv. "Blade-grijs met Azure-blauw — tool boven alles."); volledige tokenpariteit incl. header-, nav-, hero-, on-accent- en personality-tokens; WCAG-paren naar docs/A11Y-CONTRAST.md (let op: `#605e5c` muted op `#f5f5f5` en de warning-amber zijn de risico-paren — meet).

## 5. Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (parity, bleed, smoke beide varianten); contrast-sweep nul violaties. Walk Buttons, forms, DataGrid (de portal-look leeft of sterft bij het grid: hairlines, compacte koppen), pickers/popups, navigatie, Home en de Werkorders-showcase in beide varianten — donkere header in beide, geen gradients, scherp en dicht. Rapporteer de palettabel met ratio's.
