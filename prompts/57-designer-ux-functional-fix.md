# Prompt 57 — Designer UX: van prototype naar werkende, intuïtieve designer

Vereist prompt 56 (completeness audit + reparaties). De designer-code bestaat, maar de ervaring is die van een onafgewerkt prototype: drag & drop werkt niet betrouwbaar, het canvas toont ruwe tekst in plaats van gerenderde componenten, visuele feedback ontbreekt, en de algehele UI mist de polish die van een design-system-product verwacht mag worden. Deze prompt maakt de designer **functioneel bruikbaar en visueel overtuigend** — geen nieuwe features, alleen het werkend en intuïtief maken van wat er al is.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Drag & drop: werkend maken

Dit is het fundament — zonder werkende drag & drop is de designer nutteloos.

### Diagnose eerst
- Open de browser-console (`F12`) en probeer een component uit het palet naar het canvas te slepen. Log ELKE fout, elke ontbrekende event-handler, elke JS-interop-fout. Dit is de startdiagnose — commit het als commentaar in het reparatie-commit-bericht.
- Controleer of `designer-interop.js` daadwerkelijk geladen wordt (geen 404, geen lege module).
- Controleer of de `ondragstart`/`ondragover`/`ondrop` events op de juiste elementen zitten (palet-items, canvas-dropzones) en of ze correct doorlinken naar de Blazor command-laag.

### Repareer de volledige flow
1. **Palet → canvas**: sleep een component uit het palet naar een lege slot op het canvas. Het component MOET verschijnen als gerenderd Blazor-component (via `DesignRenderer`/`DynamicComponent`), niet als tekst.
2. **Canvas → canvas**: sleep een bestaand component naar een andere positie. Het model moet muteren via de command-stack (undo-baar).
3. **Palet → container**: sleep een component IN een Card/Tabs/Stack-slot. De container moet visueel aangeven dat hij een drop accepteert.
4. **Verwijderen**: selecteer een component, druk Delete. Het component verdwijnt, undo brengt het terug.
5. **Dupliceren**: selecteer een component, Ctrl+D (of via command palette). Een kopie verschijnt.

### Bewijs
Na deze fase: open `/designer/edit?template=Blank`, sleep een `Text Field` uit het palet naar het canvas. Het tekstveld verschijnt als gerenderd Radzen-component. Sleep er een `Primary Button` onder. Beide componenten zijn zichtbaar, selecteerbaar, en via het property panel bewerkbaar. Dit is de minimale bar — als dit niet werkt, stop en rapporteer de blokkade.

## Fase 2 — Canvas visuele feedback

Het canvas moet de gebruiker op elk moment vertellen wat er mogelijk is.

### Dropzones
- Lege slots tonen een subtiele gestippelde rand met tekst "Sleep een component hierheen" (gebruik `--agt-text-tertiary` kleur, `--agt-border-radius-md`).
- Bij hover tijdens een drag: de dropzone krijgt een accent-achtergrondkleur (`--agt-primary` op 10% opacity) en een solide accent-rand.
- Invoeglijnen: wanneer je tussen twee bestaande componenten sleept, verschijnt een horizontale accentlijn (2px, `--agt-primary`) op de invoegpositie — boven of onder het dichtsbijzijnde component.

### Geselecteerd component
- Selectie toont een accent-outline (2px solid `--agt-primary`) rond het component.
- Linksboven op het component: een klein label met de componentnaam (bijv. "Text Field") in een accent-badge (`--agt-primary` achtergrond, wit tekst, `--agt-border-radius-sm`).
- Hover (zonder selectie): subtiele outline (`--agt-border-default`, 1px dashed).

### Lege canvas
- Wanneer het canvas geen componenten bevat: toon `AgtEmptyState` met icoon, titel "Begin met ontwerpen" en subtitel "Sleep componenten uit het palet of kies een patroon als startpunt", plus een knop "Kies een patroon" die terugnavigert naar het startscherm.

### Container-feedback
- Containers (Card, Tabs, Stack) tonen hun slots als benoemde dropzones: "ChildContent", "Header", etc. met een label in `--agt-text-tertiary`.
- Bij hover tijdens drag over een container-slot: dezelfde accent-highlight als reguliere dropzones.

## Fase 3 — Palet polish

### Visuele verbeteringen
- Elke categorie-header (`DATA & SCHEDULING`, `FORMS & INPUTS`, etc.) krijgt een subtiele scheidingslijn en consistent kapitaalgebruik (sentence case: "Data & scheduling").
- Palet-items krijgen een hover-state: lichte achtergrondkleur (`--agt-surface-hover`).
- Palet-items tonen een drag-cursor (`cursor: grab`, `cursor: grabbing` tijdens slepen).
- Het filterinvoerveld krijgt een zoekicoon en een clear-knop.
- Tijdens het slepen van een palet-item: toon een semi-transparante "ghost" van het item dat gesleept wordt (HTML5 DnD `setDragImage` via de interop).

