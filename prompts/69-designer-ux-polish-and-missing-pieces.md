# Prompt 69 — Designer UX polish: van structuur-editor naar visuele designer

Na prompts 67 en 68 zijn de features aanwezig (hover-feedback, floating toolbar, inline editing, 3-tab inspector, toast, command palette, etc.), maar het geheel voelt nog als een **gestructureerde formulier-editor** in plaats van een **visuele designer**. Deze prompt pakt de visuele en interactieve verfijning aan die het verschil maakt.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Probleemanalyse — waarom het nog niet "goed voelt"

| Probleem | Oorzaak | Impact |
|----------|---------|--------|
| Canvas lijkt op een lijst van dozen | Elk component heeft een volle `designer-canvas-node__bar` (icoon+naam) die 2rem hoog is, visueel zwaarder dan de component zelf | Gebruiker ziet meer chrome dan content |
| Geen visuele hiërarchie op canvas | Alle nodes hebben dezelfde styling ongeacht nesting-diepte | Moeilijk te zien welke component in welke container zit |
| Geen copy/paste | Ctrl+C/Ctrl+V doet niets | Basis verwachting van elke editor |
| Geen multi-select | Shift+klik/Ctrl+klik doet niets | Kan niet meerdere items tegelijk verwijderen/verplaatsen |
| Command Palette is een lege wrapper | `AgtCommandPalette` wordt getoond in een modal, maar heeft geen zichtbaar zoekveld in die context | Ctrl+K opent iets dat er niet uitzienbaar uitziet |
| Palette-items tonen technische namen | `designer-palette-item__meta` toont "RadzenTextBox" naast "Tekstveld" | Verwarrend voor niet-technische gebruikers |
| Geen undo-feedback | Ctrl+Z werkt maar er is geen indicatie van wat er ongedaan gemaakt werd | Gebruiker weet niet of de undo werkte |
| Property panel scrollt weg | Bij veel properties verlies je de component-header | Geen context over wat je bewerkt |
| Divider-resize geeft geen feedback | Geen hover-effect, geen minimum-breedte, geen dubbel-klik-om-te-resetten | Frustrerend bij resizen |
| Lege canvas te kaal | `AgtEmptyState` zonder visueel voorbeeld | Geen motivatie om te beginnen |

---

## Fase 1 — Canvas-node chrome minimaliseren

### Doel
De `designer-canvas-node__bar` moet NIET de primaire visuele laag zijn. De component-preview IS de primaire laag. De bar is een subtiel hulpmiddel.

### Wijzigingen

**In `designer.css`:**

```css
/* De bar wordt een smalle, subtiele strip — niet de focus */
.designer-canvas-node__bar {
    align-items: center;
    background: color-mix(in srgb, var(--agt-surface-2) 60%, transparent);
    border-bottom: 1px solid color-mix(in srgb, var(--agt-input-border) 50%, transparent);
    display: flex;
    min-height: 1.4rem;  /* was 2rem */
    padding: 0 var(--agt-spacing-1);  /* was spacing-2 */
    position: relative;
}

.designer-canvas-node__select {
    font-size: 0.65rem;  /* was font-size-xs (~0.75rem) */
    gap: 0.25rem;  /* was spacing-1 */
    opacity: 0.6;
}

/* Bij hover wordt de bar duidelijker */
.designer-canvas-node:hover .designer-canvas-node__bar {
    background: var(--agt-surface-2);
    border-bottom-color: var(--agt-input-border);
}

.designer-canvas-node:hover .designer-canvas-node__select {
    opacity: 1;
}

/* Geselecteerd: bar is prominent */
.designer-canvas-node--selected .designer-canvas-node__select {
    opacity: 1;
}
```

Dit maakt de bar subtiel wanneer je niet interacteert, en zichtbaar wanneer je hovert of selecteert.

**In `DesignerCanvasNode.razor`:**

