# Prompt 68 — Designer UX-redesign op basis van industry best practices

Uitgebreide UX-redesign van de designer op basis van bewezen patronen uit Webflow, Retool, Figma, Wix, GrapeJS, Appsmith en Penpot. Per gebied is de beste aanpak gekozen uit meerdere oplossingen. Doel: een designer die aanvoelt als een professionele tool — intuïtief voor niet-technische gebruikers, krachtig genoeg voor developers.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Competitieve analyse — sterke punten per platform

| Platform | Sterkste UX-patroon | Wat wij overnemen |
|----------|---------------------|-------------------|
| **Webflow** | Blue-highlight hover + insertion line, breadcrumb navigator, Quick Find (Cmd+E) | Canvas-selectie feedback, breadcrumb nav, command palette |
| **Retool** | Subcomponent-first inspector, 3-sectie model (Content/Interaction/Appearance), on-canvas inline label editing, progressive disclosure | Property panel herstructurering, inline editing op canvas |
| **Figma** | Direct manipulation met handles, snapping + alignment guides, multi-select, bounding box | Selectie-visuelen, toekomstige snap guides |
| **Wix** | Contextual floating toolbar bij selectie, snap-to-objects paarse lijnen, element-specifieke toolbars | Floating toolbar bij geselecteerd component |
| **GrapeJS** | Block manager → component scheiding, trait system voor properties, component tree mirroring HTML | Component tree structuur (al aanwezig) |
| **Appsmith** | Grid-gebaseerde canvas met dot-grid, snel van leeg naar werkend | Visueel grid op canvas |
| **Penpot** | CSS-native layouts (flex/grid), responsive constraints, design tokens | Responsive layout indicatoren |
| **NN/g research** | Elke drag-fase (resting→grabbed→in-transit→dropped) eigen visuele state, altijd keyboard-alternatief, snap-animatie bij drop | Complete drag-state feedback systeem |

---

## Fase 1 — Canvas selectie-feedback (van Webflow + Figma)

### Huidige situatie
Bij selectie verschijnt een selectie-bar bovenaan de node met naam + drag-handle. Hover geeft geen feedback. Er is geen visueel verschil tussen "ik hover over dit element" en "dit element is niet interactief."

### Gewenst gedrag (Webflow-model)

**Hover-state:**
- Bij hover over een canvas-node: toon een **lichtblauwe outline** (2px, `var(--agt-color-primary-200)`) rond het element
- Toon een **label-tag** linksboven met de componentnaam (kleine pill, blauw, 11px font)
- Hover op een child terwijl parent al hover heeft: parent outline wordt subtiel (1px dashed), child krijgt de volle hover

**Selectie-state (bestaande bar vervangen):**
- Bij klik: toon een **blauwe outline** (2px solid, `var(--agt-color-primary-500)`) rond het element
- Toon de **label-tag** linksboven (bolder, achtergrond `var(--agt-color-primary-500)`, witte tekst)
- Toon **resize handles** op de hoeken als het element resizable is (RadzenColumn → Size aanpassen)
- De bestaande `designer-canvas-node__bar` met drag-handle BLIJFT maar wordt visueel subtieler — een smalle strip bovenaan in plaats van een brede balk

**Breadcrumb navigator (Webflow-model):**
- Onder de canvas (of bovenaan, bij de huidige `designer-breadcrumb`): toon het **volledige pad** van root → geselecteerd element
- Elke stap is klikbaar om dat parent-element te selecteren
- Voorbeeld: `RadzenRow > RadzenColumn > AgtCard > AgtTextField`
- De huidige `SelectionBreadcrumb` property bestaat al — verbeter de rendering met klikbare links

### Implementatie

In `DesignerCanvasNode.razor`:
```razor
<div class="designer-canvas-node @NodeClasses"
     data-agt-design-node-id="@Node.Id"
     data-agt-design-component="@Node.ComponentType"
     @onmouseenter="OnMouseEnter"
     @onmouseleave="OnMouseLeave">
    
    <div class="designer-canvas-node__label">
        @DescriptorLabel
    </div>
    
    @* ... bestaande content ... *@
</div>
```

