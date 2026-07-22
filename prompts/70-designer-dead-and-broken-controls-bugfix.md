# Prompt 70 — Designer broken/dead controls bugfix

Code-analyse heeft uitgewezen dat meerdere UI-controls in de designer stuk zijn of niets doen. Dit zijn geen UX-verbeteringen maar **functionele bugs**: controls die zichtbaar zijn maar niet werken, event-handlers die geregistreerd maar niet aangeroepen worden, en ontbrekende browser-API-calls waardoor HTML5 drag-and-drop faalt.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Bug 1 — Root-canvas dropzones missen `@ondragover:preventDefault` (KRITIEK)

### Symptoom
Componenten slepen vanuit het palet naar het canvas werkt niet als er al nodes zijn. De `ondrop` event vuurt niet.

### Oorzaak
In `DesignerShell.razor` hebben de root-level dropzones (`<div class="@GetRootDropzoneClass(...)">`) wél `@ondragover` maar **geen** `@ondragover:preventDefault="true"`. Zonder `preventDefault` op `dragover` blokkeert de browser de `drop`-event conform de HTML5 drag-and-drop spec. De slot-dropzones in `DesignerCanvasNode.razor` hebben dit wél (via `builder.AddEventPreventDefaultAttribute`), daarom werkt droppen in bestaande slots wél.

### Fix
Voeg `@ondragover:preventDefault="true"` toe aan **alle** root-dropzone divs in `DesignerShell.razor`. Er zijn twee locaties:

1. De dropzone vóór de eerste node (rond regel 297-302):
```razor
<div class="@GetRootDropzoneClass(ActivePage.Nodes.Count)"
     @ondragenter='...'
     @ondragleave='...'
     @ondragover='args => OnDropZoneDragOver(args, $"root-{ActivePage.Nodes.Count}")'
     @ondragover:preventDefault="true"
     @ondrop='...'
     title="Drop op canvas"></div>
```

2. De dropzones tussen/na nodes in de for-loop (rond regel 328-333):
```razor
<div class="@GetRootDropzoneClass(index + 1)"
     @ondragenter='...'
     @ondragleave='...'
     @ondragover='args => OnDropZoneDragOver(args, $"root-{index + 1}")'
     @ondragover:preventDefault="true"
     @ondrop='...'
     title="Drop op canvas"></div>
```

### Verificatie
- Start de designer, sleep een component uit het palet naar een root-dropzone → component wordt toegevoegd.
- Sleep een component tussen twee bestaande root-nodes → component verschijnt op de juiste plek.

---

## Bug 2 — Pagina-tab drag-reorder werkt niet

### Symptoom
Pagina-tabs kunnen niet via drag-and-drop herordend worden. De drag start, maar bij loslaten gebeurt er niets.

### Oorzaak
Twee problemen:
1. `OnPageDragOver` in `DesignerShell.razor.cs` (regel 2578) is een **lege methode**: `private void OnPageDragOver(DragEventArgs args) { }`. Er wordt geen `preventDefault` aangeroepen.
2. In de razor template (regel 165-166) ontbreekt `@ondragover:preventDefault="true"` op de page-tab div. Zonder `preventDefault` op `dragover` vuurt `ondrop` niet.

### Fix
1. Voeg `@ondragover:preventDefault="true"` toe aan de page-tab div in `DesignerShell.razor`:

```razor
<div class="designer-page-tab @(isActive ? "designer-page-tab--active" : null)"
     draggable="true"
     @ondragstart="() => OnPageDragStart(pageIndex)"
     @ondragover="OnPageDragOver"
     @ondragover:preventDefault="true"
     @ondrop="() => OnPageDropAsync(pageIndex)"
     @ondragend="OnPageDragEnd">
```

2. Optioneel: `OnPageDragOver` kan leeg blijven — `preventDefault` op het attribuut is voldoende.

### Verificatie
- Maak 3+ pagina's aan.
- Sleep pagina-tab 1 naar positie 3 → tabs herordenen correct.
- De actieve pagina volgt de versleepte tab.

---

## Bug 3 — Data panel Rijen/Seed wijzigingen updaten preview niet