### Gedrag
- Categorieën zijn inklapbaar (standaard open). Ingeklapte staat wordt onthouden in localStorage.
- Als het filter actief is: toon alleen matching items, verberg lege categorieën.

## Fase 4 — Property panel interactie

### Selectie-koppeling
- Bij selectie van een component op het canvas: het property panel wisselt van "Pagina-eigenschappen" naar de eigenschappen van het geselecteerde component.
- De paneel-header toont de componentnaam en het componenticoon.
- Bij deselectie (Escape of klik op leeg canvas): terug naar pagina-eigenschappen.

### Visuele verbeteringen
- Parametergroepen als inklapbare secties (standaard open voor "Algemeen" en "Toegankelijkheid", ingeklapt voor "Overig").
- Afwijkende waarden (niet-default) visueel gemarkeerd: een accent-stip of vetgedrukt label.
- Voldoende padding en spacing tussen velden (minimaal `--agt-spacing-md`).

### Live preview
- Elke wijziging in het property panel rendert DIRECT mee op het canvas. Geen "Apply"-knop nodig — het model muteert via de command-stack, de renderer reageert.

## Fase 5 — Structuurboom polish

- De boom links-onder (of als inklapbaar paneel onder het palet) toont de node-hiërarchie met componenticonen.
- Selectie in de boom = selectie op het canvas (bidirectioneel).
- Rechter-muisklik op een boom-item: contextmenu met Dupliceren, Verwijderen, Omhoog/Omlaag verplaatsen.
- Slepen binnen de boom verplaatst nodes (alternatief voor canvas-drag als precisie nodig is).
- Lege boom: toon dezelfde "Sleep componenten" hint als het lege canvas.

## Fase 6 — Toolbar en werkbalk polish

- Groepeer toolbar-acties visueel: [Nieuw | Openen | Opslaan] — [Undo | Redo] — [Viewport: Mobiel | Tablet | Desktop] — [Canvas-thema].
- Gebruik `AgtButton` met `ButtonStyle.Secondary` en iconen (geen platte tekst-links).
- "Opslaan" is disabled wanneer er geen wijzigingen zijn (dirty-state); "Undo" disabled wanneer de stack leeg is.
- Viewport-schakelaar als toggle-groep met de breedte in pixels als tooltip (360 / 768 / 1280).
- Canvas-thema dropdown toont de thema-kleuren als preview-swatch naast de naam.

## Fase 7 — Startscherm polish

Het startscherm (`/designer`) is het eerste wat de gebruiker ziet.

- **Recent-sectie**: kaarten met projectnaam, laatst bewerkt datum, en een thumbnail (eerste pagina gerenderd als kleine preview — mag een placeholder zijn als rendering te complex is). Hover: schaduw-effect. Klik opent het project.
- **Patronen-sectie**: visuele kaarten per patroon (niet platte tekst-knoppen). Elk patroon krijgt een schematische preview-afbeelding (SVG wireframe die de layoutstructuur toont: formulier = invoervelden-blokken, CRUD = tabel + knoppen, master-detail = twee panelen, etc.) plus titel en korte beschrijving.
- **Importeren**: duidelijke "Importeer .agtdesign bestand" knop met upload-icoon.
- Layout: CSS grid, responsive (patronen wrappen naar meerdere rijen op smaller scherm).

## Fase 8 — Monaco Editor als live werkende code-editor

De huidige code-tab gebruikt een textarea — dat wordt vervangen door een volwaardige Monaco Editor die LIVE synchroon loopt met het canvas.

### Installatie en hosting
- Monaco Editor installeren via npm (zelf-gehost, NIET via CDN — huisregel). Assets lazy-loaden: Monaco wordt pas geladen wanneer de gebruiker het code-paneel opent of vergroot.
- Monaco-theme volgt het actieve editor-thema: lichte families → `vs` theme, donkere families → `vs-dark` theme. Wissel mee bij theme-switch.

