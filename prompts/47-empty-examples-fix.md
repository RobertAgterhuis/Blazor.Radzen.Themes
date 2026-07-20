# Prompt 47 — Lege voorbeelden: scannen en corrigeren naar werkende demo's

Observed (o.a. /catalog/accordion, autotaalglas-dark): example cards render with title, description and Voorbeeld/Code tabs — but the DEMO AREA IS EMPTY. No accordion visible, while blazor.radzen.com/accordion shows working panels. This must be scanned repo-wide and fixed: every example renders a WORKING, visible component.

---

Copy below into Claude Code in the repo root:

---

## 1. Mechanische scan (bouw op de bestaande Playwright-infra)

`eng/example-scan/` (npm-script + rapport `docs/EXAMPLE-SCAN.md`):
- Loop ALLE componentpagina's (Agt + catalogus, uit de inventaris-routes) in één theme; per DemoExample op de pagina: activeer de Voorbeeld-tab en assert dat het demo-gebied ECHTE inhoud rendert. Detectie-criteria (alle drie): (a) het gebied bevat ≥1 element met een component-rootclass (`.rz-*` of `agt-*`), (b) gerenderde hoogte ≥ 40px, (c) geen zichtbare foutmelding/lege string. Interactieve popup-componenten (dropdown, datepicker): assert de trigger zichtbaar; klik en assert dat het paneel opent.
- Vang ook stille rendercrashes: console-errors per pagina loggen; een voorbeeld dat een exception gooit en leeg blijft is een violatie.
- Output: per pagina per voorbeeld [OK | LEEG | ERROR] + telling. De scan wordt een blijvend npm-script naast de contrast-sweep; documenteer in de release-routine.

## 2. Diagnose-volgorde per leeg voorbeeld (fix bij de bron)

Waarschijnlijke oorzaken — check in deze volgorde en rapporteer welke het was (per groep):
a) **Voorbeeld-bestand rendert niets**: het embedded example-.razor bevat alleen code voor de Code-tab maar de demo-slot krijgt geen (of een lege) instantie — de DemoExample-koppeling demo↔bron is dan structureel kapot voor die pagina's; fix in het framework, niet per pagina.
b) **Component vereist children/data die ontbreken**: Accordion zonder `RadzenAccordionItem`s, grid zonder Data, tabs zonder items → vul werkende sample-content (kijk naar de opbouw van het Radzen-demovoorbeeld voor het equivalent als referentie voor wat een goed voorbeeld toont: single expand, multiple expand, dynamische items, events — neem die capability-assen over waar relevant, conform prompt 46).
c) **Stille exception** (parameter-fout, null-data): fix de fout; de smoke tests horen dit te vangen — als ze het niet vingen, versterk de bestaande render-smoke zodat een exception in een voorbeeld de test laat falen.
d) **CSS verbergt de inhoud** (height 0/overflow/token-issue in bepaalde families): fix aan token-bron en check dezelfde pagina in twee andere families.

## 3. Kwaliteit van de gevulde voorbeelden

Geen placeholder-vulling: elk gerepareerd voorbeeld moet zijn titel waarmaken ("Met iconen" toont echt iconen, "Template header" echt een headertemplate) — de titel-vs-code-heuristiek uit de voorbeelden-audit (prompt 46) bewaakt dit; draai die guard mee. Sample-content in het Nederlands, realistisch, per voorbeeld variërend.

## 4. Klein meegenomen

De kruisverwijzing bovenaan de catalogpagina's staat krap tegen de kicker geplakt ("Voor gebruik in applicaties..." direct boven NAVIGATION & ACTIONS): geef hem zijn plek volgens prompt 36 §3 (bescheiden regel ONDER het paginakop-blok, niet erboven).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (versterkte smoke: rendercrash in voorbeeld = fail). Example-scan draait op NUL lege voorbeelden over alle pagina's. Steekproef handmatig in twee families: Accordion (single/multiple expand zichtbaar werkend), één picker (popup opent), één datacomponent (rijen zichtbaar), één Agt-wrapper. Rapporteer: totaal gescand, aantal leeg/error per oorzaak-categorie (a–d), en de structurele fixes in het framework.