Verberg het RadzenIcon in de bar standaard — toon het alleen bij hover/select:
```razor
<div class="designer-canvas-node__bar">
    <button type="button"
            class="designer-canvas-node__select"
            draggable="true"
            @ondragstart="OnDragStart"
            @ondragend="OnDragEnd"
            @onclick="OnSelectClicked"
            aria-current="@(IsSelected ? "true" : "false")">
        <span class="designer-canvas-node__bar-icon">
            <RadzenIcon Icon="@DescriptorIcon" />
        </span>
        <span>@DescriptorLabel</span>
    </button>
</div>
```

CSS voor het icoon:
```css
.designer-canvas-node__bar-icon {
    display: none;
}

.designer-canvas-node:hover .designer-canvas-node__bar-icon,
.designer-canvas-node--selected .designer-canvas-node__bar-icon {
    display: inline-flex;
}
```

## Fase 2 — Visuele nesting-diepte

### Doel
Bij geneste componenten (Row → Column → Card → TextField) moet de nesting visueel herkenbaar zijn, zodat de structuur vanuit de canvas afleesbaar is.

### Wijzigingen

**In `designer.css`:**

```css
/* Eerste nesting-niveau: geen extra rand */
.designer-canvas-node {
    /* bestaand */
}

/* Tweede niveau: iets donkerder achtergrond */
.designer-canvas-node .designer-canvas-node {
    background: color-mix(in srgb, var(--agt-surface-1) 90%, var(--agt-surface-2) 10%);
    border-left: 3px solid var(--agt-alpha-primary-10);
}

/* Derde en dieper: nog iets donkerder + breder border-left */
.designer-canvas-node .designer-canvas-node .designer-canvas-node {
    background: color-mix(in srgb, var(--agt-surface-1) 80%, var(--agt-surface-2) 20%);
    border-left: 3px solid var(--agt-alpha-primary-20);
}
```

Dit geeft een visuele "inspringing" zonder de layout te breken.

## Fase 3 — Copy/Paste

### Doel
Ctrl+C kopieert het geselecteerde component naar een interne clipboard. Ctrl+V plakt het als sibling na het geselecteerde component.

### Wijzigingen

**In `DesignerShell.razor.cs`:**

Voeg een clipboard-veld toe:
```csharp
private string? _clipboardNodeJson;
```

In `OnPageKeyDown`:
```csharp
// Na de bestaande Ctrl+D handler:
if (args.CtrlKey && string.Equals(args.Key, "c", StringComparison.OrdinalIgnoreCase) && _selectedNodeId is not null)
{
    CopySelectedNode();
    return;
}

if (args.CtrlKey && string.Equals(args.Key, "v", StringComparison.OrdinalIgnoreCase) && _clipboardNodeJson is not null)
{
    await PasteNodeAsync();
    return;
}
```

Implementatie:
```csharp
private void CopySelectedNode()
{
    if (_selectedNodeId is null || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index))
    {
        return;
    }

    var node = container[index];
    _clipboardNodeJson = DesignDocumentSerializer.SerializeNode(node);
    ShowToast("Component gekopieerd", ToastType.Info);
    _liveAnnouncement = "Component gekopieerd naar klembord.";
}

private async Task PasteNodeAsync()
{
    if (string.IsNullOrWhiteSpace(_clipboardNodeJson))
    {
        return;
    }

    var pasted = DesignDocumentSerializer.DeserializeNode(_clipboardNodeJson);
    if (pasted is null)
    {
        return;
    }

    // Genereer nieuwe IDs voor de geplakte node en al zijn children
    RegenerateIds(pasted);

    var location = _selectedNodeId is not null && TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index)
        ? DesignNodeLocation.Root(index + 1)  // Plak na het geselecteerde item
        : DesignNodeLocation.Root(ActivePage.Nodes.Count);

    if (_commands.Execute(new AddNodeCommand(ActivePageIndex, location, pasted)))
    {
        _selectedNodeId = pasted.Id;
        _hasRecoveredDraft = true;
        ShowToast("Component geplakt", ToastType.Success);
        _liveAnnouncement = "Component geplakt.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.flashNode", pasted.Id);
    }
}

private static void RegenerateIds(DesignNode node)
{
    node.Id = Guid.NewGuid().ToString("n");
    foreach (var slot in node.Children.Values)
    {
        foreach (var child in slot)
        {
            RegenerateIds(child);
        }
    }
}
```