### Code-tab (Razor): tweerichtings live synchronisatie
- De Code-tab toont de gegenereerde Razor-code (uit `RazorCodeGenerator`) van de actieve pagina.
- **Canvas → Monaco**: elke wijziging op het canvas (drag & drop, property panel edit, delete, undo) regenereert de Razor-code en update Monaco LIVE — geen handmatige refresh nodig.
- **Monaco → Canvas**: de Code-tab is BEWERKBAAR (niet read-only). Wanneer de gebruiker code wijzigt in Monaco en de wijziging is valide Razor die overeenkomt met het designmodel, dan wordt het model bijgewerkt en het canvas rendert de wijziging direct. Dit werkt via een PARSE-stap: de gegenereerde Razor heeft een voorspelbare structuur (componenttags met parameters) — bouw een lightweight parser die componenttags + parameters terugmapt naar `DesignNode`-mutaties.
- **Foutafhandeling**: als de Monaco-inhoud niet parsebaar is (bijv. handmatige Razor die niet in het model past), toon een subtiele waarschuwing onder de editor ("Niet alle wijzigingen konden naar het model worden vertaald") en behoud de laatste geldige modelstaat. Het canvas crasht NOOIT door ongeldige code in Monaco.
- **Selectie-koppeling**: klik op een component op het canvas → Monaco scrollt naar en highlight het bijbehorende codeblok. Klik op een componenttag in Monaco → het canvas selecteert die node.
- **Debounce**: Monaco → model synchronisatie heeft een debounce van 500ms na de laatste toetsaanslag (voorkom model-churn bij snel typen).

### Model JSON-tab: bewerkbaar met validatie
- De JSON-tab toont het volledige `DesignDocument` als JSON in Monaco met JSON-schema-validatie.
- Schema wordt als Monaco-diagnostics geregistreerd: ongeldige structuur, onbekende velden, type-mismatches tonen inline rode squiggles.
- Bewerken is direct: wijzigingen in de JSON worden na Apply (knop of Ctrl+Enter) als één command op de stack gezet (undo-baar). Het canvas en de Code-tab renderen direct de nieuwe staat.
- Als de JSON ongeldig is: Apply is disabled, de foutpositie is gemarkeerd.

### Layout-integratie
- Het code-paneel zit ONDER het canvas (niet als aparte pagina). Het is standaard ingeklapt tot een tab-balk ("Code (Razor)" | "Model (JSON)"). Klik op een tab opent het paneel; de scheidingslijn tussen canvas en code-paneel is versleepbaar (zie fase 9).
- Monaco neemt de volledige breedte van het canvas-gebied in, niet de volledige scherm-breedte (palet en property panel blijven zichtbaar).

## Fase 9 — Resizable panelen (VS Code-stijl)

De designer krijgt versleepbare scheidingslijnen tussen alle panelen, zoals in VS Code.

### Implementatie
- **Drie scheidingslijnen**:
  1. **Links**: tussen palet (+ structuurboom) en canvas — verticaal
  2. **Rechts**: tussen canvas en property panel (+ data panel) — verticaal
  3. **Onder**: tussen canvas en code-paneel (Monaco) — horizontaal

- **Interactie**: de gebruiker drukt de linker muisknop in op de scheidingslijn (5px breed, cursor `col-resize` of `row-resize`), sleept, en laat los. Het paneel aan weerszijden past direct mee in grootte (CSS grid of flexbox met inline `style` op de container).

- **JS-interop**: voeg een `designer-resize-interop.js` toe (of breid `designer-interop.js` uit) met:
  - `mousedown` op de scheidingslijn registreert de startpositie
  - `mousemove` op `document` berekent de nieuwe paneelgrootte en past de CSS-variabele / inline style aan (requestAnimationFrame voor soepelheid)
  - `mouseup` stopt het slepen en slaat de paneelgroottes op in localStorage
  - Tijdens het slepen: een semi-transparante overlay over de hele designer voorkomt dat andere elementen hover-events afvuren

- **Visuele affordance**:
  - De scheidingslijn is standaard nauwelijks zichtbaar (1px `--agt-border-default`)
  - Bij hover: de lijn wordt breder (3px) en toont de accent-kleur (`--agt-primary`)
  - Tijdens slepen: de lijn is 3px accent-kleur en de cursor verandert

- **Grenzen**: elk paneel heeft een minimum- en maximumbreedte/-hoogte:
  - Palet: min 150px, max 400px, standaard 220px
  - Property panel: min 200px, max 500px, standaard 320px
  - Code-paneel (hoogte): min 100px, max 60% van viewport-hoogte, standaard 250px
  - Canvas: neemt alle resterende ruimte (flex: 1)

- **Dubbelklik**: dubbelklik op een scheidingslijn reset het paneel naar de standaardbreedte.

- **Persistentie**: paneelgroottes worden opgeslagen in localStorage (`designer-panel-sizes`) en hersteld bij het openen van de designer. Als er geen opgeslagen waardes zijn, gebruik de standaardgroottes.

- **Inklapbaar**: het palet en property panel kunnen volledig worden ingeklapt door de scheidingslijn helemaal naar de rand te slepen (onder de minimumgrootte). Een inklapknop (chevron-icoon) in de paneel-header doet hetzelfde. Ingeklapt paneel toont alleen een smalle balk met een uitklap-chevron.

- **Reduced motion**: de resize-feedback is instant (geen animatie), dus reduced-motion is automatisch gerespecteerd.

- **Touch**: scheidingslijnen reageren ook op touch-events (touchstart/touchmove/touchend) voor tablet-gebruik.

