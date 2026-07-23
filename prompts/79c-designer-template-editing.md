# Prompt 79c — RenderFragment template editing mode

Radzen Studio laat gebruikers op een pencil-icoon klikken naast RenderFragment-properties om in een template edit mode te komen, waar je componenten visueel kunt drag-droppen binnenin de template. De huidige designer behandelt RenderFragments als interne `Children` slots maar biedt geen expliciete manier om templates van Radzen-componenten te bewerken (bijv. `RadzenDataGridColumn.Template`, `RadzenTabsItem.ChildContent`, `RadzenAccordionItem.ChildContent`).

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Context: huidige werking

De designer gebruikt de `Children` dictionary op `DesignNode` voor slots/RenderFragments. `DesignerCanvasNode.razor` itereert over `_descriptor.Slots` en rendert `RenderSlot()` met dropzones. Dit werkt voor Agt-componenten (AgtSidebarLayout, AgtCard) die expliciete slots hebben.

Maar er zijn twee problemen:

1. **Niet alle RenderFragment-parameters zijn geregistreerd als slots.** De component-registry registreert slots alleen als ze in `SlotNames` staan. Radzen-componenten met `RenderFragment` parameters (zoals `Template`, `HeaderTemplate`, `FooterTemplate`) worden niet automatisch als bewerkbare slots getoond.

2. **Er is geen visuele hint dat een RenderFragment bewerkbaar is.** De PropertyPanel filtert RenderFragments volledig weg (`!parameter.IsRenderFragment`).

---

## Fase 1 — Toon RenderFragment parameters als bewerkbare slots

### Doel
Alle `RenderFragment` parameters van een geselecteerde component worden zichtbaar in de PropertyPanel met een klik-om-te-bewerken actie.

### Implementatie

**Stap 1: Voeg een "Slots" sectie toe aan de PropertyPanel.**

In `PropertyPanel.razor`, voeg toe na de grouped parameters:

```razor
@if (RenderFragmentParameters.Count > 0 && _activeSection == InspectorSection.Content)
{
    <section class="designer-properties__group">
        <h3>Sjablonen (slots)</h3>
        @foreach (var fragment in RenderFragmentParameters)
        {
            var slotName = fragment.Name;
            var displayName = GetSlotDisplayName(slotName);
            var hasContent = HasSlotContent(slotName);
            <div class="designer-properties__field designer-slot-entry">
                <div class="designer-properties__field-head">
                    <span class="designer-properties__field-label">
                        <RadzenIcon Icon="@(hasContent ? "edit_note" : "add_circle_outline")" />
                        @displayName
                    </span>
                    <RadzenButton Text="@(hasContent ? "Bewerken" : "Invullen")"
                                  Icon="edit"
                                  ButtonStyle="ButtonStyle.Base"
                                  Variant="Variant.Text"
                                  Click="@(() => NavigateToSlot.InvokeAsync(slotName))" />
                </div>
                @if (hasContent)
                {
                    <div class="designer-properties__hint">@GetSlotChildCount(slotName) component(en)</div>
                }
            </div>
        }
    </section>
}
```

**Stap 2: Helpers.**

```csharp
private IReadOnlyList<ComponentParameterDescriptor> RenderFragmentParameters
    => SelectedDescriptor?.Parameters
        .Where(static p => p.IsRenderFragment)
        .OrderBy(static p => p.Name, StringComparer.Ordinal)
        .ToArray() ?? [];

private bool HasSlotContent(string slotName)
    => SelectedNode?.Children.TryGetValue(slotName, out var children) == true && children.Count > 0;

private int GetSlotChildCount(string slotName)
    => SelectedNode?.Children.TryGetValue(slotName, out var children) == true ? children.Count : 0;

private static string GetSlotDisplayName(string slotName)
    => DesignerDisplayText.GetSlotDisplayName(slotName);

[Parameter]
public EventCallback<string> NavigateToSlot { get; set; }
```

**Stap 3: Implementeer NavigateToSlot in DesignerShell.**

In `DesignerShell.razor`, voeg toe aan `<PropertyPanel>`:
```razor
NavigateToSlot="OnNavigateToSlot"
```

In `DesignerShell.razor.cs`:
```csharp
private async Task OnNavigateToSlot(string slotName)
{
    if (_selectedNodeId is null) return;

    // Ensure the slot exists in Children
    if (TryFindNode(ActivePage.Nodes, _selectedNodeId, out var node, out _, out _))
    {
        if (!node.Children.ContainsKey(slotName))
        {
            node.Children[slotName] = [];
        }
    }

    // Switch to navigator, expand the node's slot
    _leftTab = LeftPanelTab.Navigator;
    // Focus the breadcrumb to show we're inside the slot
    ShowToast($"Bewerk slot: {DesignerDisplayText.GetSlotDisplayName(slotName)}", ToastType.Info);
    await InvokeAsync(StateHasChanged);
}
```

---

## Fase 2 — Auto-detect slots van Radzen-componenten

### Probleem
De component-registry's `SlotNames` is handmatig geconfigureerd. Veel Radzen-componenten met `RenderFragment` parameters worden niet als slots geregistreerd.