**Noodzakelijke helpers in `DesignDocumentSerializer`:**

Als `SerializeNode` en `DeserializeNode` nog niet bestaan, voeg ze toe:
```csharp
public static string SerializeNode(DesignNode node)
{
    var wrapper = new DesignDocument
    {
        Pages = [new DesignPage { Route = "/", Title = "clipboard", Nodes = [node] }]
    };
    return Serialize(wrapper);
}

public static DesignNode? DeserializeNode(string json)
{
    var doc = Deserialize(json);
    return doc.Pages.FirstOrDefault()?.Nodes.FirstOrDefault();
}
```

## Fase 4 — Multi-select

### Doel
Shift+klik voegt een node toe aan de selectie. Ctrl+klik togglet. Bij meerdere geselecteerde nodes werken Delete, Ctrl+D en de floating toolbar op alle geselecteerde items.

### Wijzigingen

**In `DesignerShell.razor.cs`:**

Vervang `_selectedNodeId` niet — houd het voor single-select compatibiliteit. Voeg toe:
```csharp
private readonly HashSet<string> _selectedNodeIds = new(StringComparer.Ordinal);

private bool IsNodeSelected(string nodeId) 
    => string.Equals(_selectedNodeId, nodeId, StringComparison.Ordinal) 
    || _selectedNodeIds.Contains(nodeId);
```

In `OnSelectNode`, verwerk modifiers:
```csharp
private async Task OnSelectNode(string nodeId, bool shiftKey = false, bool ctrlKey = false)
{
    if (ctrlKey)
    {
        // Toggle selectie
        if (!_selectedNodeIds.Remove(nodeId))
        {
            _selectedNodeIds.Add(nodeId);
        }
    }
    else if (shiftKey)
    {
        // Voeg toe aan selectie
        _selectedNodeIds.Add(nodeId);
    }
    else
    {
        // Vervang selectie
        _selectedNodeIds.Clear();
        _selectedNodeId = nodeId;
    }
    
    _liveAnnouncement = _selectedNodeIds.Count > 0 
        ? $"{_selectedNodeIds.Count + 1} nodes geselecteerd." 
        : "Node geselecteerd.";
    await InvokeAsync(StateHasChanged);
    await JS.InvokeVoidAsync("designerInterop.scrollTreeItemIntoView", nodeId);
}
```

In `DesignerCanvasNode.razor`, breid `OnSelectClicked` uit om modifier-keys door te geven:
```csharp
[Parameter]
public EventCallback<(string NodeId, bool ShiftKey, bool CtrlKey)> SelectNodeExtended { get; set; }
```

Pas de `@onclick` aan op de select-button:
```razor
@onclick="OnSelectClickedExtended"
@onclick:stopPropagation="true"
```

```csharp
private async Task OnSelectClickedExtended(MouseEventArgs args)
{
    await SelectNodeExtended.InvokeAsync((Node.Id, args.ShiftKey, args.CtrlKey));
}
```

Update de Delete-handler voor multi-select:
```csharp
if (string.Equals(args.Key, "Delete", StringComparison.OrdinalIgnoreCase))
{
    var toDelete = _selectedNodeIds.Count > 0 
        ? _selectedNodeIds.ToList() 
        : _selectedNodeId is not null ? [_selectedNodeId] : [];
    
    foreach (var nodeId in toDelete)
    {
        _commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId));
    }
    
    if (toDelete.Count > 0)
    {
        _selectedNodeId = null;
        _selectedNodeIds.Clear();
        _hasRecoveredDraft = true;
        ShowToast($"{toDelete.Count} component(en) verwijderd", ToastType.Success);
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }
    return;
}
```

## Fase 5 — Command Palette UX verbeteren

### Doel
De command palette (Ctrl+K) moet een **duidelijk zoekveld** bovenaan hebben, met **fuzzy matching**, en resultaten gegroepeerd per categorie.

### Wijzigingen