Voeg CSS toe:
```css
.designer-canvas-node {
    position: relative;
    outline: 2px solid transparent;
    outline-offset: -2px;
    transition: outline-color 0.15s ease;
}

.designer-canvas-node--hover {
    outline-color: var(--agt-color-primary-200);
}

.designer-canvas-node--selected {
    outline-color: var(--agt-color-primary-500);
}

.designer-canvas-node__label {
    position: absolute;
    top: -20px;
    left: 0;
    font-size: 11px;
    line-height: 18px;
    padding: 0 6px;
    border-radius: 3px 3px 0 0;
    background: var(--agt-color-primary-200);
    color: var(--agt-color-primary-900);
    opacity: 0;
    pointer-events: none;
    transition: opacity 0.15s ease;
    white-space: nowrap;
    z-index: 10;
}

.designer-canvas-node--hover > .designer-canvas-node__label {
    opacity: 1;
}

.designer-canvas-node--selected > .designer-canvas-node__label {
    opacity: 1;
    background: var(--agt-color-primary-500);
    color: white;
    font-weight: 600;
}
```

Voeg `_isHovering` state toe aan DesignerCanvasNode met `@onmouseenter` / `@onmouseleave` handlers. Gebruik `@onmouseenter:stopPropagation` om parent-hover correct te laten werken.

## Fase 2 — Property panel herstructurering (van Retool)

### Huidige situatie
Het property panel toont ALLE parameters in categorieën (Layout, Invoer, Data, etc.) met een "Geavanceerd" toggle. De Layout-sectie (Rij, Kolom, etc.) is misleidend voor niet-technische gebruikers.

### Gewenst gedrag (Retool subcomponent-first model)

Herstructureer het property panel naar **drie secties** (Retool-model):

**1. Inhoud (Content)**
- Alle tekst/data parameters: Label, Placeholder, Value, Text, Title, Description, Data, Icon
- Data-binding keuzes (entiteit/veld selectie)
- Dit is wat 90% van de gebruikers 90% van de tijd nodig heeft

**2. Interactie (Interaction)**
- Validation rules (Verplicht, Min/Max, Patroon)
- Disabled/Visible toggles
- Event handlers (alleen in geavanceerde modus)

**3. Weergave (Appearance)**
- ButtonStyle, Variant als visuele keuze-knoppen
- ColorToken als kleurenwiel/swatch
- Layout-slot (Grid-rij, Grid-kolom) — **alleen in geavanceerde modus**
- Spacing, grootte-instellingen

### Progressive disclosure (Retool-model)

Elke sectie heeft een **"Meer opties" toggle** die geavanceerde properties toont:
- Standaard: alleen de 3-5 meest gebruikte properties per sectie
- "Meer opties": alle properties van die sectie
- De toggle onthoudt zijn staat per sessie (niet per component)

### Subcomponent navigatie (Retool on-canvas click)

Wanneer een component subcomponenten heeft (Label, Icon, Tooltip):
- Toon ze als klikbare pills bovenaan de Inhoud-sectie
- Klik op "Label" → filter toont alleen Label-gerelateerde properties
- Dit vervangt de huidige platte lijst van alle parameters

### Implementatie

Refactor `PropertyPanel.razor`:

```csharp
private enum InspectorSection { Content, Interaction, Appearance }
private InspectorSection _activeSection = InspectorSection.Content;
private bool _showAdvancedContent;
private bool _showAdvancedInteraction;
private bool _showAdvancedAppearance;
```

Maak een `PropertyClassifier` static class die per parameter bepaalt in welke sectie hij hoort:
```csharp
public static InspectorSection Classify(ComponentParameterDescriptor param)
{
    if (IsContentParam(param)) return InspectorSection.Content;
    if (IsInteractionParam(param)) return InspectorSection.Interaction;
    return InspectorSection.Appearance;
}

private static bool IsContentParam(ComponentParameterDescriptor p)
    => ContentNames.Contains(p.Name) || p.IsBindable || p.IsRenderFragment;

private static bool IsInteractionParam(ComponentParameterDescriptor p)
    => InteractionNames.Contains(p.Name) || p.IsEventCallback;

private static readonly HashSet<string> ContentNames = new(StringComparer.OrdinalIgnoreCase)
{
    "Label", "Placeholder", "Value", "Text", "Title", "Description",
    "Data", "Icon", "Name", "AriaLabel", "SaveText", "CancelText"
};

private static readonly HashSet<string> InteractionNames = new(StringComparer.OrdinalIgnoreCase)
{
    "Disabled", "Visible", "Required", "ReadOnly", "AllowPaging",
    "PageSize", "AllowSorting", "AllowFiltering"
};
```

