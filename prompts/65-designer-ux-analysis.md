# Prompt 65 — Designer UX-analyse en herstelplan

Uitgebreide analyse van de WYSIWYG designer op basis van code-review van alle designer-componenten, CSS, en JavaScript. Doel: de designer bruikbaar maken voor niet-technische gebruikers die schermen via drag & drop willen bouwen.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Analyse-bevindingen

### 1. KRITIEK — CSS is gebroken na prompt 59 extractie

**Ernst: Blokkerend — de designer is visueel kapot.**

Bij de extractie in prompt 59 zijn componenten van `samples/Agterhuis.Ui.Demo/` naar `src/Agterhuis.Ui.Designer/` verplaatst, maar de bijbehorende **scoped CSS-bestanden zijn achtergebleven** in de demo:

| CSS-bestand (demo) | Component (RCL) | Status |
|---|---|---|
| `samples/.../Designer.razor.css` (618 regels) | `DesignerShell.razor` in RCL | **Orphaned** — scoped CSS cascadeert niet naar child-components in andere assemblies |
| `samples/.../Designer/DesignerCanvasNode.razor.css` (222 regels) | `DesignerCanvasNode.razor` in RCL | **Orphaned** — bestand staat naast een niet-bestaand component in de demo |

**Gevolg**: De volgende CSS-klassen zijn NIET geladen en de bijbehorende elementen zijn ongestyled:
- `.designer-canvas-node`, `.designer-canvas-node--selected`, `.designer-canvas-node__bar`, `.designer-canvas-node__preview` — canvas nodes zijn onherkenbaar
- `.designer-dropzone`, `.designer-dropzone--drag-over` — dropzones zijn onzichtbaar
- `.designer-slot`, `.designer-slot__hint`, `.designer-slot__empty` — slots tonen geen visuele hints
- `.designer-divider--vertical`, `.designer-divider--horizontal` — resize-dividers zijn onzichtbaar
- `.designer-tree__item`, `.designer-tree__item--selected` — tree items zijn ongestyled
- `.designer-breadcrumb` — breadcrumb is ongestyled
- `.designer-toolbar button` styling, hover-effecten, disabled-state — toolbar knoppen missen interactie-feedback

**Oplossing**: Verplaats alle regels uit `Designer.razor.css` en `DesignerCanvasNode.razor.css` naar `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css` (de RCL-brede stylesheet). Verwijder daarna de orphaned bestanden.

---

### 2. KRITIEK — Drag & drop werkt niet betrouwbaar

**Dubbel systeem met conflicten:**

1. **Blazor-events** (`@ondragstart`, `@ondrop`) op palette-items in `DesignerShell.razor` — stuurt events naar `OnPaletteDragStart` / `OnDropRequested`.
2. **JS-listeners** in `setupDragAndDrop()` — hecht eigen `dragstart`/`drop`-listeners op dezelfde elementen, roept `OnJavaScriptDrop` aan via JSInterop.

**Probleem A**: `setupDragAndDrop()` wordt **nergens aangeroepen**. `OnInitializedAsync` roept `setupResizablePanels` aan maar NIET `setupDragAndDrop`. De JS drag & drop code is dood.

**Probleem B**: De JS-versie gebruikt `querySelectorAll('.designer-dropzone')` met een statische index, wat alleen werkt voor root-level drops op het moment van setup — niet voor dynamisch aangemaakte dropzones in geneste slots.

**Probleem C**: Dropzones zijn 2px hoog met transparante achtergrond. Bij het slepen krijgt de gebruiker **geen visuele feedback** waar gedropped kan worden, omdat:
- De CSS-klasse `designer-dropzone--drag-over` alleen wordt toegevoegd door de JS-handler (die niet actief is)
- De Blazor `@ondragover` handler (`OnDropZoneDragOver`) is een lege methode die niets doet
- CSS `:hover` werkt niet betrouwbaar tijdens drag-operaties in browsers

**Probleem D**: De `DesignerCanvasNode` genereert dropzones via `RenderTreeBuilder` (regel 173-209) met Blazor `ondragover`/`ondrop` events. Deze dropzones werken WEL met het Blazor-systeem, maar missen visuele drag-over feedback.

**Oplossing**: 
- Verwijder de dode JS `setupDragAndDrop` functie
- Voeg CSS-state toe op de Blazor dropzones via een `_activeDrag`-cascading parameter die dropzones een visuele "ontvangst-klaar" state geeft wanneer er een drag actief is
- Vergroot de drop-target hit-area tijdens drag (van 2px naar minimaal 24px)
- Voeg visuele insertielijn + glow toe bij dragover

