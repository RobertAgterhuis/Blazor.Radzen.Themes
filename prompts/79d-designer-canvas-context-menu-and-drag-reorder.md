# Prompt 79d — Canvas context menu en drag-reorder

De designer heeft een context menu in de Navigator-tree, maar niet op de canvas zelf. Componenten verplaatsen kan alleen via pijl-knoppen (omhoog/omlaag) in de floating toolbar of via het tree context menu. Radzen Studio biedt rechtermuisklik op de canvas voor cut/copy/paste/delete en een drag-handle om componenten visueel te verslepen.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Canvas context menu (rechtermuisklik)

### Huidige staat
- `DesignerCanvasNode.razor` heeft GEEN `@oncontextmenu` handler.
- De floating toolbar (regel 22-36) heeft: omhoog, omlaag, dupliceren, verwijderen.
- Het tree context menu (DesignerShell.razor regels 314-327) heeft: dupliceren, verwijderen, omhoog, omlaag, wikkel in kaart, wikkel in rij.
- Keyboard shortcuts bestaan al: Ctrl+C (copy), Ctrl+V (paste), Ctrl+D (duplicate), Delete.

### Fix

**Stap 1: Voeg `@oncontextmenu` toe aan DesignerCanvasNode.**

In `DesignerCanvasNode.razor`, op de root div:

```razor
<div class="designer-canvas-node ..."
     ...
     @oncontextmenu="OnContextMenu"
     @oncontextmenu:preventDefault="true"
     @oncontextmenu:stopPropagation="true">
```

```csharp
[Parameter]
public EventCallback<(string NodeId, double X, double Y)> ContextMenuNode { get; set; }

private Task OnContextMenu(MouseEventArgs e)
{
    return ContextMenuNode.InvokeAsync((Node.Id, e.ClientX, e.ClientY));
}
```

**Stap 2: Propageer door naar DesignerShell.**

In `DesignerShell.razor`, voeg `ContextMenuNode` toe aan elke `DesignerCanvasNode`:

```razor
ContextMenuNode="OnCanvasContextMenu"
```

In `DesignerShell.razor.cs`:

```csharp
private string? _canvasContextMenuNodeId;
private string _canvasContextMenuXpx = "0px";
private string _canvasContextMenuYpx = "0px";

private Task OnCanvasContextMenu((string NodeId, double X, double Y) args)
{
    _selectedNodeId = args.NodeId;
    _selectedNodeIds.Clear();
    _canvasContextMenuNodeId = args.NodeId;
    _canvasContextMenuXpx = $"{args.X}px";
    _canvasContextMenuYpx = $"{args.Y}px";
    return InvokeAsync(StateHasChanged);
}

private void CloseCanvasContextMenu()
{
    _canvasContextMenuNodeId = null;
}
```

**Stap 3: Render het context menu op de canvas.**

In `DesignerShell.razor`, in de canvas-sectie (na de canvas content, net als bij het tree context menu):

```razor
@if (_canvasContextMenuNodeId is not null)
{
    <div class="designer-canvas-context-menu"
         role="menu"
         style="position: fixed; left: @_canvasContextMenuXpx; top: @_canvasContextMenuYpx; z-index: 1000;"
         @onclick:stopPropagation="true"
         @onfocusout="CloseCanvasContextMenu">
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextCopy">
            <RadzenIcon Icon="content_copy" /> Kopiëren
        </button>
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextPaste">
            <RadzenIcon Icon="content_paste" /> Plakken
        </button>
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextDuplicate">
            <RadzenIcon Icon="content_copy" /> Dupliceren
        </button>
        <hr class="designer-context-menu-divider" />
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextMoveUp">
            <RadzenIcon Icon="arrow_upward" /> Omhoog
        </button>
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextMoveDown">
            <RadzenIcon Icon="arrow_downward" /> Omlaag
        </button>
        <hr class="designer-context-menu-divider" />
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextWrapCard">
            <RadzenIcon Icon="crop_square" /> Wikkel in kaart
        </button>
        <button type="button" class="designer-tree-context-menu__item" role="menuitem" @onclick="OnCanvasContextWrapRow">
            <RadzenIcon Icon="view_column" /> Wikkel in rij
        </button>
        <hr class="designer-context-menu-divider" />
        <button type="button" class="designer-tree-context-menu__item designer-tree-context-menu__item--danger" role="menuitem" @onclick="OnCanvasContextDelete">
            <RadzenIcon Icon="delete_outline" /> Verwijderen
        </button>
    </div>
}
```

**Stap 4: Context menu handlers.**

```csharp
private async Task OnCanvasContextCopy()
{
    if (_canvasContextMenuNodeId is not null)
    {
        _selectedNodeId = _canvasContextMenuNodeId;
        await CopySelectedNodeAsync();
    }
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextPaste()
{
    await PasteNodeAsync();
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextDuplicate()
{
    if (_canvasContextMenuNodeId is not null)
        await OnDuplicateNode(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextMoveUp()
{
    if (_canvasContextMenuNodeId is not null)
        await OnMoveNodeUp(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextMoveDown()
{
    if (_canvasContextMenuNodeId is not null)
        await OnMoveNodeDown(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextWrapCard()
{
    if (_canvasContextMenuNodeId is not null)
        await OnWrapInCard(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextWrapRow()
{
    if (_canvasContextMenuNodeId is not null)
        await OnWrapInRow(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}

private async Task OnCanvasContextDelete()
{
    if (_canvasContextMenuNodeId is not null)
        await OnDeleteNode(_canvasContextMenuNodeId);
    CloseCanvasContextMenu();
}
```

