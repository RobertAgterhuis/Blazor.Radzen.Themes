# Prompt 79a — Inline style editor en custom attributen

Radzen Studio heeft een Styles-tab in de Property Grid waarmee gebruikers visueel CSS-eigenschappen instellen. De huidige designer heeft nul style-editing — gebruikers kunnen geen padding, margin, breedte, hoogte, achtergrondkleur of tekstkleur aanpassen. Dit is de grootste UX-kloof vergeleken met elke serieuze visuele designer.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Uitbreiding DesignNode met Style en Attributes

### Probleem
`DesignNode` heeft alleen `Parameters`, `Children`, en `LayoutSlot`. Er is geen plek om inline styles of custom attributen op te slaan.

### Fix

Breid `DesignNode` uit in `src/Agterhuis.Ui.Designer/Model/DesignNode.cs`:

```csharp
public sealed class DesignNode
{
    public string Id { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public Dictionary<string, DesignParameterValue> Parameters { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, List<DesignNode>> Children { get; set; } = new(StringComparer.Ordinal);
    public DesignLayoutSlot? LayoutSlot { get; set; }

    // NIEUW:
    /// <summary>
    /// Inline CSS style properties. Key = CSS property name (e.g. "padding"), Value = CSS value (e.g. "8px").
    /// Rendered as a single style="" attribute on the wrapper div.
    /// </summary>
    public Dictionary<string, string> InlineStyles { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Custom HTML/Blazor attributes (e.g. data-testid, role, aria-label).
    /// Applied to the component via [Parameter(CaptureUnmatchedValues = true)] or the wrapper div.
    /// </summary>
    public Dictionary<string, string> CustomAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
```

Update `DesignDocumentSerializer` en `DesignDocumentDeserializer` om `InlineStyles` en `CustomAttributes` te (de)serialiseren. Gebruik dezelfde JSON-structuur:

```json
{
  "id": "node-1",
  "componentType": "AgtTextField",
  "parameters": { ... },
  "inlineStyles": { "padding": "8px", "margin-bottom": "16px" },
  "customAttributes": { "data-testid": "email-field" }
}
```

Zorg dat lege dictionaries niet geserialiseerd worden (geen `"inlineStyles": {}` in de JSON).

---

## Fase 2 — Style-editor in PropertyPanel

### Doel
Een vierde tab "Stijl" in de PropertyPanel inspector waarmee de gebruiker visueel CSS-eigenschappen instelt.

### Implementatie

**Stap 1: Voeg de tab toe.**

In `PropertyPanel.razor`, voeg een vierde tab toe na "Weergave":

```razor
<button type="button"
        role="tab"
        class="@TabClass(InspectorSection.Style)"
        aria-selected="@(IsActiveSection(InspectorSection.Style))"
        @onclick="() => SetActiveSection(InspectorSection.Style)">Stijl</button>
```

Voeg `Style` toe aan de `InspectorSection` enum:

```csharp
private enum InspectorSection
{
    Content,
    Interaction,
    Appearance,
    Style  // NIEUW
}
```

**Stap 2: Bouw de style-editor UI.**

In `PropertyPanel.razor`, na de bestaande section-content:

```razor
@if (_activeSection == InspectorSection.Style && SelectedNode is not null)
{
    <section class="designer-properties__group">
        <h3>Afmetingen</h3>
        <div class="designer-properties__layout-grid">
            <AgtTextField Label="Breedte" AriaLabel="CSS breedte"
                          Value="@GetStyle("width")"
                          ValueChanged="@(v => SetStyle("width", v))"
                          Placeholder="bijv. 100%, 200px, auto" />
            <AgtTextField Label="Hoogte" AriaLabel="CSS hoogte"
                          Value="@GetStyle("height")"
                          ValueChanged="@(v => SetStyle("height", v))"
                          Placeholder="bijv. auto, 300px" />
            <AgtTextField Label="Min. breedte" AriaLabel="CSS min-width"
                          Value="@GetStyle("min-width")"
                          ValueChanged="@(v => SetStyle("min-width", v))" />
            <AgtTextField Label="Max. breedte" AriaLabel="CSS max-width"
                          Value="@GetStyle("max-width")"
                          ValueChanged="@(v => SetStyle("max-width", v))" />
        </div>
    </section>

    <section class="designer-properties__group">
        <h3>Spacing</h3>
        <div class="designer-properties__layout-grid">
            <AgtTextField Label="Padding" AriaLabel="CSS padding"
                          Value="@GetStyle("padding")"
                          ValueChanged="@(v => SetStyle("padding", v))"
                          Placeholder="bijv. 8px, 16px 24px" />
            <AgtTextField Label="Margin" AriaLabel="CSS margin"
                          Value="@GetStyle("margin")"
                          ValueChanged="@(v => SetStyle("margin", v))"
                          Placeholder="bijv. 0 auto, 16px 0" />
            <AgtTextField Label="Gap" AriaLabel="CSS gap"
                          Value="@GetStyle("gap")"
                          ValueChanged="@(v => SetStyle("gap", v))" />
        </div>
    </section>

    <section class="designer-properties__group">
        <h3>Typografie</h3>
        <div class="designer-properties__layout-grid">
            <AgtTextField Label="Lettergrootte" AriaLabel="CSS font-size"
                          Value="@GetStyle("font-size")"
                          ValueChanged="@(v => SetStyle("font-size", v))"
                          Placeholder="bijv. 14px, 1rem" />
            <AgtDropdown TValue="string" Label="Letterdikte" AriaLabel="CSS font-weight"
                         Data="FontWeightOptions"
                         Value="@GetStyle("font-weight")"
                         ValueChanged="@(v => SetStyle("font-weight", v))"
                         TextProperty="Label" ValueProperty="Value" />
            <AgtDropdown TValue="string" Label="Tekstuitlijning" AriaLabel="CSS text-align"
                         Data="TextAlignOptions"
                         Value="@GetStyle("text-align")"
                         ValueChanged="@(v => SetStyle("text-align", v))"
                         TextProperty="Label" ValueProperty="Value" />
        </div>
    </section>

    <section class="designer-properties__group">
        <h3>Kleuren</h3>
        <div class="designer-properties__layout-grid">
            <AgtDropdown TValue="string" Label="Achtergrond" AriaLabel="CSS background"
                         Data="BackgroundColorOptions"
                         Value="@GetStyle("background")"
                         ValueChanged="@(v => SetStyle("background", v))"
                         TextProperty="Label" ValueProperty="Value" />
            <AgtDropdown TValue="string" Label="Tekstkleur" AriaLabel="CSS color"
                         Data="TextColorOptions"
                         Value="@GetStyle("color")"
                         ValueChanged="@(v => SetStyle("color", v))"
                         TextProperty="Label" ValueProperty="Value" />
        </div>
        <div class="designer-properties__hint">Gebruik design tokens voor thema-compatibiliteit.</div>
    </section>

    <section class="designer-properties__group">
        <h3>Border</h3>
        <div class="designer-properties__layout-grid">
            <AgtTextField Label="Border" AriaLabel="CSS border"
                          Value="@GetStyle("border")"
                          ValueChanged="@(v => SetStyle("border", v))"
                          Placeholder="bijv. 1px solid var(--agt-border-subtle)" />
            <AgtTextField Label="Border-radius" AriaLabel="CSS border-radius"
                          Value="@GetStyle("border-radius")"
                          ValueChanged="@(v => SetStyle("border-radius", v))"
                          Placeholder="bijv. 8px, var(--agt-border-radius-md)" />
        </div>
    </section>

    <section class="designer-properties__group">
        <h3>Layout</h3>
        <div class="designer-properties__layout-grid">
            <AgtDropdown TValue="string" Label="Display" AriaLabel="CSS display"
                         Data="DisplayOptions"
                         Value="@GetStyle("display")"
                         ValueChanged="@(v => SetStyle("display", v))"
                         TextProperty="Label" ValueProperty="Value" />
            <AgtDropdown TValue="string" Label="Flex-richting" AriaLabel="CSS flex-direction"
                         Data="FlexDirectionOptions"
                         Value="@GetStyle("flex-direction")"
                         ValueChanged="@(v => SetStyle("flex-direction", v))"
                         TextProperty="Label" ValueProperty="Value" />
            <AgtDropdown TValue="string" Label="Uitlijning" AriaLabel="CSS align-items"
                         Data="AlignItemsOptions"
                         Value="@GetStyle("align-items")"
                         ValueChanged="@(v => SetStyle("align-items", v))"
                         TextProperty="Label" ValueProperty="Value" />
            <AgtDropdown TValue="string" Label="Verdeling" AriaLabel="CSS justify-content"
                         Data="JustifyContentOptions"
                         Value="@GetStyle("justify-content")"
                         ValueChanged="@(v => SetStyle("justify-content", v))"
                         TextProperty="Label" ValueProperty="Value" />
        </div>
    </section>

    <section class="designer-properties__group">
        <h3>Vrije CSS</h3>
        <div class="designer-properties__hint">Voeg willekeurige CSS-properties toe.</div>
        @foreach (var entry in GetFreeStyleEntries())
        {
            <div class="designer-properties__layout-grid">
                <AgtTextField Label="Property" Value="@entry.Key" ValueChanged="@(v => RenameStyle(entry.Key, v))" />
                <AgtTextField Label="Waarde" Value="@entry.Value" ValueChanged="@(v => SetStyle(entry.Key, v))" />
                <RadzenButton Icon="close" ButtonStyle="ButtonStyle.Base" Variant="Variant.Text" Click="@(() => RemoveStyle(entry.Key))" title="Verwijderen" />
            </div>
        }
        <RadzenButton Text="CSS-property toevoegen" Icon="add" ButtonStyle="ButtonStyle.Base" Variant="Variant.Text" Click="AddFreeStyle" />
    </section>
}
```