### Fix

In de slot-rendering logica van `DesignerCanvasNode.razor`, combineer geregistreerde slots met dynamisch gedetecteerde RenderFragment parameters:

```csharp
private IReadOnlyList<string> EffectiveSlots
{
    get
    {
        if (_descriptor is null) return [];

        var registered = _descriptor.Slots;
        var fromParameters = _descriptor.Parameters
            .Where(static p => p.IsRenderFragment)
            .Select(static p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        // Merge: registered + any RenderFragment parameters not already in slots
        var all = new HashSet<string>(registered, StringComparer.Ordinal);
        foreach (var param in fromParameters)
        {
            all.Add(param);
        }

        return all.OrderBy(static s => s, StringComparer.Ordinal).ToArray();
    }
}
```

Gebruik `EffectiveSlots` in plaats van `_descriptor.Slots` in de rendering-logica.

---

## Fase 3 — Verrijk slot-display namen

### Doel
Meer RenderFragment-namen vertalen naar Nederlands.

### Implementatie

In `DesignerDisplayText.cs` (of waar `GetSlotDisplayName` is gedefinieerd), breid de dictionary uit:

```csharp
// Bestaande:
["ChildContent"] = "Inhoud",
["HeaderActions"] = "Acties",
["Logo"] = "Logo",
["Sidebar"] = "Zijmenu",
["Header"] = "Koptekst",
["Footer"] = "Voettekst",
["Columns"] = "Kolommen",
["Template"] = "Sjabloon",
["EmptyTemplate"] = "Lege weergave",
["HeaderTemplate"] = "Kop-sjabloon",
["FooterTemplate"] = "Voet-sjabloon",
["SummaryTemplate"] = "Samenvatting",

// NIEUW:
["Start"] = "Start-inhoud",
["End"] = "Eind-inhoud",
["PageTitle"] = "Paginatitel",
["Body"] = "Hoofdinhoud",
["Tabs"] = "Tabbladen",
["Items"] = "Items",
["Content"] = "Inhoud",
["Actions"] = "Acties",
["Icon"] = "Icoon",
["Prefix"] = "Prefix",
["Suffix"] = "Suffix",
["ValueTemplate"] = "Waarde-sjabloon",
["GroupHeaderTemplate"] = "Groepskop-sjabloon",
["EditTemplate"] = "Bewerk-sjabloon",
["FilterTemplate"] = "Filter-sjabloon",
["TitleTemplate"] = "Titel-sjabloon",
["DetailRowTemplate"] = "Detail-rij sjabloon"
```

---

## Fase 4 — Visuele indicator op canvas

### Doel
Op de canvas, wanneer een component bewerkbare (maar lege) slots heeft, toon een visuele hint.

### Implementatie

In `DesignerCanvasNode.razor`, na de bestaande slot-rendering, voeg toe voor lege slots:

```razor
@foreach (var slotName in EffectiveSlots)
{
    @if (!Node.Children.ContainsKey(slotName) || Node.Children[slotName].Count == 0)
    {
        <div class="designer-slot-empty-hint">
            <RadzenIcon Icon="add_circle_outline" />
            <span>@GetSlotDisplayName(slotName)</span>
        </div>
    }
}
```

CSS in `designer.css`:

```css
.designer-slot-empty-hint {
    align-items: center;
    border: 1px dashed var(--agt-border-subtle);
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-muted);
    display: flex;
    font-size: 0.75rem;
    gap: var(--agt-spacing-1);
    justify-content: center;
    min-height: 32px;
    opacity: 0.6;
    padding: var(--agt-spacing-1) var(--agt-spacing-2);
}

.designer-slot-entry .rz-icon {
    font-size: 16px;
}
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Components/PropertyPanel.razor` | 1 | Slots-sectie, NavigateToSlot EventCallback |
| `Components/DesignerShell.razor` | 1 | NavigateToSlot doorgeven |
| `Components/DesignerShell.razor.cs` | 1 | `OnNavigateToSlot` method |
| `Components/DesignerCanvasNode.razor` | 2, 4 | `EffectiveSlots`, lege slot hints |
| `Services/DesignerDisplayText.cs` | 3 | Uitgebreide slot-namen dictionary |
| `wwwroot/css/designer.css` | 4 | `.designer-slot-empty-hint` styling |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Selecteer een AgtCard → Inhoud-tab | Slots sectie toont "Inhoud" en "Acties" met Bewerken/Invullen |
| 2 | Klik "Invullen" bij een lege slot | Navigator opent, toast "Bewerk slot: Inhoud" |
| 3 | Selecteer RadzenDataGrid → Inhoud-tab | "Sjabloon", "Lege weergave", etc. zichtbaar als bewerkbare slots |
| 4 | Voeg RadzenTabsItem toe aan canvas | Lege slot hint "Inhoud" zichtbaar met gestippelde rand |
| 5 | Voeg content toe in slot → selecteer parent | Slot toont "1 component(en)" en "Bewerken" knop |
| 6 | Sla op en herlaad | Slot-content behouden |