De huidige implementatie opent een modal met `<AgtCommandPalette />` erin, maar het is niet duidelijk of de AgtCommandPalette zelf al een zoek-input heeft. Controleer de bestaande component.

**Als AgtCommandPalette al een zoek-input heeft:** zorg dat deze `autofocus` krijgt wanneer de modal opent. Voeg toe aan de modal wrapper:

```razor
@if (_showDesignerCommandPalette)
{
    <div class="designer-overlay designer-overlay--transparent" role="dialog" aria-modal="true" aria-label="Quick Find" @onclick="CloseDesignerCommandPalette">
        <div class="designer-command-modal" @onclick:stopPropagation="true">
            <input class="designer-command-search"
                   type="text"
                   placeholder="Zoek component, actie of eigenschap..."
                   value="@_commandSearchQuery"
                   @oninput="OnCommandSearchChanged"
                   @onkeydown="OnCommandSearchKeyDown"
                   autofocus />
            <div class="designer-command-results">
                @foreach (var group in FilteredCommandGroups)
                {
                    <div class="designer-command-group">
                        <div class="designer-command-group__header">@group.Key</div>
                        @foreach (var item in group.Value)
                        {
                            <button type="button"
                                    class="designer-command-item @(_commandSelectedIndex == item.Index ? "designer-command-item--active" : string.Empty)"
                                    @onclick="() => ExecuteCommandItem(item)">
                                <span>@item.Title</span>
                                @if (!string.IsNullOrWhiteSpace(item.ShortcutHint))
                                {
                                    <kbd>@item.ShortcutHint</kbd>
                                }
                            </button>
                        }
                    </div>
                }
                @if (FilteredCommandGroups.Count == 0)
                {
                    <div class="designer-command-empty">Geen resultaten voor "@_commandSearchQuery"</div>
                }
            </div>
        </div>
    </div>
}
```

Voeg state toe:
```csharp
private string _commandSearchQuery = string.Empty;
private int _commandSelectedIndex;
```

**CSS voor de command palette:**
```css
.designer-overlay--transparent {
    background: color-mix(in srgb, var(--agt-surface-0) 50%, transparent);
    backdrop-filter: blur(4px);
}

.designer-command-modal {
    background: var(--agt-surface-1);
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-md);
    box-shadow: 0 20px 60px var(--agt-shadow-2);
    display: grid;
    gap: 0;
    margin: 15vh auto 0;
    max-height: 60vh;
    max-width: 36rem;
    overflow: hidden;
    width: 90%;
}

.designer-command-search {
    background: var(--agt-surface-0);
    border: 0;
    border-bottom: 1px solid var(--agt-input-border);
    color: var(--agt-text-body);
    font: inherit;
    font-size: 1.1rem;
    min-height: 3rem;
    outline: 0;
    padding: 0 var(--agt-spacing-3);
}

.designer-command-results {
    max-height: 50vh;
    overflow-y: auto;
    padding: var(--agt-spacing-1);
}

.designer-command-group__header {
    color: var(--agt-text-muted);
    font-size: var(--agt-font-size-xs);
    font-weight: 600;
    letter-spacing: 0.5px;
    padding: var(--agt-spacing-2) var(--agt-spacing-2) var(--agt-spacing-1);
    text-transform: uppercase;
}

.designer-command-item {
    align-items: center;
    background: transparent;
    border: 0;
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-body);
    cursor: pointer;
    display: flex;
    font: inherit;
    gap: var(--agt-spacing-2);
    justify-content: space-between;
    min-height: 2.2rem;
    padding: 0 var(--agt-spacing-2);
    text-align: left;
    width: 100%;
}

.designer-command-item:hover,
.designer-command-item--active {
    background: var(--agt-alpha-primary-10);
}

.designer-command-item kbd {
    background: var(--agt-surface-0);
    border: 1px solid var(--agt-input-border);
    border-radius: 3px;
    color: var(--agt-text-muted);
    font-family: inherit;
    font-size: var(--agt-font-size-xs);
    padding: 0.1rem 0.4rem;
}

.designer-command-empty {
    color: var(--agt-text-muted);
    padding: var(--agt-spacing-3);
    text-align: center;
}
```