---

### 3. HOOG — Layout met 5 zijpanelen is onwerkbaar

De designer toont **altijd alle 5 panelen** naast de canvas:

```
[Palette 220px] | [Canvas flex:1] | [Properties 320px] | [Data 320px] | [Tree 220px]
```

Op een 1920px scherm: canvas = ~840px. Op 1440px: canvas = ~360px. Op 1200px: **canvas is nul pixels breed**.

**Problemen:**
- Data-panel is irrelevant wanneer je componenten sleept (80% van de tijd)
- Tree-panel en Issues-panel zijn gecombineerd in één panel, maar het tree-panel is ook irrelevant bij eenvoudige ontwerpen
- Alle 5 panelen zijn altijd open — geen manier om ze in te klappen
- De responsive breakpoints in de RCL CSS (`flex-wrap` op 1200px) en de demo CSS (`grid` op 1300px) conflicteren

**Oplossing**:
- Maak panelen in- en uitklapbaar (toggle-knoppen in de toolbar of panel-headers)
- Default: alleen Palette + Canvas + Properties zichtbaar
- Data en Structure/Issues als uitklapbare panels of tabs in het rechterpaneel
- Bewaar open/dicht-state in localStorage

---

### 4. HOOG — Toolbar is overbelast

De toolbar bevat **16+ controls** in drie groepen:
1. Template-dropdown, Nieuw, Openen, file-input, opgeslagen-docs-dropdown, Opslaan, Versiegeschiedenis, Exporteren, Command Palette
2. Undo, Redo, dirty-indicator
3. Theme-dropdown, Mobiel, Tablet, Desktop

**Problemen:**
- Geen visuele hiërarchie — alle knoppen zien er hetzelfde uit
- Template-dropdown en opgeslagen-docs-dropdown zijn naast elkaar, beide met onduidelijk doel
- "Versiegeschiedenis" neemt evenveel ruimte in als "Opslaan" terwijl het een secundaire actie is
- Op schermen < 1400px wrapt de toolbar naar 2-3 regels
- De `InputFile` is zichtbaar als raw HTML file-input

**Oplossing**:
- Primaire acties prominent: Opslaan, Undo/Redo
- Secundaire acties in overflow-menu (⋯): Versiegeschiedenis, Exporteren, Command Palette
- Bestand-acties gegroepeerd in een "Bestand"-dropdown: Nieuw, Openen, Opslaan als, Importeren
- Viewport-knoppen als icoon-toggle-groep (📱 💻 🖥️) in plaats van tekst-knoppen

---

### 5. HOOG — Monaco code-editor doet twee-weg sync niet

`OnCodeEditorChanged` in `DesignerCodePanel.razor` bevat:
```csharp
// In a real implementation, this would parse the code and update the model.
// For now, just sync the preview.
```

De Razor-code tab heet "Code (Editable)" maar wijzigingen in de code worden **niet teruggeschreven naar het model**. Alleen de JSON-tab heeft werkende twee-weg sync.

**Oplossing**: 
- Hernoem de tab naar "Code (Preview)" totdat parsing geïmplementeerd is, OF
- Implementeer Razor→model parsing (complex — mogelijk beter om de tab als read-only te markeren)

---

### 6. HOOG — Geen alternatief voor drag & drop om componenten toe te voegen

Drag & drop is de **enige manier** om een component aan de canvas toe te voegen. Er is geen:
- Dubbelklik op een palette-item om het toe te voegen aan de geselecteerde container/root
- "Toevoegen"-knop op palette-items
- Context-menu op de canvas met "Component invoegen"
- Keyboard-shortcut om het paletten-zoekresultaat toe te voegen

**Gevolg**: Niet-technische gebruikers die niet gewend zijn aan drag & drop (of een trackpad gebruiken) kunnen de designer niet gebruiken.

**Oplossing**: 
- Voeg een klik-handler toe op palette-items: klik = voeg toe aan geselecteerde container of canvas-root
- Voeg een "+" knop toe in lege slots met een component-picker dropdown

---

### 7. MEDIUM — Paginatabs UX

Elke tab toont **5 tekst-knoppen** altijd zichtbaar: "Naam", "Dup", "Omhoog", "Omlaag", "Weg".

