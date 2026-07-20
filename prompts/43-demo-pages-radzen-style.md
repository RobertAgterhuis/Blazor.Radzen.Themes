# Prompt 43 — DataGrid-bug + demopagina's naar blazor.radzen.com-niveau

Observed (imperial-light, /components/data/data-grid): the AgtDataGrid renders MISALIGNED — the "Naam" column shows empty cells while the name values shift into the wrong visual column and scores float right. Separately: our demo pages show one bare example each, while blazor.radzen.com gives per component multiple worked examples with live demo + source code + API info. Adopt that pattern.

---

Copy below into Claude Code in the repo root:

---

# PART A — Fix de DataGrid-kolomverschuiving (eerst)

Reproduce on /components/data/data-grid (meerdere themes en beide dichtheden). Diagnose: kolomdefinities vs. gerenderde cellen (een lege eerste kolom suggereert een frozen-column/hidden-column artefact, een template-kolom zonder Property, of CSS die de eerste kolom verbergt/verschuift — check ook onze `_datagrid.css` overrides op width/transform-regels die kolommen raken). Fix bij de bron; controleer daarna ALLE grids (catalogus-datapagina's, showcase werkorders-grid) op hetzelfde effect. bUnit-test: gerenderde headervolgorde matcht celvolgorde per rij.

# PART B — Demopagina-framework à la blazor.radzen.com

## B1. Herbruikbaar voorbeeld-framework (bouw dit één keer)

- `DemoExample` component: titel, korte uitleg (1–3 zinnen), live demo-area, en tabs **Voorbeeld | Code** — de Code-tab toont de EXACTE broncode van het voorbeeld met syntax highlighting en kopieerknop. Implementatie: elk voorbeeld is een eigen kleine `.razor`-file; embed de bron build-time (EmbeddedResource of source-generator/MSBuild-copy naar wwwroot) zodat code en demo nooit uiteenlopen — geen handmatig gedupliceerde code-strings.
- `ComponentPage` sjabloon: kicker + componentnaam + intro, dan een reeks DemoExamples, onderaan een **API-sectie**: voor Agt-wrappers een automatisch gegenereerde parametertabel (reflectie over `[Parameter]`-properties: naam, type, default, beschrijving uit XML-docs — schakel XML-doc generatie aan indien uit), voor rauwe Radzen-catalogpagina's een link naar de officiële Radzen-docs van dat component plus onze theme-notities.
- Syntax highlighting: klein en zelf-gehost (geen CDN) — bijv. highlight.js subset of een eenvoudige Razor-tokenizer; themed via tokens (code-blok-tokens bestaan al door de blog-showcase? hergebruik).

## B2. Pas het toe op ÁLLE componenten (definition of done — geen deelconversie)

Dit is geen pilot: ELK component krijgt zijn eigen pagina in het nieuwe sjabloon, zoals blazor.radzen.com dat voor elk van zijn 145+ componenten doet. Concreet:

- **Eén component = één pagina.** De huidige verzamel-catalogpagina's per familie ("Text Inputs", "Pickers", "Overlays"...) worden opgesplitst: elk standalone component uit docs/RADZEN-COMPONENT-INVENTORY.md krijgt een eigen route (bijv. /catalog/textbox, /catalog/autocomplete, /catalog/scheduler) met het ComponentPage-sjabloon. Child-componenten (kolommen, items) horen bij hun parent-pagina. De navigatie toont ze per categorie uitklapbaar — zoals de Radzen-site-navigatie (DataGrid, Data, Forms, Navigation, Layout, Feedback, Validators, Data Visualization...).
- **Per pagina zoveel benoemde voorbeelden als er wezenlijk verschillende mogelijkheden zijn** (minimaal 1) met code-tab; datarijke componenten (DataGrid, Scheduler, Gantt, PivotGrid, Tree, Charts) krijgen er meer wanneer de capability-set dat vraagt.
- **Agt-wrapperpagina's** volgen hetzelfde sjabloon (met parametertabel); wrapper- en catalogpagina van hetzelfde component kruisverwijzen zoals nu.
- **Voortgang is meetbaar**: breid docs/RADZEN-COMPONENT-INVENTORY.md uit met een kolom "demopagina-sjabloon" en werk tot ELKE rij ✓ heeft. Dit mag in gefaseerde commits (categorie voor categorie), maar de prompt is pas klaar bij 100% — rapporteer per categorie de teller.
- De oude verzamelpagina's blijven als dunne index-pagina's per categorie (linken naar de componentpagina's) zodat bestaande routes niet breken; smoke tests bijwerken naar de nieuwe routes.

## B3. Kwaliteit

Framework en voorbeelden token-only (bleed/contrast-guards); code-tabs toegankelijk (tabs-semantiek, kopieerknop met aria-label + toast); voorbeelden werken in alle families en beide dichtheden; mobiel: code-tab horizontaal scrollbaar zonder layout-breuk. bUnit: DemoExample rendert demo én code, parametertabel genereert rijen voor een bekende wrapper.

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen (incl. de nieuwe grid-kolomtest en smoke tests voor ALLE nieuwe componentroutes). Definition of done: docs/RADZEN-COMPONENT-INVENTORY.md heeft nul rijen zonder ✓ in de demopagina-kolom — elk standalone component heeft een eigen pagina met capability-gedreven voorbeelden (minimaal 1) en werkende code-tabs. Steekproef: DataGrid (7+ voorbeelden, kolommen correct in drie families/beide dichtheden), Scheduler, TextBox, Menu en één validator in het nieuwe sjabloon; parametertabellen kloppen met de werkelijke API; navigatie toont alle categorieën uitklapbaar zoals de Radzen-site. Rapporteer: de oorzaak van de kolomverschuiving, hoe de bron-embedding werkt, en de teller per categorie (moet overal 100% zijn).