**Stap 3: Voeg helpers toe in `PropertyPanel.razor` `@code` blok.**

```csharp
// Style helpers
private string? GetStyle(string property)
    => SelectedNode?.InlineStyles.GetValueOrDefault(property);

private Task SetStyle(string property, string? value)
{
    if (SelectedNode is null) return Task.CompletedTask;

    if (string.IsNullOrWhiteSpace(value))
        SelectedNode.InlineStyles.Remove(property);
    else
        SelectedNode.InlineStyles[property] = value.Trim();

    return SetNodeStyleChanged.InvokeAsync(SelectedNode.InlineStyles);
}

private Task RemoveStyle(string property)
{
    if (SelectedNode is null) return Task.CompletedTask;
    SelectedNode.InlineStyles.Remove(property);
    return SetNodeStyleChanged.InvokeAsync(SelectedNode.InlineStyles);
}

private Task RenameStyle(string oldKey, string? newKey)
{
    if (SelectedNode is null || string.IsNullOrWhiteSpace(newKey)) return Task.CompletedTask;
    if (SelectedNode.InlineStyles.Remove(oldKey, out var value))
        SelectedNode.InlineStyles[newKey.Trim()] = value;
    return SetNodeStyleChanged.InvokeAsync(SelectedNode.InlineStyles);
}

private Task AddFreeStyle()
{
    if (SelectedNode is null) return Task.CompletedTask;
    var key = $"custom-{SelectedNode.InlineStyles.Count + 1}";
    SelectedNode.InlineStyles[key] = string.Empty;
    return Task.CompletedTask;
}

private IReadOnlyList<KeyValuePair<string, string>> GetFreeStyleEntries()
{
    if (SelectedNode is null) return [];
    var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "width", "height", "min-width", "max-width",
        "padding", "margin", "gap",
        "font-size", "font-weight", "text-align",
        "background", "color",
        "border", "border-radius",
        "display", "flex-direction", "align-items", "justify-content"
    };
    return SelectedNode.InlineStyles
        .Where(kv => !known.Contains(kv.Key))
        .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

// Dropdown options
private static readonly OptionItem[] FontWeightOptions =
[
    new("Normaal", "normal"), new("Vet", "bold"),
    new("Licht", "300"), new("Medium", "500"), new("Zwaar", "700")
];

private static readonly OptionItem[] TextAlignOptions =
[
    new("Links", "left"), new("Midden", "center"),
    new("Rechts", "right"), new("Uitvullen", "justify")
];

private static readonly OptionItem[] DisplayOptions =
[
    new("Block", "block"), new("Flex", "flex"),
    new("Grid", "grid"), new("Inline", "inline"),
    new("Inline-flex", "inline-flex"), new("Geen", "none")
];

private static readonly OptionItem[] FlexDirectionOptions =
[
    new("Rij", "row"), new("Kolom", "column"),
    new("Rij (omgekeerd)", "row-reverse"), new("Kolom (omgekeerd)", "column-reverse")
];

private static readonly OptionItem[] AlignItemsOptions =
[
    new("Start", "flex-start"), new("Midden", "center"),
    new("Einde", "flex-end"), new("Stretch", "stretch")
];

private static readonly OptionItem[] JustifyContentOptions =
[
    new("Start", "flex-start"), new("Midden", "center"),
    new("Einde", "flex-end"), new("Tussenruimte", "space-between"),
    new("Gelijkmatig", "space-evenly")
];

private static readonly OptionItem[] BackgroundColorOptions =
[
    new("Transparant", "transparent"),
    new("Surface 0", "var(--agt-surface-0)"),
    new("Surface 1", "var(--agt-surface-1)"),
    new("Surface 2", "var(--agt-surface-2)"),
    new("Primair (licht)", "var(--agt-alpha-primary-10)"),
    new("Danger (licht)", "var(--agt-alpha-danger-10)")
];

private static readonly OptionItem[] TextColorOptions =
[
    new("Body", "var(--agt-text-body)"),
    new("Heading", "var(--agt-text-heading)"),
    new("Muted", "var(--agt-text-muted)"),
    new("Primair", "var(--agt-color-primary-500)"),
    new("Danger", "var(--agt-color-danger-500)"),
    new("Succes", "var(--agt-color-success-500)")
];
```