**Problemen:**
- Visuele clutter — elke tab is breed door 5 knoppen
- "Omhoog"/"Omlaag" is redundant met drag-reorder (al geïmplementeerd)
- Geen iconen, alleen cryptische afkortingen
- "Weg" als delete-tekst is informeel en kan verwarring veroorzaken

**Oplossing**:
- Verberg acties in een context-menu (rechtermuisknop of ⋯-knop per tab)
- Verwijder "Omhoog"/"Omlaag" (drag-reorder is genoeg)
- Gebruik iconen: ✏️ hernoemen, 📋 dupliceren, 🗑️ verwijderen

---

### 8. MEDIUM — Property panel toont te veel

**Problemen:**
- Alle parameters worden getoond inclusief interne/EventCallback-parameters die "Logica in geexporteerde code" tonen
- Geen progressive disclosure (geen onderscheid basis/geavanceerd)
- Parameter-labels zijn ruwe C#-namen (`AriaLabel`, `ButtonStyle`, `ValueChanged`)
- Binding-picker is altijd zichtbaar voor bindbare parameters, ook als er geen data-model is

**Oplossing**:
- Verberg EventCallback-parameters standaard
- Groepeer in "Basis" (Label, Text, Value, Placeholder) en "Geavanceerd" (alles overige) — collapsed by default
- Toon menselijke labels (`DisplayLabel` uit prompt 62 metadata)
- Toon binding-picker alleen als het data-model entiteiten bevat

---

### 9. MEDIUM — Structure tree mist functionaliteit

De tree gebruikt `RadzenTree` met basis Text/Children, maar:
- Geen iconen per componenttype (alle items zien er identiek uit)
- Geen drag & drop reorder in de tree
- Geen context-menu (rechtermuisknop) voor verwijderen/dupliceren
- Selectie-sync tree→canvas werkt, maar canvas→tree scroll-into-view niet

---

### 10. MEDIUM — Geen touch-ondersteuning

- `touch-action: none` staat op palette-items maar er zijn geen touch-event-handlers
- HTML Drag & Drop API werkt niet op mobile/touch devices
- Resize-dividers luisteren alleen naar `mousedown`, niet `touchstart`/`pointermove`

---

### 11. LAAG — Resize-dividers missen tree/data panel

De markup heeft dividers voor palette↔canvas en canvas↔properties, maar:
- Geen divider tussen properties↔data
- Geen divider tussen data↔tree
- Double-click reset (prompt 57 fase 9) is niet geïmplementeerd

---

### 12. LAAG — Conflicterende responsive CSS

- RCL `designer.css`: `@media (max-width: 1200px)` met `flex-wrap` en `calc(50%)` breedtes
- Demo `Designer.razor.css`: `@media (max-width: 1300px)` met `grid-template-areas` layout
- Beide kunnen tegelijk actief zijn, wat leidt tot onvoorspelbaar layout-gedrag

---

### 13. LAAG — Accessibility gaps

- Canvas heeft geen focusmanagement na selectie/delete
- Geen `aria-live` aankondigingen bij drag-operaties
- Tab-acties missen beschrijvende `aria-label`s (huidige labels zijn "Naam", "Dup")
- Issues-panel severity-filters zijn checkboxen zonder `fieldset`/`legend`

---

## Aanbevolen uitvoeringsvolgorde

| Prioriteit | Bevinding | Prompt |
|---|---|---|
| **P0 — Blokkerend** | CSS orphaning (§1) | Deel van deze prompt (fase 1) |
| **P0 — Blokkerend** | Drag & drop reparatie (§2) | Deel van deze prompt (fase 2) |
| **P1 — Hoog** | Click-to-add als drag-alternatief (§6) | Deel van deze prompt (fase 3) |
| **P1 — Hoog** | Layout met inklapbare panelen (§3) | Deel van deze prompt (fase 4) |
| **P1 — Hoog** | Toolbar herstructurering (§4) | Deel van deze prompt (fase 5) |
| **P1 — Hoog** | Monaco code-tab eerlijk labelen (§5) | Deel van deze prompt (fase 6) |
| **P2 — Medium** | Paginatabs opschonen (§7) | Deel van deze prompt (fase 7) |
| **P2 — Medium** | Property panel progressive disclosure (§8) | Verwijzing naar prompt 62 |
| **P2 — Medium** | Structure tree verbeteren (§9) | Deel van deze prompt (fase 8) |
| **P2 — Medium** | Touch-ondersteuning (§10) | Aparte prompt (low priority) |
| **P3 — Laag** | Resize-dividers completeren (§11) | Verwijzing naar prompt 57 fase 9 |
| **P3 — Laag** | Responsive CSS opschonen (§12) | Deel van fase 1 (CSS merge) |
| **P3 — Laag** | Accessibility gaps (§13) | Doorlopende taak |