### Symptoom
In het Data-panel: wijzigen van "Rijen" of "Seed" velden doet niets zichtbaars. De preview-tabel blijft ongewijzigd.

### Oorzaak
In `DesignerDataPanel.razor` (regels 161-162) muteren `OnRowCountChanged` en `OnSeedChanged` de entity seed properties, maar retourneren `Task.CompletedTask` zonder `StateHasChanged()` aan te roepen. Blazor weet niet dat er iets veranderd is.

### Fix
Roep `StateHasChanged()` (of `await InvokeAsync(StateHasChanged)`) aan na het muteren:

```csharp
private Task OnRowCountChanged(decimal? value)
{
    if (CurrentEntity is not null && value.HasValue)
    {
        CurrentEntity.Seed.RowCount = Math.Max(1, Convert.ToInt32(value.Value));
        StateHasChanged();
    }
    return Task.CompletedTask;
}

private Task OnSeedChanged(decimal? value)
{
    if (CurrentEntity is not null && value.HasValue)
    {
        CurrentEntity.Seed.Seed = Math.Max(1, Convert.ToInt32(value.Value));
        StateHasChanged();
    }
    return Task.CompletedTask;
}
```

### Verificatie
- Open het Data-panel, selecteer een entiteit.
- Wijzig "Rijen" van 5 naar 20 → tabel toont nu 20 rijen.
- Wijzig "Seed" → data in de tabel verandert (andere random seed).

---

## Bug 4 — Sneltoets `Ctrl+Shift+P` geregistreerd maar niet afgevangen

### Symptoom
In de command palette staat "Pagina toevoegen" met hint `Ctrl+Shift+P`. Drukken op die toetscombinatie doet niets.

### Oorzaak
`RegisterCommands()` in `DesignerShell.razor.cs` (regel 1596) registreert het commando met `ShortcutHint = "Ctrl+Shift+P"`, maar `OnPageKeyDown` bevat geen handler voor `Ctrl+Shift+P`. De hint is puur visueel.

### Fix
Voeg een handler toe in `OnPageKeyDown`, **vóór** de bestaande `Ctrl+P` (preview) handler:

```csharp
if (args.CtrlKey && args.ShiftKey && string.Equals(args.Key, "p", StringComparison.OrdinalIgnoreCase))
{
    await OnAddPageAsync();
    return;
}
```

Plaats dit vóór het `Ctrl+P` blok (rond regel 1291) zodat `Shift` prioriteit krijgt.

### Verificatie
- Druk `Ctrl+Shift+P` → nieuwe pagina wordt toegevoegd.
- Druk `Ctrl+P` (zonder Shift) → preview mode togglet (bestaand gedrag intact).

---

## Bug 5 — Dubbele command palette: AgtCommandPalette in Instellingen-menu

### Symptoom
Het Instellingen-menu bevat een "Zoeken" knop die een *ander* command palette opent dan `Ctrl+K`. Er zijn twee overlappende command palettes.

### Oorzaak
`DesignerShell.razor` regels 139-142 embedden `<AgtCommandPalette />` (de globale UI-library palette) in het settings menu. Daarnaast heeft de designer zijn eigen command palette (`_showDesignerCommandPalette`), geopend via `Ctrl+K`. De `AgtCommandPalette` registreert zelfs een eigen `Ctrl+K` global shortcut via JS, wat kan conflicteren.

### Fix
Verwijder `<AgtCommandPalette />` uit het settings menu en vervang door een expliciete knop die de designer command palette opent:

```razor
@if (_settingsMenuOpen)
{
    <div class="designer-menu" role="menu" aria-label="Instellingen">
        <button type="button" class="designer-menu__item" role="menuitem" @onclick="ToggleShortcutsOverlay">Sneltoetsen</button>
        <button type="button" class="designer-menu__item" role="menuitem" @onclick="OpenDesignerCommandPaletteAsync">Zoeken (Ctrl+K)</button>
    </div>
}
```

### Verificatie
- Open Instellingen → "Zoeken (Ctrl+K)" → opent dezelfde palette als `Ctrl+K`.
- Geen dubbele palette meer. Geen JS-conflicten voor `Ctrl+K`.