Render de drie secties als **tabs** bovenaan het property panel:
```razor
<div class="designer-inspector-tabs" role="tablist">
    <button role="tab" class="@TabClass(InspectorSection.Content)" 
            @onclick="() => _activeSection = InspectorSection.Content">Inhoud</button>
    <button role="tab" class="@TabClass(InspectorSection.Interaction)"
            @onclick="() => _activeSection = InspectorSection.Interaction">Interactie</button>
    <button role="tab" class="@TabClass(InspectorSection.Appearance)"
            @onclick="() => _activeSection = InspectorSection.Appearance">Weergave</button>
</div>
```

## Fase 3 — Drag & drop state machine (van NN/g research + Webflow)

### Huidige situatie
Drag feedback is minimaal: dropzones krijgen een CSS class bij hover, maar er is geen ghost preview, geen insertion line, geen animatie bij drop.

### Gewenst gedrag — 5 visuele states

Implementeer de volledige drag-state machine conform NN/g research:

**State 1 — Resting (palette item)**
- Cursor: `default`
- Palette-item heeft een subtiele drag-handle icoon (6-dot gripper) links
- Tooltip bij hover: componentnaam + korte beschrijving

**State 2 — Grabbed (drag start)**
- Palette-item wordt semi-transparant op zijn originele positie (30% opacity)
- Cursor: `grabbing`
- Een **ghost preview** verschijnt bij de cursor: icoon + componentnaam, semi-transparant (70% opacity), met schaduw
- Gebruik `e.dataTransfer.setDragImage()` met een dynamisch element

**State 3 — In transit (dragging over canvas)**
- Alle dropzones op de canvas tonen een **pulserende blauwe lijn** (insertion indicator)
- De actieve dropzone (dichtstbijzijnde) toont een **dikke blauwe lijn** (4px) met een "+" icoon
- Het parent-element van de actieve dropzone krijgt een **blauwe outline** om aan te geven "hier drop je IN"
- Canvas scrollt automatisch als de cursor bij de rand komt

**State 4 — Dropped (succesvolle plaatsing)**
- Het nieuwe component verschijnt met een **flash-animatie** (300ms glow, `var(--agt-color-primary-200)`)
- Het component wordt automatisch geselecteerd
- Canvas scrollt naar het nieuwe component als het buiten beeld is
- Een korte **toast** verschijnt: "Tekstveld toegevoegd" (verdwijnt na 2s)
- De structuurboom update en het nieuwe item is highlighted

**State 5 — Cancelled (drag buiten canvas)**
- Palette-item keert terug naar 100% opacity
- Alle dropzone-indicators verdwijnen
- Geen verdere feedback nodig

### Implementatie

Breid `designer.css` uit met animatie-keyframes:
```css
@keyframes dropzone-pulse {
    0%, 100% { opacity: 0.5; }
    50% { opacity: 1; }
}

.designer-dropzone--active {
    border: 2px solid var(--agt-color-primary-500);
    animation: dropzone-pulse 1.5s ease-in-out infinite;
}

.designer-dropzone--active::before {
    content: "+";
    position: absolute;
    left: 50%;
    top: -10px;
    transform: translateX(-50%);
    background: var(--agt-color-primary-500);
    color: white;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: bold;
}

.designer-canvas-node--flash {
    animation: node-flash 0.3s ease-out;
}

@keyframes node-flash {
    0% { box-shadow: 0 0 0 4px var(--agt-color-primary-300); }
    100% { box-shadow: 0 0 0 0 transparent; }
}
```