**Stap 4: Voeg de `SetNodeStyleChanged` EventCallback toe.**

In `PropertyPanel.razor`:
```csharp
[Parameter]
public EventCallback<Dictionary<string, string>> SetNodeStyleChanged { get; set; }
```

In `DesignerShell.razor`, voeg toe aan de `<PropertyPanel>`:
```razor
SetNodeStyleChanged="OnNodeStyleChanged"
```

In `DesignerShell.razor.cs`:
```csharp
private async Task OnNodeStyleChanged(Dictionary<string, string> styles)
{
    if (_selectedNodeId is null || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out var node, out _, out _))
        return;

    node.InlineStyles = styles;
    _hasRecoveredDraft = true;
    await AutoSaveAsync();
}
```

---

## Fase 3 — Style renderen op canvas

### Doel
De inline styles moeten zichtbaar zijn op de canvas en in de export.

### Implementatie

In `DesignerCanvasNode.razor`, pas de root div aan:

```razor
<div class="designer-canvas-node ..."
     data-agt-design-node-id="@Node.Id"
     data-agt-design-component="@Node.ComponentType"
     style="@GetNodeStyle()">
```

```csharp
private string? GetNodeStyle()
{
    var columnStyle = GetColumnFlexStyle();
    var inlineStyle = Node.InlineStyles.Count > 0
        ? string.Join("; ", Node.InlineStyles.Select(kv => $"{kv.Key}: {kv.Value}"))
        : null;

    return (columnStyle, inlineStyle) switch
    {
        (null, null) => null,
        (not null, null) => columnStyle,
        (null, not null) => inlineStyle,
        _ => $"{columnStyle}; {inlineStyle}"
    };
}
```