De command palette moet fuzzy-match implementeren. Gebruik een eenvoudige subsequence match:
```csharp
private static bool FuzzyMatch(string query, string target)
{
    if (string.IsNullOrWhiteSpace(query)) return true;
    var qi = 0;
    foreach (var ch in target)
    {
        if (qi < query.Length && char.ToLowerInvariant(ch) == char.ToLowerInvariant(query[qi]))
        {
            qi++;
        }
    }
    return qi == query.Length;
}
```

## Fase 6 — Undo/redo feedback

### Doel
Bij undo/redo toont de toast WAT er ongedaan gemaakt werd.

### Wijzigingen

In `OnUndo`:
```csharp
private async Task OnUndo()
{
    var commandName = _commands.LastCommandName;
    if (_commands.Undo())
    {
        EnsureSelectedPageIndex();
        ClearSelectionIfMissing();
        ShowToast($"Ongedaan: {commandName ?? "actie"}", ToastType.Info);
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }
}
```

In `OnRedo`:
```csharp
private async Task OnRedo()
{
    if (_commands.Redo())
    {
        EnsureSelectedPageIndex();
        ClearSelectionIfMissing();
        ShowToast($"Opnieuw: {_commands.LastCommandName ?? "actie"}", ToastType.Info);
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }
}
```

## Fase 7 — Property panel sticky header

### Doel
Bij scrollen in het property panel moet de component-naam + tabs zichtbaar blijven.

### Wijzigingen

**In `designer.css`:**
```css
.designer-panel--properties {
    position: relative;
}

.designer-properties__field-head:first-child,
.designer-inspector-tabs {
    background: var(--agt-surface-1);
    position: sticky;
    top: 0;
    z-index: 5;
}

.designer-inspector-tabs {
    top: 2.2rem; /* onder de component-naam header */
}
```

Dit zorgt ervoor dat bij scrollen de tabs en componentnaam bovenaan het property panel blijven staan.

## Fase 8 — Divider UX verbeteren

### Doel
De dividers (tussen palette/canvas en canvas/properties) moeten feedback geven bij hover, een minimum-breedte afdwingen, en dubbel-klik om panel-breedte te resetten.

### Wijzigingen

**In `designer.css`:**
```css
.designer-divider {
    background: var(--agt-input-border);
    flex: 0 0 5px;
    position: relative;
    transition: background 120ms ease;
}

.designer-divider--vertical {
    cursor: col-resize;
    width: 5px;
}

.designer-divider--horizontal {
    cursor: row-resize;
    height: 5px;
}

/* Hover: blauwe highlight om aan te geven dat het een control is */
.designer-divider:hover {
    background: var(--agt-color-primary-400);
}

/* Tijdens actief slepen: dikker en prominenter */
.designer-divider:active {
    background: var(--agt-color-primary-500);
}
```

**In `designer-resize-interop.js`:**

Voeg dubbel-klik handler toe die het panel reset naar de CSS custom property default:
```javascript
// In setupResizablePanels, na het initialiseren van elke divider:
divider.addEventListener('dblclick', () => {
    const page = document.querySelector('.designer-page');
    if (!page) return;
    
    if (divider.dataset.divider === 'palette-canvas') {
        page.style.setProperty('--designer-palette-width', '220px');
    } else if (divider.dataset.divider === 'canvas-property') {
        page.style.setProperty('--designer-property-width', '320px');
    } else if (divider.dataset.divider === 'canvas-code') {
        page.style.setProperty('--designer-code-height', '250px');
    }
});
```

Voeg minimumbreedtes toe in de resize-handler:
```javascript
// In de mousemove handler:
const MIN_PANEL = 160;
const MAX_PANEL = 600;
newSize = Math.max(MIN_PANEL, Math.min(MAX_PANEL, newSize));
```

## Fase 9 — Palette technische namen verbergen

### Doel
De technische componentnaam ("RadzenTextBox") moet verborgen worden in de standaardmodus.

### Wijzigingen

**In `DesignerShell.razor`:**