In `designer-interop.js`, breid `flashNode` uit om ook te scrollen:
```javascript
const flashNode = (nodeId) => {
    const el = document.querySelector(`[data-agt-design-node-id="${nodeId}"]`);
    if (!el) return;
    el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    el.classList.add('designer-canvas-node--flash');
    el.addEventListener('animationend', () => {
        el.classList.remove('designer-canvas-node--flash');
    }, { once: true });
};
```

## Fase 4 — Quick Find / Command Palette (van Webflow)

### Huidige situatie
Er is een `AgtCommandPalette` component in het Instellingen-menu. Dit is slecht vindbaar en beperkt.

### Gewenst gedrag (Webflow Quick Find model)

De Command Palette wordt de **centrale zoekbalk** van de designer, bereikbaar via **Ctrl+K** (of Ctrl+E):

**Zoekresultaten in categorieën:**

| Categorie | Zoekbare items | Actie bij selectie |
|-----------|---------------|-------------------|
| Componenten | Alle palette-items met Nederlandse naam | Voeg component toe op canvas |
| Pagina's | Alle pagina's in het document | Navigeer naar die pagina |
| Acties | Opslaan, Exporteren, Undo, Redo, Preview, Nieuw | Voer actie uit |
| Eigenschappen | Parameters van het geselecteerde component | Scroll naar die property in panel |

**UX-details:**
- Floating dialog, gecentreerd op het scherm, met zoek-input bovenaan
- Fuzzy matching: "tksv" vindt "Tekstveld"
- Keyboard navigatie: pijltjes + Enter
- Recent gebruikte items bovenaan
- Escape sluit de palette

### Implementatie

De `AgtCommandPalette` bestaat al. Wijzigingen:
1. Verplaats de trigger van het Instellingen-menu naar een **globale keyboard shortcut** (Ctrl+K)
2. Voeg component-items toe aan het command registry: elk palette-item wordt een command dat `OnPaletteItemClickedAsync` aanroept
3. Voeg pagina-navigatie items toe
4. Voeg property-scrolling toe: bij selectie van een property → scroll de PropertyPanel naar dat veld

In `DesignerShell.razor.cs`, `OnPageKeyDown`:
```csharp
if (args.CtrlKey && string.Equals(args.Key, "k", StringComparison.OrdinalIgnoreCase))
{
    CommandRegistry.OpenPalette();
    return;
}
```

## Fase 5 — Floating toolbar bij selectie (van Wix)

### Huidige situatie
Wanneer een component geselecteerd is, moet de gebruiker naar het property panel rechts navigeren om iets te wijzigen. Voor simpele acties (verwijderen, dupliceren, verplaatsen) is dit omslachtig.

### Gewenst gedrag (Wix contextual toolbar)

Bij selectie van een component op de canvas, toon een **floating toolbar** direct boven het element:

```
┌─────────────────────────────────────────────┐
│ [↑] [↓] [📋] [🗑️] [⋮]                      │
│  Omhoog Omlaag Dupliceer Verwijder  Meer    │
└─────────────────────────────────────────────┘
```

**Knoppen:**
- **Omhoog/Omlaag**: Verplaats het component één positie (bestaande `ReorderSiblingCommand`)
- **Dupliceren**: Kopieer het component (bestaande `DuplicateNode` functionaliteit)
- **Verwijderen**: Verwijder het component (bestaande `RemoveNodeCommand`)
- **Meer (⋮)**: Dropdown met: "Kopieer als JSON", "Wikkel in Card", "Wikkel in Row"

### Implementatie

Voeg een floating toolbar div toe aan `DesignerCanvasNode.razor`, alleen zichtbaar bij `IsSelected`:

```razor
@if (IsSelected)
{
    <div class="designer-floating-toolbar" @onclick:stopPropagation="true">
        <button type="button" title="Omhoog verplaatsen" @onclick="OnMoveUp">
            <RadzenIcon Icon="arrow_upward" />
        </button>
        <button type="button" title="Omlaag verplaatsen" @onclick="OnMoveDown">
            <RadzenIcon Icon="arrow_downward" />
        </button>
        <button type="button" title="Dupliceren" @onclick="OnDuplicate">
            <RadzenIcon Icon="content_copy" />
        </button>
        <button type="button" title="Verwijderen" @onclick="OnDelete">
            <RadzenIcon Icon="delete_outline" />
        </button>
    </div>
}
```