---

## Bug 6 — Dead code: `OnTreeChange` method nooit aangeroepen

### Symptoom
Geen direct symptoom — dode code.

### Oorzaak
`OnTreeChange(TreeEventArgs args)` in `DesignerShell.razor.cs` (regel 432-438) wordt nergens aangeroepen. De structuurboom gebruikt `OnTreeNodeClicked` in plaats daarvan.

### Fix
Verwijder de methode:

```csharp
// VERWIJDER:
private void OnTreeChange(TreeEventArgs args)
{
    if (args.Value is string nodeId)
    {
        _selectedNodeId = nodeId;
    }
}
```

### Verificatie
- Build slaagt zonder fouten.
- Structuurboom klikken werkt nog steeds (via `OnTreeNodeClicked`).

---

## Bug 7 — Copy/paste werkt niet cross-tab (alleen in-memory)

### Symptoom
`Ctrl+C` / `Ctrl+V` werkt binnen dezelfde sessie maar niet tussen tabs of na page refresh.

### Oorzaak
`CopySelectedNode()` slaat op in `_clipboardNodeJson` (in-memory string). Er is geen integratie met de browser clipboard API.

### Fix
Gebruik `navigator.clipboard.writeText()` / `readText()` via JS interop. Voeg toe aan `designer-interop.js`:

```javascript
const copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        return false;
    }
};

const readFromClipboard = async () => {
    try {
        return await navigator.clipboard.readText();
    } catch {
        return null;
    }
};
```

Exporteer beide in de return block. Update `CopySelectedNode()` en `PasteNodeAsync()` in `DesignerShell.razor.cs`:

```csharp
private async Task CopySelectedNodeAsync()
{
    if (_selectedNodeId is null || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index))
    {
        return;
    }

    var json = DesignDocumentSerializer.SerializeNode(container[index]);
    _clipboardNodeJson = json;
    await JS.InvokeVoidAsync("designerInterop.copyToClipboard", json);
    ShowToast("Component gekopieerd", ToastType.Info);
}
```

En update `PasteNodeAsync()` om eerst de browser clipboard te proberen:

```csharp
private async Task PasteNodeAsync()
{
    var json = await JS.InvokeAsync<string?>("designerInterop.readFromClipboard");
    json ??= _clipboardNodeJson;
    if (string.IsNullOrWhiteSpace(json))
    {
        return;
    }
    // ... rest van bestaande paste-logica
}
```

Update ook het keyboard handler blok (`Ctrl+C`) om de async versie aan te roepen:

```csharp
if (args.CtrlKey && string.Equals(args.Key, "c", StringComparison.OrdinalIgnoreCase) && _selectedNodeId is not null)
{
    await CopySelectedNodeAsync();
    return;
}
```

### Verificatie
- Selecteer component, `Ctrl+C`, open nieuw tab met designer, `Ctrl+V` → component geplakt.
- Binnen dezelfde tab: `Ctrl+C` + `Ctrl+V` blijft werken.
- Toast "Component gekopieerd" verschijnt bij kopiëren.

---

## Bug 8 — Kolom toevoegen plaatst in verkeerde positie (KRITIEK)

### Symptoom
Wanneer een `RadzenColumn` geselecteerd is en de gebruiker klikt op "Column" in het palet, wordt de nieuwe kolom **genest in** de geselecteerde kolom (als child in ChildContent) in plaats van als **sibling** in de bovenliggende `RadzenRow`. Visueel lijkt het alsof er een rij wordt toegevoegd in plaats van een kolom naast de bestaande.

### Oorzaak
`ResolvePaletteClickInsertLocation()` in `DesignerShell.razor.cs` (regel 2614-2625) controleert alleen of het geselecteerde component een `ChildContent` slot heeft. Omdat `RadzenColumn` dat slot heeft, wordt de nieuwe kolom daar ingevoegd — maar dat is semantisch verkeerd. Een kolom hoort als sibling in een `RadzenRow`, niet genest in een andere kolom.

```csharp
// Huidige logica — te naïef:
if (SelectedDescriptor?.Slots.Contains("ChildContent", StringComparer.Ordinal) == true)
{
    return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
}
return DesignNodeLocation.Root(ActivePage.Nodes.Count);
```