In `ProjectExporter`, bij het genereren van het geëxporteerde component, voeg de `style` attribute toe:

```csharp
if (node.InlineStyles.Count > 0)
{
    var styleValue = string.Join("; ", node.InlineStyles.Select(kv => $"{kv.Key}: {kv.Value}"));
    lines.Add($"    style=\"{styleValue}\"");
}
```

---

## Fase 4 — Custom attributen editor

### Doel
Gebruikers moeten willekeurige HTML-attributen kunnen toevoegen (data-*, role, aria-*, id).

### Implementatie

Voeg een "Attributen" sectie toe aan de Style-tab in PropertyPanel:

```razor
<section class="designer-properties__group">
    <h3>HTML-attributen</h3>
    @foreach (var attr in SelectedNode.CustomAttributes)
    {
        <div class="designer-properties__layout-grid">
            <AgtTextField Label="Naam" Value="@attr.Key" ValueChanged="@(v => RenameAttribute(attr.Key, v))" />
            <AgtTextField Label="Waarde" Value="@attr.Value" ValueChanged="@(v => SetAttribute(attr.Key, v))" />
            <RadzenButton Icon="close" ButtonStyle="ButtonStyle.Base" Variant="Variant.Text" Click="@(() => RemoveAttribute(attr.Key))" />
        </div>
    }
    <RadzenButton Text="Attribuut toevoegen" Icon="add" ButtonStyle="ButtonStyle.Base" Variant="Variant.Text" Click="AddAttribute" />
</section>
```

Voeg een `SetNodeAttributesChanged` EventCallback toe en implementeer het patroon identiek aan de style-helpers.

In `DesignerCanvasNode.razor`, render de custom attributes:

```csharp
// In BuildParameterDictionary, als het component [Parameter(CaptureUnmatchedValues = true)] heeft:
if (descriptor.AcceptsUnmatchedValues && Node.CustomAttributes.Count > 0)
{
    var additionalAttributes = new Dictionary<string, object>(Node.CustomAttributes.Count, StringComparer.OrdinalIgnoreCase);
    foreach (var (key, value) in Node.CustomAttributes)
    {
        additionalAttributes[key] = value;
    }
    parameters["Attributes"] = additionalAttributes;
}
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Model/DesignNode.cs` | 1 | `InlineStyles` + `CustomAttributes` dictionaries |
| `Serialization/DesignDocumentSerializer.cs` | 1 | Serialisatie inlineStyles, customAttributes |
| `Serialization/DesignDocumentDeserializer.cs` | 1 | Deserialisatie |
| `Components/PropertyPanel.razor` | 2, 4 | Style-tab, attributen-sectie, helpers, options |
| `Components/DesignerShell.razor` | 2, 4 | EventCallbacks doorgeven |
| `Components/DesignerShell.razor.cs` | 2, 4 | `OnNodeStyleChanged`, `OnNodeAttributesChanged` |
| `Components/DesignerCanvasNode.razor` | 3, 4 | `GetNodeStyle()`, custom attributes rendering |
| `Export/ProjectExporter.cs` | 3, 4 | Style + attributes in export |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Selecteer een AgtCard, open Stijl-tab | Afmetingen, Spacing, Typografie, etc. secties zichtbaar |
| 2 | Zet padding op "16px" | Card op canvas toont extra padding |
| 3 | Zet achtergrond op "var(--agt-surface-1)" | Card achtergrondkleur verandert |
| 4 | Voeg custom CSS property "opacity" = "0.8" toe | Component wordt semi-transparant |
| 5 | Voeg HTML-attribuut "data-testid" = "my-card" toe | Attribuut zichtbaar in export |
| 6 | Sla document op, herlaad | Styles en attributen behouden |
| 7 | Exporteer project | Gegenereerde code bevat style="" en custom attributen |