CSS:
```css
.designer-floating-toolbar {
    position: absolute;
    top: -36px;
    right: 0;
    display: flex;
    gap: 2px;
    background: var(--agt-surface-1);
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-sm);
    box-shadow: 0 4px 12px var(--agt-shadow-2);
    padding: 4px;
    z-index: 20;
}

.designer-floating-toolbar button {
    background: transparent;
    border: none;
    border-radius: var(--agt-border-radius-sm);
    cursor: pointer;
    padding: 4px;
    color: var(--agt-text-secondary);
    transition: background 0.15s, color 0.15s;
}

.designer-floating-toolbar button:hover {
    background: var(--agt-alpha-primary-10);
    color: var(--agt-color-primary-500);
}
```

Voeg EventCallbacks toe aan DesignerCanvasNode:
```csharp
[Parameter] public EventCallback<string> MoveNodeUp { get; set; }
[Parameter] public EventCallback<string> MoveNodeDown { get; set; }
[Parameter] public EventCallback<string> DuplicateNode { get; set; }
[Parameter] public EventCallback<string> DeleteNode { get; set; }
```

## Fase 6 — Inline editing op canvas (van Retool)

### Huidige situatie
Om een Label te wijzigen moet de gebruiker: component selecteren → property panel scrollen → Label veld vinden → tekst typen. Dit is 4 stappen voor de meest voorkomende actie.

### Gewenst gedrag (Retool on-canvas click model)

**Dubbel-klik** op een tekst-element op de canvas (Label, Title, Description, Placeholder-tekst) activeert **inline editing**:

1. De tekst wordt een bewerkbaar `<input>` of `contenteditable` element, direct op de canvas
2. De tekst is geselecteerd (select-all)
3. Wijzigingen worden live doorgevoerd via `SetNodeParameterCommand`
4. Enter of klik-buiten beëindigt de inline edit
5. Escape annuleert de wijziging

### Implementatie

Voeg aan `DesignerCanvasNode` een `_inlineEditParam` state toe:

```csharp
private string? _inlineEditParam;

private void OnDoubleClick()
{
    // Bepaal welke parameter inline bewerkbaar is
    _inlineEditParam = _descriptor?.Parameters
        .FirstOrDefault(p => InlineEditableNames.Contains(p.Name))?.Name;
}

private static readonly HashSet<string> InlineEditableNames = new(StringComparer.OrdinalIgnoreCase)
{
    "Label", "Text", "Title", "Description", "Placeholder"
};
```

Wanneer `_inlineEditParam` niet null is, render een overlay-input over het component:
```razor
@if (_inlineEditParam is not null)
{
    <div class="designer-inline-edit" @onclick:stopPropagation="true">
        <input type="text"
               value="@GetParamValue(_inlineEditParam)"
               @oninput="args => OnInlineEditChanged(args, _inlineEditParam)"
               @onblur="CommitInlineEdit"
               @onkeydown="OnInlineEditKeyDown"
               autofocus />
    </div>
}
```

Dit vereist een nieuw `EventCallback<(string ParameterName, string Value)> InlineEditCommitted` parameter op DesignerCanvasNode die terugroept naar DesignerShell.

## Fase 7 — Canvas visuele verbeteringen (van Appsmith + Penpot)

### 7a. Dot-grid achtergrond (Appsmith-model)

Voeg een subtiel dot-grid patroon toe aan de canvas-achtergrond om structuur te suggereren:

```css
.designer-canvas {
    background-image: radial-gradient(
        circle,
        var(--agt-input-border) 1px,
        transparent 1px
    );
    background-size: 20px 20px;
}
```

### 7b. Lege slot visuelen

Verbeter de lege slot indicatoren:
- In bewerkingsmodus: toon een **dashed border** met een **"+"** icoon en tekst "Sleep component hierheen"
- De achtergrond is licht gestreept (diagonale lijntjes) om het onderscheid met gevulde slots duidelijk te maken

### 7c. Viewport frame