Verwijder de `<small class="designer-palette-item__meta">` regel wanneer `_simpleMode` actief is op de globale designer. Alternatief: toon het alleen bij hover via CSS:

```css
.designer-palette-item__meta {
    display: none;
}

.designer-palette-item:hover .designer-palette-item__meta {
    display: inline;
}
```

Dit houdt de palette schoon maar toont de technische naam bij hover voor powerusers.

## Fase 10 — Verbeterde lege canvas

### Doel
De lege canvas moet aantrekkelijker zijn met visuele hints over hoe te beginnen.

### Wijzigingen

**In `DesignerShell.razor`, vervang het `AgtEmptyState`-blok:**

```razor
@if (ActivePage.Nodes.Count == 0)
{
    <div class="designer-canvas--empty">
        <div class="designer-canvas-onboarding">
            <RadzenIcon Icon="dashboard_customize" Style="font-size: 3rem; color: var(--agt-color-primary-300);" />
            <h3>Begin met ontwerpen</h3>
            <p>Sleep een component uit het palet links, of gebruik een van de snelstartopties hieronder.</p>
            <div class="designer-canvas-onboarding__actions">
                <RadzenButton Text="Formulier" Icon="description" ButtonStyle="ButtonStyle.Base" Variant="Variant.Outlined" Click='() => OnTemplateStartSelected(DesignDocumentTemplateKind.FormPage)' />
                <RadzenButton Text="Dashboard" Icon="dashboard" ButtonStyle="ButtonStyle.Base" Variant="Variant.Outlined" Click='() => OnTemplateStartSelected(DesignDocumentTemplateKind.Dashboard)' />
                <RadzenButton Text="Lege pagina" Icon="add" ButtonStyle="ButtonStyle.Base" Variant="Variant.Outlined" Click='() => OnTemplateStartSelected(DesignDocumentTemplateKind.Blank)' />
            </div>
            <p class="designer-canvas-onboarding__shortcut">Tip: druk <kbd>Ctrl+K</kbd> om snel een component te zoeken.</p>
        </div>
    </div>
}
```

**CSS:**
```css
.designer-canvas-onboarding {
    align-items: center;
    display: grid;
    gap: var(--agt-spacing-2);
    justify-items: center;
    padding: var(--agt-spacing-6);
    text-align: center;
}

.designer-canvas-onboarding h3 {
    color: var(--agt-text-heading);
    margin: 0;
}

.designer-canvas-onboarding p {
    color: var(--agt-text-muted);
    margin: 0;
    max-width: 28rem;
}

.designer-canvas-onboarding__actions {
    display: flex;
    gap: var(--agt-spacing-2);
}

.designer-canvas-onboarding__shortcut {
    font-size: var(--agt-font-size-xs);
}

.designer-canvas-onboarding__shortcut kbd {
    background: var(--agt-surface-0);
    border: 1px solid var(--agt-input-border);
    border-radius: 3px;
    font-family: inherit;
    font-size: var(--agt-font-size-xs);
    padding: 0.1rem 0.4rem;
}
```

## Fase 11 — Viewport device-frame

### Doel
Wanneer mobiel/tablet viewport actief is, toon een device-frame rond de canvas zodat het aanvoelt alsof je ontwerpt voor een specifiek apparaat.

### Wijzigingen

**In `designer.css`:**
```css
.designer-canvas-frame--mobile {
    background: var(--agt-surface-2);
    border: 2px solid var(--agt-text-muted);
    border-radius: 2rem;
    margin: var(--agt-spacing-3) auto;
    max-width: 360px;
    padding: 2.5rem 0.5rem 1.5rem;
    position: relative;
}

.designer-canvas-frame--mobile::before {
    background: var(--agt-text-muted);
    border-radius: 999px;
    content: "";
    height: 4px;
    left: 50%;
    position: absolute;
    top: 1rem;
    transform: translateX(-50%);
    width: 3rem;
}

.designer-canvas-frame--tablet {
    background: var(--agt-surface-2);
    border: 2px solid var(--agt-text-muted);
    border-radius: 1.2rem;
    margin: var(--agt-spacing-3) auto;
    max-width: 768px;
    padding: 1.5rem 0.5rem;
}
```