### Fix
Maak de insert-logica context-bewust. Als het in te voegen component een layout-child is (zoals `RadzenColumn`), zoek dan de juiste parent (zoals `RadzenRow`):

```csharp
private DesignNodeLocation ResolvePaletteClickInsertLocation(string? componentType = null)
{
    if (_selectedNodeId is null)
    {
        return DesignNodeLocation.Root(ActivePage.Nodes.Count);
    }

    // Als we een kolom toevoegen en het geselecteerde element IS een kolom,
    // dan moeten we de kolom als sibling toevoegen in de parent row
    if (IsColumnType(componentType) && IsColumnType(SelectedNode?.ComponentType))
    {
        var parentRow = FindParentOfType(_selectedNodeId, "RadzenRow");
        if (parentRow is not null)
        {
            var siblingCount = parentRow.Children.TryGetValue("ChildContent", out var siblings) ? siblings.Count : 0;
            return new DesignNodeLocation(parentRow.Id, "ChildContent", siblingCount);
        }
    }

    // Als het geselecteerde element een Row is en we voegen een kolom toe, direct erin
    if (IsColumnType(componentType) && string.Equals(SelectedNode?.ComponentType, "RadzenRow", StringComparison.Ordinal))
    {
        return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
    }

    // Standaard: als het geselecteerde element een ChildContent slot heeft, voeg daar toe
    if (SelectedDescriptor?.Slots.Contains("ChildContent", StringComparer.Ordinal) == true)
    {
        return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
    }

    return DesignNodeLocation.Root(ActivePage.Nodes.Count);
}

private static bool IsColumnType(string? componentType)
    => string.Equals(componentType, "RadzenColumn", StringComparison.Ordinal);

private DesignNode? FindParentOfType(string childNodeId, string parentComponentType)
{
    return FindParentOfTypeRecursive(ActivePage.Nodes, childNodeId, parentComponentType, null);
}

private static DesignNode? FindParentOfTypeRecursive(
    IReadOnlyList<DesignNode> nodes, string targetId, string parentType, DesignNode? currentParent)
{
    foreach (var node in nodes)
    {
        if (string.Equals(node.Id, targetId, StringComparison.Ordinal))
        {
            return string.Equals(currentParent?.ComponentType, parentType, StringComparison.Ordinal)
                ? currentParent
                : null;
        }

        foreach (var slot in node.Children.Values)
        {
            var result = FindParentOfTypeRecursive(slot, targetId, parentType, node);
            if (result is not null)
            {
                return result;
            }
        }
    }

    return null;
}
```

Update ook de aanroep in `OnPaletteItemClickedAsync` om het componenttype door te geven:

```csharp
private async Task OnPaletteItemClickedAsync(string componentType)
{
    var location = ResolvePaletteClickInsertLocation(componentType);
    // ... rest ongewijzigd
}
```

### Verificatie
- Selecteer een RadzenColumn in een Row → klik "Column" in palet → nieuwe kolom verschijnt als sibling naast de bestaande kolom.
- Selecteer een RadzenRow → klik "Column" → kolom wordt in de row ingevoegd.
- Selecteer een ander component (bijv. Card) → klik "Tekstveld" → gedrag ongewijzigd (in ChildContent).

---

## Bug 9 — AgtSwitch label-klik togglet niet (label `for` matcht niet)

### Symptoom
Klikken op de labeltekst van "Geavanceerd" of "Meer opties" switches in het properties panel doet niets. Alleen klikken op het kleine switch-element zelf werkt.

### Oorzaak
`AgtSwitch.razor` gebruikt `<label for="@ResolvedName">` waar `ResolvedName` een eigen GUID is (`agt-switch-{guid}`). Maar `RadzenSwitch` genereert intern een eigen `id` voor zijn `<input>` element dat NIET gelijk is aan de `Name` parameter. De `for` van het label wijst dus naar een niet-bestaand element.

HTML spec: een `<label for="X">` togglet het form-element met `id="X"`. Omdat er geen element met die `id` bestaat, doet klikken op het label niets.