## Fase 10 — Algehele designer-layout

- **Standaard paneelverhoudingen**: palet ±220px, canvas flex, property panel ±320px, code-paneel ±250px hoogte (alle aanpasbaar via de scheidingslijnen uit fase 9).
- **Scrolling**: palet en property panel scrollen onafhankelijk; het canvas scrollt alleen als de inhoud groter is dan de viewport; het code-paneel (Monaco) heeft eigen scrolling.
- **Kleurconsistentie**: de designer-chrome (toolbar, palet, panelen, scheidingslijnen) volgt het actieve editor-thema. Gebruik uitsluitend design-tokens — geen hardcoded kleuren.
- **Density**: de designer respecteert de compact/comfortable density-toggle. In compact mode: kleinere palet-items, minder padding, compactere property panel velden.
- **Reduced motion**: alle hover-transities en drag-animaties respecteren `prefers-reduced-motion`.

## Fase 11 — End-to-end rooktest

Na alle voorgaande fasen, voer deze handmatige test uit en rapporteer per stap:

### Startscherm & navigatie
1. Open `/designer` → startscherm toont patronen als visuele kaarten
2. Klik "Leeg ontwerp" → designer opent met leeg canvas en empty-state

### Drag & drop
3. Sleep `Row` uit palet naar canvas → rij verschijnt met lege kolom-slots
4. Sleep `Text Field` naar de kolom-slot → tekstveld verschijnt als gerenderd component
5. Klik op het tekstveld → selectie-outline + naam-badge verschijnt, property panel toont Text Field-eigenschappen
6. Wijzig "Label" in het property panel → canvas toont direct het bijgewerkte label
7. Sleep `Primary Button` onder het tekstveld → invoeg-lijn verschijnt, knop wordt geplaatst
8. Sleep `Card` naar canvas → card verschijnt met "ChildContent"-dropzone
9. Sleep het tekstveld IN de card → tekstveld verhuist naar de card (model-mutatie via command)
10. Ctrl+Z → tekstveld keert terug naar de originele positie
11. Selecteer de knop, druk Delete → knop verdwijnt, Ctrl+Z brengt hem terug
12. Structuurboom toont de juiste hiërarchie (Card > Row > TextField)

### Monaco Editor
13. Open het code-paneel (klik op "Code (Razor)" tab onder het canvas) → Monaco laadt met syntaxhighlighting, toont de gegenereerde Razor
14. Sleep een nieuw component op het canvas → Monaco-code update LIVE mee
15. Wijzig in Monaco de `Title`-parameter van de `AgtPageHeader` → canvas toont direct de nieuwe titel
16. Open de "Model (JSON)"-tab → JSON van het volledige document met schema-validatie
17. Wijzig een waarde in de JSON, druk Ctrl+Enter (Apply) → canvas en Code-tab renderen de wijziging
18. Klik op een component op het canvas → Monaco scrollt naar het bijbehorende codeblok

### Resizable panelen
19. Sleep de linker scheidingslijn (palet ↔ canvas) naar rechts → palet wordt breder, canvas smaller
20. Sleep de rechter scheidingslijn (canvas ↔ property panel) naar links → property panel wordt breder
21. Sleep de onderste scheidingslijn (canvas ↔ code-paneel) omhoog → code-paneel wordt hoger
22. Dubbelklik op de linker scheidingslijn → palet reset naar standaardbreedte
23. Ververs de pagina → paneelgroottes zijn hersteld uit localStorage

### Theme & persistentie
24. Wissel canvas-thema naar "ocean-dark" → componenten renderen in ocean-kleuren, Monaco wisselt naar donker theme
25. Klik "Opslaan" → bestand wordt opgeslagen (File System Access API of download)
26. Ververs de pagina → herstel-banner verschijnt NIET (was opgeslagen)
27. Open het opgeslagen bestand → designer toont het ontwerp correct met correcte paneelgroottes

Rapporteer elke stap als ✅ of ❌ met toelichting. Als een stap faalt, rapporteer de fout en ga door met de volgende stappen.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Alle bestaande sweeps (contrast, bleed) nul violaties over de designer-routes
- De 27-staps rooktest hierboven volledig ✅
- Geen `console.error` in de browser-console tijdens de rooktest
- Monaco-assets laden lazy (bewijs: network-tab toont geen Monaco-requests op `/designer` startscherm, wel na openen code-paneel)
- Paneelgroottes persistent na refresh (bewijs: localStorage key `designer-panel-sizes` bevat de verwachte waarden)
- Rapporteer: welke fase de meeste reparatie vergde, de totale hoeveelheid gewijzigde bestanden, en een eerlijk oordeel over de huidige designer-UX op een schaal van 1–10 (met toelichting wat er nog beter kan in een toekomstige prompt)