**In `DesignerShell.razor`:**

Voeg de viewport-class toe aan `designer-canvas-frame`:
```razor
<div class="designer-canvas-frame @GetCanvasFrameClass()" style="@CanvasFrameStyle">
```

```csharp
private string GetCanvasFrameClass() => _viewport switch
{
    "mobile" => "designer-canvas-frame--mobile",
    "tablet" => "designer-canvas-frame--tablet",
    _ => string.Empty
};
```

## Fase 12 — Toast verbetering met animatie

### Doel
Toasts moeten in- en uit-faden, en een close-knop hebben.

### Wijzigingen

**In `designer.css`:**
```css
.designer-toast {
    animation: toast-in 200ms ease-out;
    /* bestaande styles... */
}

.designer-toast--exiting {
    animation: toast-out 200ms ease-in forwards;
}

@keyframes toast-in {
    from { opacity: 0; transform: translateX(100%); }
    to { opacity: 1; transform: translateX(0); }
}

@keyframes toast-out {
    from { opacity: 1; transform: translateX(0); }
    to { opacity: 0; transform: translateX(100%); }
}
```

**In `DesignerShell.razor`:**

Voeg een dismiss-knop toe:
```razor
<div class="designer-toast @GetToastClass(toast.Type)">
    <span>@toast.Text</span>
    <button type="button" class="designer-toast__dismiss" @onclick="() => DismissToast(toast)">&times;</button>
</div>
```

```css
.designer-toast__dismiss {
    background: transparent;
    border: 0;
    color: inherit;
    cursor: pointer;
    font-size: 1.1rem;
    opacity: 0.6;
    padding: 0 0.3rem;
}

.designer-toast__dismiss:hover {
    opacity: 1;
}
```

---

## Verificatie

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **UX-checklist:**

| # | Fase | Test |
|---|------|------|
| 1 | Canvas chrome | Bar is subtiel (1.4rem) zonder hover; zichtbaar bij hover; prominent bij selectie |
| 2 | Nesting | Geneste nodes hebben progressief donkerder achtergrond + border-left |
| 3 | Copy/paste | Ctrl+C op component → toast "Gekopieerd"; Ctrl+V → component verschijnt met nieuwe ID |
| 4 | Multi-select | Ctrl+klik selecteert meerdere; Delete verwijdert alle; visueel meerdere nodes gehighlight |
| 5 | Command Palette | Ctrl+K → groot zoekveld bovenaan; typ "tek" → "Tekstveld" verschijnt; pijltjes + Enter |
| 6 | Undo feedback | Ctrl+Z → toast "Ongedaan: Set parameter" |
| 7 | Property sticky | Scroll properties → tabs blijven bovenaan |
| 8 | Dividers | Hover → blauw; dubbel-klik → reset; min/max breedte gerespecteerd |
| 9 | Palette namen | Technische naam verborgen; zichtbaar bij hover |
| 10 | Lege canvas | Aantrekkelijke onboarding met 3 snelstartknoppen + Ctrl+K tip |
| 11 | Device frame | Mobiel → iPhone-achtige frame; tablet → iPad-achtige frame |
| 12 | Toasts | Slide-in animatie; close-knop; fade-out na 3s |

## Volgorde van uitvoering

1. **Fase 1** (Canvas chrome) — visuele basis, niet-breaking
2. **Fase 2** (Nesting) — CSS-only, geen code
3. **Fase 9** (Palette namen) — CSS-only
4. **Fase 8** (Dividers) — JS + CSS
5. **Fase 7** (Property sticky) — CSS-only
6. **Fase 12** (Toasts) — CSS + kleine Razor
7. **Fase 10** (Lege canvas) — Razor + CSS
8. **Fase 11** (Device frame) — CSS + 1 methode
9. **Fase 6** (Undo feedback) — kleine C# wijziging
10. **Fase 3** (Copy/paste) — C# + serializer helpers
11. **Fase 4** (Multi-select) — grotere refactor
12. **Fase 5** (Command Palette) — grootste UI refactor