### Fix
Verwijder de `for` attribute van het label element en gebruik in plaats daarvan een wrapper-click benadering. De `<label>` zonder `for` maar met het input element erin genest werkt via implicit label association:

In `AgtSwitch.razor`, verwijder `for="@ResolvedName"`:

```razor
<div class="agt-field-wrapper @CssClass" role="group">
    <label class="agt-switch">
        <span class="agt-switch__label">@ResolvedLabel</span>
        <RadzenSwitch Name="@ResolvedName"
                      Value="@Value"
                      ValueChanged="@ValueChanged"
                      Disabled="@Disabled"
                      InputAttributes="@ResolvedInputAttributes" />
        <span class="agt-switch__state" aria-live="polite">@(Value ? OnText : OffText)</span>
    </label>
</div>
```

Door het `for` attribuut te verwijderen en het `<input>` element (gegenereerd door RadzenSwitch) **binnen** het `<label>` te houden, werkt de implicit label association: klikken overal op het label (inclusief de tekst) togglet het input element.

**Let op:** controleer of `RadzenSwitch` een eigen wrapping element rendert dat de implicit association zou blokkeren. Test dit. Als de implicit association niet werkt omdat RadzenSwitch een `<div>` om het input element heen rendert, gebruik dan een expliciete `@onclick` handler:

```razor
<div class="agt-field-wrapper @CssClass" role="group" @onclick="OnWrapperClicked" @onclick:stopPropagation="true">
    <label class="agt-switch">
        <span class="agt-switch__label">@ResolvedLabel</span>
        <RadzenSwitch Name="@ResolvedName"
                      Value="@Value"
                      ValueChanged="@ValueChanged"
                      Disabled="@Disabled"
                      InputAttributes="@ResolvedInputAttributes" />
        <span class="agt-switch__state" aria-live="polite">@(Value ? OnText : OffText)</span>
    </label>
</div>
```

Met in de code:
```csharp
private Task OnWrapperClicked()
{
    if (Disabled) return Task.CompletedTask;
    return ValueChanged.InvokeAsync(!Value);
}
```

**Belangrijk:** deze fallback-click kan een double-toggle veroorzaken als het klik-event ook door RadzenSwitch wordt opgepakt. Test grondig: klik op labeltekst, klik op switch-thumb, klik op de "Aan/Uit" tekst — alle drie moeten exact één toggle produceren.

De veiligste aanpak is het verwijderen van `for` en testen of implicit labeling werkt. Zo niet, gebruik dan de `@onclick` fallback maar met `@onclick:preventDefault="true"` om double-toggle te voorkomen.

### Verificatie
- Klik op "Geavanceerd" tekst → switch togglet, extra parameters verschijnen.
- Klik op "Meer opties" tekst → switch togglet, Layout-sectie verschijnt in Weergave-tab.
- Klik op de switch-thumb zelf → werkt nog steeds correct (geen double-toggle).
- Test alle AgtSwitch-instanties in de designer: property panel, data panel column toggles, issues filters.

---

## Samenvatting wijzigingen per bestand

| Bestand | Bugs |
|---------|------|
| `DesignerShell.razor` | #1 (dropzone preventDefault), #2 (page tab preventDefault), #5 (remove AgtCommandPalette) |
| `DesignerShell.razor.cs` | #4 (Ctrl+Shift+P handler), #6 (remove OnTreeChange), #7 (async copy/paste), #8 (column insert logic) |
| `DesignerDataPanel.razor` | #3 (StateHasChanged in row/seed handlers) |
| `designer-interop.js` | #7 (clipboard API functions) |
| `AgtSwitch.razor` | #9 (label for/id mismatch) |

## Volgorde van uitvoering

1. Bug 1 + Bug 2 (drag-and-drop fixes — kritiek)
2. Bug 8 (column insert locatie — kritiek)
3. Bug 9 (switch label-klik fix — kritiek)
4. Bug 3 (data panel render fix)
5. Bug 4 (shortcut wiring)
6. Bug 5 (duplicate palette cleanup)
7. Bug 6 (dead code removal)
8. Bug 7 (clipboard integration)

Elke bug is een eigen commit met beschrijvende message.