---

## Fase 1 — CSS consolidatie en orphaned bestanden verplaatsen

### Stap 1: Merge CSS
1. Kopieer alle regels uit `samples/Agterhuis.Ui.Demo/Components/Pages/Designer.razor.css` naar `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css`.
2. Kopieer alle regels uit `samples/Agterhuis.Ui.Demo/Components/Designer/DesignerCanvasNode.razor.css` naar dezelfde `designer.css`.
3. De-dupliceer: waar de RCL-versie en demo-versie dezelfde selector hebben, behoud de **demo-versie** (die is recenter en bevat meer styling).
4. Verwijder de orphaned bestanden:
   - `samples/Agterhuis.Ui.Demo/Components/Pages/Designer.razor.css`
   - `samples/Agterhuis.Ui.Demo/Components/Designer/DesignerCanvasNode.razor.css`
5. Verwijder eventuele `samples/Agterhuis.Ui.Demo/Components/Designer/` directory als deze leeg is na de verwijdering.

### Stap 2: Responsive CSS unificeren
- Verwijder de `@media (max-width: 1200px)` en `@media (max-width: 900px)` breakpoints die `flex-wrap` gebruiken.
- Behoud en verbeter het grid-based responsive layout met één consistente breakpoint (1300px).

### Stap 3: Verifieer CSS-referenties
- Controleer dat `_content/Agterhuis.Ui.Designer/css/designer.css` correct wordt geladen in de demo-app's `index.html`.
- Zoek naar eventuele `<link>` tags die naar de oude demo CSS-bestanden verwijzen.

## Fase 2 — Drag & drop reparatie

### Stap 1: Verwijder dode JS code
- Verwijder `setupDragAndDrop` functie uit `designer-interop.js`.
- Verwijder `blazorRef`, `activeDrag` variabelen.
- Behoud `OnJavaScriptDrop` [JSInvokable] als fallback maar markeer als deprecated.

### Stap 2: Visuele drag-feedback via Blazor state
- Voeg een `bool IsDragActive` property toe aan `DesignerShell` die `true` is wanneer `_activeDrag is not null`.
- Cascade deze waarde naar `DesignerCanvasNode` (nieuwe parameter `IsDragActive`).
- Wanneer `IsDragActive`:
  - Dropzones krijgen een extra CSS-klasse `designer-dropzone--ready` met vergroot hit-area (min-height 24px), zichtbare stippellijn, en achtergrondkleur.
  - Lege slots krijgen een pulserende "Sleep hier" animatie.
- Bij `@ondragover:preventDefault` op dropzones: voeg een CSS-klasse `designer-dropzone--hover` toe via Blazor state (niet JS).

### Stap 3: Vergroot drop-targets
- Wijzig `.designer-dropzone` CSS: wanneer `.designer-dropzone--ready`:
  ```css
  .designer-dropzone--ready {
      min-height: 24px;
      border: 2px dashed var(--agt-color-primary-300);
      background: var(--agt-alpha-primary-5);
      margin: var(--agt-spacing-1) 0;
  }
  ```
- Root dropzone (`designer-dropzone--root`): min-height 40px wanneer ready.

### Stap 4: Insertie-indicator
- Bij `@ondragenter` op een dropzone: stel een `_hoverDropzoneId` in op de DesignerShell.
- Toon een horizontale lijn (3px, primary-500) met fade-in op de gehoverde dropzone.
- Bij `@ondragleave`: verwijder de indicator.

## Fase 3 — Click-to-add als alternatief voor drag & drop

### Stap 1: Klik op palette-item
- Voeg een `@onclick` handler toe aan palette-items naast de bestaande `@ondragstart`.
- Bij klik: voeg het component toe aan:
  1. De geselecteerde container's `ChildContent` slot (als een node geselecteerd is en de beschrijving een ChildContent slot heeft), OF
  2. De root van de actieve pagina (als er niets geselecteerd is).
- Selecteer automatisch de nieuw toegevoegde node.
- Toon een korte toast/feedback ("Component toegevoegd").