Wanneer een viewport (mobiel/tablet) geselecteerd is, toon een **device frame** rond de canvas:
- Mobiel: iPhone-achtige afgeronde hoeken met notch
- Tablet: iPad-achtige afgeronde hoeken
- Desktop: geen frame (full-width)

Dit is puur cosmetisch CSS, geen functionele wijziging.

## Fase 8 — Toolbar herstructurering

### Huidige situatie
De toolbar bevat: Bestand (menu), Opslaan, Undo/Redo, Preview, Viewport-knoppen, Instellingen (menu). De menu's sluiten niet (fix in prompt 67). De layout is functioneel maar niet geoptimaliseerd.

### Gewenst toolbar-layout (gebaseerd op Webflow/Figma)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ [📄 Bestand ▾] [💾] [↩ Undo] [↪ Redo] │ Untitled ★ │ [📱] [📱] [🖥] │ [👁 Preview] [⚙ Instellingen ▾] [📦 Exporteren] │
│                                        │  doc naam   │   viewport     │                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

**Wijzigingen:**
1. **Document-naam centraal**: de huidige naam van het document, klikbaar om te hernoemen (inline edit)
2. **Dirty indicator**: een ★ of ● naast de naam als het document onopgeslagen wijzigingen heeft
3. **Exporteren als eigen knop**: niet verstopt in Bestand-menu, want het is de primaire output-actie
4. **Thema-selectie verplaatsen**: van Instellingen-menu naar het property panel onder pagina-eigenschappen (daar hoort het conceptueel)

## Fase 9 — Keyboard shortcuts overzicht (van Figma)

