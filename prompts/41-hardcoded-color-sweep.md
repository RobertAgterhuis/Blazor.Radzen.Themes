# Prompt 41 — Hardcoded-kleuren sweep: audit-gaten dichten én alles fixen

Finding: the token-bleed audit (eng/token-audit + TokenBleedAuditTests) passes GREEN while at least 28 raw hex literals exist outside token/theme files — 26 in `src/Agterhuis.Ui/Components/Layout/AgtThemeSwitcher.razor.css` (the per-family swatch colors) and 2 in catalog pages (`SelectionInputsCatalog.razor`, `FormsAdvancedCatalog.razor`). So the audit has scope gaps. Fix the DETECTOR first, then every finding, so themes apply cleanly everywhere.

---

Copy below into Claude Code in the repo root:

---

## 1. Dicht de audit-gaten (waarom stond hij groen?)

Inspect `eng/token-audit` (RepositoryLayout/TokenAuditEngine): determine exactly which files it scans. Close every gap so the scan covers ALL shipped styling/markup/code:
- `**/*.razor.css` (CSS isolation — the known miss), `wwwroot/**/*.css` incl. demo `app.css`, theme partials (`css/theme/_*.css` — deze mogen alleen `var(--...)` bevatten), `**/*.razor` (inline `style=`, attribute values, SVG `fill`/`stroke`), `**/*.cs` (kleur-strings, chart-palettes), `theme-interop.js`.
- Detectiepatronen verbreden: `#rgb`/`#rrggbb`/`#rrggbbaa`, `rgb()/rgba()/hsl()/hsla()/oklch()`, CSS named colors (behalve `transparent`/`currentColor`/`inherit`/`none`), gradients met literals, `box-shadow` met literals.
- Schrijf een regressietest die de NU bekende 28 gevallen zou hebben gevangen (fixture-achtig: plant een tijdelijk proefbestand met een hex en assert dat de audit hem meldt; ruim op).
- De allowlist (nu 2 regels) krijgt het formaat pad+patroon+reden; elke entry gemotiveerd.

## 2. Fix de bekende vondsten — bij de bron

- **AgtThemeSwitcher.razor.css (26 hexes)**: dit zijn de preview-swatches van de theme-OPTIES — die moeten de kleuren van de DOELfamilie tonen terwijl een ander theme actief is, dus theme-scoped tokens werken hier niet via cascade. Juiste oplossing: één bron in C# — breid het `AgtTheme` record uit met `PreviewCanvas`/`PreviewPrimary`/`PreviewAccent` (hex-strings per familie, naast de bestaande metadata) en render de swatches met inline style vanuit die metadata. Verwijder alle 26 CSS-hexes. De audit krijgt één gemotiveerde allowlist-regel: kleurliteralen zijn UITSLUITEND toegestaan in `AgtTheme`-metadata (en de token/theme-css zelf). Voeg een test toe: elke geregistreerde familie heeft drie preview-kleuren, en die matchen de canvas/primary/accent-tokens van de bijbehorende theme-scope (parse de theme-css — zo kunnen preview en werkelijkheid nooit uiteenlopen).
- **De 2 catalog-pagina's**: vervang door token-referenties (of de juiste component-parameter); geen inline literals in demo-pagina's.

## 3. Volledige sweep na het dichten

Run the hardened audit over the whole solution; fix EVERY new finding per the decision tree (juiste token gebruiken → tokenwaarde aanpassen in alle theme-scopes → nieuw gepaard token met pariteit). Verwacht extra vondsten in: showcase-css (app.css secties), nieuwe componenten uit de enterprise-polish (drawer, command palette, chips, skeletons), Home/Theme-pagina css, chart-configuratie in C#. Nul on-geallowliste violaties is de eis.

## 4. Borging

- `TokenBleedAuditTests` faalt vanaf nu op elk van de bovenstaande patronen/paden (bewijs: de fixture-test uit §1).
- Draai ook de runtime-probe en de contrast-sweep na de fixes (kleuren die naar tokens verhuizen kunnen per theme licht verschuiven — meet).
- Noteer in CONTRIBUTING/copilot-instructions dat `AgtTheme`-metadata de enige toegestane plek voor kleurliteralen buiten token/theme-css is.

## 5. Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen — inclusief de aangescherpte audit (nul violaties), de preview-vs-token match-test en de fixture-regressietest. Switcher-swatches tonen nog steeds de juiste familiekleuren in elke actieve theme. Rapporteer: welke scan-gaten de audit had, totaal gevonden literalen per categorie ná verbreding, en per groep de gekozen fix.