> **Let op**: `OnWrapInCard` en `OnWrapInRow` bestaan al als tree context menu handlers. Hergebruik dezelfde logica. Als de methode-namen anders zijn, zoek naar `OnTreeContextWrapCardAsync` en roep dezelfde command aan.

**Stap 5: Sluit context menu bij klik elders.**

In de canvas area `@onclick`, voeg toe:
```csharp
CloseCanvasContextMenu();
```

---

## Fase 2 — CSS voor canvas context menu

In `designer.css`:

```css
.designer-canvas-context-menu {
    background: var(--agt-surface-1);
    border: 1px solid var(--agt-border-subtle);
    border-radius: var(--agt-border-radius-md);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
    min-width: 180px;
    padding: var(--agt-spacing-1) 0;
}

.designer-context-menu-divider {
    border: 0;
    border-top: 1px solid var(--agt-border-subtle);
    margin: var(--agt-spacing-1) 0;
}

.designer-tree-context-menu__item--danger {
    color: var(--agt-color-danger-500);
}

.designer-tree-context-menu__item .rz-icon {
    font-size: 16px;
    margin-right: var(--agt-spacing-1);
}
```

---

## Fase 3 — Verbeterd drag-reorder op canvas

### Huidige staat
- Drag-start werkt al (`@ondragstart="OnDragStart"` op de select-knop in de component bar).
- Drop targets bestaan al (dropzones tussen componenten).
- De floating toolbar heeft pijl-knoppen voor omhoog/omlaag.

### Probleem
De drag-handle is het hele component-bar label. Dit is verwarrend: klikken selecteert, draggen verplaatst — dezelfde element doet twee dingen. Er is geen visueel onderscheid.

### Fix

**Stap 1: Scheid drag-handle van selectie-knop.**

In `DesignerCanvasNode.razor`, splits het component-bar:

```razor
<div class="designer-canvas-node__bar">
    <span class="designer-canvas-node__drag-handle"
          draggable="true"
          @ondragstart="OnDragStart"
          @ondragend="OnDragEnd"
          title="Versleep om te verplaatsen">
        <RadzenIcon Icon="drag_indicator" />
    </span>
    <button type="button"
            class="designer-canvas-node__select"
            @onclick="OnSelectClickedExtended"
            @onclick:stopPropagation="true"
            aria-current="@(IsSelected ? "true" : "false")">
        <span class="designer-canvas-node__bar-icon">
            <RadzenIcon Icon="@DescriptorIcon" />
        </span>
        <span>@DescriptorLabel</span>
    </button>
</div>
```

**Stap 2: CSS voor drag handle.**

```css
.designer-canvas-node__drag-handle {
    align-items: center;
    color: var(--agt-text-muted);
    cursor: grab;
    display: flex;
    flex-shrink: 0;
    opacity: 0;
    padding: 0 2px;
    transition: opacity 120ms;
}

.designer-canvas-node__drag-handle:active {
    cursor: grabbing;
}

.designer-canvas-node:hover .designer-canvas-node__drag-handle,
.designer-canvas-node--selected .designer-canvas-node__drag-handle {
    opacity: 1;
}

.designer-canvas-node__drag-handle .rz-icon {
    font-size: 16px;
}
```

**Stap 3: Verwijder `draggable="true"` van de select-knop.**

De select-knop had eerder `draggable="true"` en `@ondragstart`. Verwijder die attributen — drag gaat nu uitsluitend via de drag-handle.

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Components/DesignerCanvasNode.razor` | 1, 3 | `@oncontextmenu`, `ContextMenuNode` EventCallback, drag-handle |
| `Components/DesignerShell.razor` | 1 | Canvas context menu markup, `ContextMenuNode` binding |
| `Components/DesignerShell.razor.cs` | 1 | Context menu state + handlers |
| `wwwroot/css/designer.css` | 2, 3 | Context menu + drag handle CSS |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Rechtermuisklik op een component op canvas | Context menu verschijnt met Kopiëren, Plakken, Dupliceren, etc. |
| 2 | Klik "Kopiëren" → rechtermuisklik elders → "Plakken" | Component gekopieerd naar nieuwe positie |
| 3 | Klik "Verwijderen" in context menu | Component verwijderd |
| 4 | Klik "Wikkel in kaart" | Component gewrapt in AgtCard |
| 5 | Klik ergens anders op canvas | Context menu verdwijnt |
| 6 | Hover over component | Drag-handle (⠿ icoon) verschijnt links van het label |
| 7 | Sleep drag-handle naar andere positie | Component verplaatst |
| 8 | Klik op component-label (niet drag-handle) | Component geselecteerd, niet gesleept |