Voeg een keyboard shortcuts overlay toe, bereikbaar via **Ctrl+/** of via het Instellingen-menu:

| Shortcut | Actie |
|----------|-------|
| Ctrl+K | Quick Find / Command Palette |
| Ctrl+S | Opslaan |
| Ctrl+Z | Ongedaan maken |
| Ctrl+Y | Opnieuw |
| Ctrl+P | Preview toggle |
| Ctrl+E | Exporteren |
| Delete | Geselecteerd component verwijderen |
| Ctrl+D | Geselecteerd component dupliceren |
| Ctrl+↑ / Ctrl+↓ | Component omhoog/omlaag verplaatsen |
| Escape | Menu's sluiten / selectie opheffen |
| Ctrl+/ | Dit shortcut-overzicht |

### Implementatie
Maak een `DesignerShortcutsOverlay.razor` component:
- Een modal/overlay met een nette tabel van alle shortcuts
- Toggle via `_showShortcutsOverlay` boolean in DesignerShell
- Registreer Ctrl+/ in `OnPageKeyDown`

## Fase 10 — Structuurboom verbeteringen (van Webflow Navigator)

### Huidige situatie
De structuurboom toont nodes met `RadzenTree`. Het is functioneel maar de visuele koppeling met de canvas is zwak.

### Gewenst gedrag (Webflow Navigator model)

1. **Hover synchronisatie**: hover over een item in de boom → highlight het bijbehorende element op de canvas (en vice versa)
2. **Drag & drop in de boom**: items in de boom moeten versleepbaar zijn om de volgorde te wijzigen (naast de bestaande canvas drag)
3. **Visuele status-iconen**: toon een klein icoon per node dat aangeeft:
   - 🟢 Groen bolletje: component heeft alle vereiste parameters ingevuld
   - 🟡 Geel bolletje: component heeft waarschuwingen
   - 🔴 Rood bolletje: component heeft fouten (ontbrekende verplichte parameters)
4. **Inklapbaar per node**: elk parent-node is inklapbaar met een chevron (▸/▾)
5. **Context-menu** bij rechts-klikken: Dupliceren, Verwijderen, Omhoog, Omlaag, Wikkel in...

### Implementatie

De structuurboom gebruikt al een custom `RenderTreeNodes` methode. Breid deze uit:
```csharp
private RenderFragment RenderTreeNodes(IReadOnlyList<DesignerTreeNode> nodes) => builder =>
{
    foreach (var treeNode in nodes)
    {
        // Voeg data-attribute toe voor hover-synchronisatie
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", GetTreeNodeClass(treeNode));
        builder.AddAttribute(seq++, "data-agt-tree-node-id", treeNode.NodeId);
        builder.AddAttribute(seq++, "onmouseenter", 
            EventCallback.Factory.Create(this, () => OnTreeNodeHover(treeNode.NodeId)));
        builder.AddAttribute(seq++, "onmouseleave", 
            EventCallback.Factory.Create(this, () => OnTreeNodeHover(null)));
        // ... rest van bestaande rendering
    }
};
```

## Fase 11 — Toast / feedback systeem (van meerdere)

### Huidige situatie
De `_uiFeedback` string wordt weergegeven in een banner bovenaan. Dit verdwijnt niet automatisch en is visueel prominent.

### Gewenst gedrag

Vervang de banner door een **toast-systeem** rechtsonder:
- Toasts verschijnen rechtsonder, stapelen naar boven
- Automatisch verdwijnen na 3 seconden
- Types: succes (groen), info (blauw), waarschuwing (oranje)
- Subtiel en niet-blokkerend

### Implementatie

Maak een `DesignerToast.razor` component:
```csharp
public sealed record ToastMessage(string Text, ToastType Type, DateTimeOffset Created);
public enum ToastType { Success, Info, Warning }

// In DesignerShell:
private readonly List<ToastMessage> _toasts = [];

private void ShowToast(string message, ToastType type = ToastType.Success)
{
    _toasts.Add(new ToastMessage(message, type, DateTimeOffset.UtcNow));
    StateHasChanged();
    _ = DismissToastAfterDelay(_toasts.Count - 1);
}

private async Task DismissToastAfterDelay(int index)
{
    await Task.Delay(3000);
    if (index < _toasts.Count)
    {
        _toasts.RemoveAt(index);
        await InvokeAsync(StateHasChanged);
    }
}
```

Vervang alle `_uiFeedback = "..."` aanroepen door `ShowToast("...", ToastType.Success)`.

## Verificatie

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **UX-checklist per fase:**

| # | Fase | Test |
|---|------|------|
| 1 | Canvas selectie | Hover toont blauwe outline + label; klik toont blauwe outline + bold label |
| 2 | Property panel | 3 tabs (Inhoud/Interactie/Weergave); "Meer opties" toggle per sectie |
| 3 | Drag & drop | Ghost bij cursor; pulserende dropzones; flash + scroll bij drop |
| 4 | Quick Find | Ctrl+K opent; zoek "tekst" vindt Tekstveld; Enter voegt toe |
| 5 | Floating toolbar | Selecteer component → toolbar boven element met Dupliceer/Verwijder |
| 6 | Inline editing | Dubbel-klik op Label-tekst → inline input → Enter slaat op |
| 7 | Canvas visuelen | Dot-grid achtergrond; dashed lege slots met "+"; viewport frames |
| 8 | Toolbar | Document-naam centraal; Exporteren als eigen knop |
| 9 | Shortcuts | Ctrl+/ toont overlay met alle shortcuts |
| 10 | Structuurboom | Hover in boom → highlight op canvas; status-iconen per node |
| 11 | Toasts | Acties tonen toast rechtsonder; verdwijnt na 3s |

## Volgorde van uitvoering

Voer de fasen uit in deze volgorde (afhankelijkheden):
1. **Fase 11** (Toasts) — basis voor feedback in alle andere fasen
2. **Fase 1** (Canvas selectie) — visuele basis
3. **Fase 5** (Floating toolbar) — afhankelijk van selectie-feedback
4. **Fase 3** (Drag & drop states) — bouwt voort op canvas visuelen
5. **Fase 2** (Property panel) — grote refactor, onafhankelijk
6. **Fase 8** (Toolbar) — kleine refactor
7. **Fase 7** (Canvas visuelen) — cosmetisch, onafhankelijk
8. **Fase 4** (Quick Find) — bouwt voort op bestaande CommandPalette
9. **Fase 6** (Inline editing) — complexst, als laatste
10. **Fase 10** (Structuurboom) — verbetering, niet kritiek
11. **Fase 9** (Shortcuts overlay) — triviale toevoeging