### Stap 2: "+" knop in lege slots
- In `DesignerCanvasNode.RenderSlot()`: wanneer een slot leeg is, toon naast "Lege slot" een "+" knop.
- De "+" knop opent een mini-palette dropdown (gefilterd op compatibele componenten voor die slot).
- Selecteer het nieuw toegevoegde component.

## Fase 4 — Panelen inklapbaar maken

### Stap 1: Panel toggle state
- Voeg `bool` velden toe: `_paletteCollapsed`, `_dataCollapsed`, `_treeCollapsed`, `_codeCollapsed`.
- Properties panel blijft altijd open (essentieel voor de workflow).
- Bewaar state in localStorage via `designerInterop.setJson` / `getJson`.

### Stap 2: Toggle-knoppen
- Voeg toggle-iconen toe aan de panel-headers (chevron-links/rechts voor zijpanelen, chevron-omhoog/omlaag voor code-panel).
- Wanneer collapsed: toon alleen een smalle balk (32px) met het panel-icoon en de toggle-knop.

### Stap 3: Standaard state
- Default bij eerste bezoek: Palette open, Canvas open, Properties open, Data dicht, Tree/Issues dicht, Code dicht.
- Na het openen van een opgeslagen document: herstel de laatst gebruikte layout.

## Fase 5 — Toolbar herstructurering

### Stap 1: Groepeer in dropdown-menu's
- **Bestand-menu** (dropdown): Nieuw, Openen (bestand), Openen (opgeslagen), Opslaan, Opslaan als…, Exporteren, Versiegeschiedenis.
- **Primaire acties** (altijd zichtbaar): Opslaan-knop (prominent), Undo, Redo.
- **Viewport toggle** (icoon-groep): 📱 💻 🖥️ met active-state.
- **Instellingen** (overflow ⋯): Theme-dropdown, Command Palette.

### Stap 2: Dirty indicator
- Verplaats "Wijzigingen niet opgeslagen" naar een subtiele dot-indicator op de Opslaan-knop (gele dot = unsaved).

### Stap 3: Template-selectie
- Verplaats template-dropdown naar het "Nieuw document"-dialoog in plaats van de toolbar.

## Fase 6 — Monaco code-tab eerlijk labelen

- Hernoem "Code (Editable)" naar "Razor (preview)" in `DesignerCodePanel.razor`.
- Zet de Monaco code-editor op `readOnly: true` totdat Razor→model parsing is geïmplementeerd.
- Behoud de JSON-tab als "Model (JSON)" met werkende twee-weg sync.

## Fase 7 — Paginatabs opschonen

### Stap 1: Context-menu patroon
- Verwijder de inline "Naam", "Dup", "Omhoog", "Omlaag", "Weg" knoppen.
- Voeg een `⋯` knop per tab toe die een context-menu opent met:
  - Hernoemen
  - Dupliceren
  - Verwijderen (met bevestiging, disabled als het de laatste pagina is)
- Behoud drag-reorder (al geïmplementeerd).

### Stap 2: Tab styling
- Actieve tab: duidelijke visuele distinctie (primary-kleur bodem-rand).
- Tab-label: truncate met ellipsis bij > 20 karakters.
- "+" knop: consistent gestyled met de tabs.

## Fase 8 — Structure tree verbeteren

### Stap 1: Iconen per componenttype
- Gebruik het `Icon` veld van `DesignerComponentDescriptor` in de tree-items.
- Toon het icoon naast de componentnaam.

### Stap 2: Scroll-into-view bij canvas-selectie
- Wanneer een node op de canvas wordt geselecteerd: scroll de corresponderende tree-item in beeld.
- Gebruik `JS.InvokeVoidAsync("element.scrollIntoView", ...)` of de RadzenTree's ingebouwde selectie-API.

---

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- **Visueel**: open de designer → canvas nodes hebben stijl (rand, achtergrond, selectie-glow)
- **Visueel**: sleep een component → dropzones worden zichtbaar (24px hoog, stippellijn)
- **Visueel**: klik op een palette-item → component verschijnt op de canvas
- **Visueel**: panelen in-/uitklappen werkt, state blijft na page refresh
- **Visueel**: toolbar is compact, bestand-menu opent als dropdown
- **Visueel**: paginatabs tonen alleen tab-naam + ⋯ knop
- **Visueel**: code-tab zegt "Razor (preview)" en is read-only
- Rapporteer: het aantal verplaatste CSS-regels, de nieuwe panel-layout structuur, en de klik-to-add implementatie
