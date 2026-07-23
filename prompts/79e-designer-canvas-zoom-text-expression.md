# Prompt 79e — Canvas zoom, Text-items, Expression-items en Expression editor

Drie ontbrekende features die Radzen Studio wel heeft: canvas zoom met responsieve preview, het invoegen van platte tekst en C#-expressies als items, en een expression editor met autocomplete voor data-binding.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Canvas zoom en custom breedte

### Huidige staat
- `_viewport` kan "desktop" (geen beperking) of "mobile" (360px) zijn.
- `ViewportWidths` dictionary: `["desktop"] = 0, ["mobile"] = 360`.
- `CanvasFrameStyle` genereert `max-width: {px}px` wanneer width > 0.

### Fix

**Stap 1: Breid viewport-opties uit.**

In `DesignerShell.razor.cs`, vervang de bestaande `ViewportWidths`:

```csharp
private static readonly Dictionary<string, int> ViewportWidths = new(StringComparer.Ordinal)
{
    ["desktop"] = 0,
    ["tablet"] = 768,
    ["mobile"] = 360
};

private int _canvasZoomPercent = 100;
private int? _customCanvasWidth;
```

**Stap 2: Voeg zoom-controls toe aan de toolbar.**

In `DesignerShell.razor`, na de bestaande viewport-knoppen:

```razor
@* Viewport toggles *@
<button type="button" class="designer-icon-toggle @(string.Equals(_viewport, "desktop", StringComparison.Ordinal) ? "designer-icon-toggle--active" : null)" @onclick='() => SetViewport("desktop")' aria-label="Desktop">
    <RadzenIcon Icon="desktop_windows" />
</button>
<button type="button" class="designer-icon-toggle @(string.Equals(_viewport, "tablet", StringComparison.Ordinal) ? "designer-icon-toggle--active" : null)" @onclick='() => SetViewport("tablet")' aria-label="Tablet">
    <RadzenIcon Icon="tablet" />
</button>
<button type="button" class="designer-icon-toggle @(string.Equals(_viewport, "mobile", StringComparison.Ordinal) ? "designer-icon-toggle--active" : null)" @onclick='() => SetViewport("mobile")' aria-label="Mobiel">
    <RadzenIcon Icon="phone_iphone" />
</button>

<span class="designer-toolbar-separator"></span>

@* Zoom controls *@
<button type="button" class="designer-icon-toggle" @onclick="ZoomOut" title="Uitzoomen" disabled="@(_canvasZoomPercent <= 50)">
    <RadzenIcon Icon="zoom_out" />
</button>
<span class="designer-zoom-label">@(_canvasZoomPercent)%</span>
<button type="button" class="designer-icon-toggle" @onclick="ZoomIn" title="Inzoomen" disabled="@(_canvasZoomPercent >= 200)">
    <RadzenIcon Icon="zoom_in" />
</button>
<button type="button" class="designer-icon-toggle" @onclick="ZoomReset" title="100%">
    <RadzenIcon Icon="fit_screen" />
</button>

@* Custom width *@
<AgtNumericField Style="width: 80px; margin-left: 8px;"
                 Value="@ConvertToDecimal(_customCanvasWidth ?? 0)"
                 ValueChanged="OnCustomWidthChanged"
                 Placeholder="px"
                 Min="0"
                 Step="10" />
```

**Stap 3: Zoom methods.**

```csharp
private void ZoomIn()
{
    _canvasZoomPercent = Math.Min(200, _canvasZoomPercent + 10);
}

private void ZoomOut()
{
    _canvasZoomPercent = Math.Max(50, _canvasZoomPercent - 10);
}

private void ZoomReset()
{
    _canvasZoomPercent = 100;
}

private void OnCustomWidthChanged(decimal? value)
{
    _customCanvasWidth = value is > 0 ? (int)value : null;
    _viewport = _customCanvasWidth.HasValue ? "custom" : "desktop";
}
```

**Stap 4: Pas `CanvasFrameStyle` aan.**

```csharp
private string? CanvasFrameStyle
{
    get
    {
        var parts = new List<string>();

        // Width
        var width = _customCanvasWidth
            ?? (ViewportWidths.TryGetValue(_viewport, out var w) ? w : 0);
        if (width > 0)
            parts.Add($"max-width: {width}px");

        // Zoom
        if (_canvasZoomPercent != 100)
        {
            var scale = _canvasZoomPercent / 100.0;
            parts.Add($"transform: scale({scale.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})");
            parts.Add("transform-origin: top center");
        }

        return parts.Count > 0 ? string.Join("; ", parts) : null;
    }
}
```

**Stap 5: CSS.**

```css
.designer-zoom-label {
    color: var(--agt-text-muted);
    font-size: 0.75rem;
    min-width: 36px;
    text-align: center;
}

.designer-toolbar-separator {
    background: var(--agt-border-subtle);
    height: 20px;
    margin: 0 var(--agt-spacing-2);
    width: 1px;
}
```

---

## Fase 2 — Text-item (platte tekst invoegen)

### Doel
Gebruikers moeten platte tekst kunnen invoegen op de canvas — koppen, labels, paragrafen — zonder een component te hoeven kiezen.

### Implementatie

**Stap 1: Maak een DesignTextNode component.**

Dit is geen apart Blazor-component maar een speciaal componenttype in de registry. Registreer `DesignText` als pseudo-component:

In `DesignerComponentRegistry.g.cs`, voeg toe (of in een aparte registratie-methode):

```csharp
RegisterDesignerPseudoComponent(
    "DesignText",
    "Tekst",
    "text_fields",
    "HTML",
    [
        new ComponentParameterDescriptor("Text", typeof(string), false, false, false, false, "Uw tekst hier"),
        new ComponentParameterDescriptor("Tag", typeof(string), false, false, false, false, "p"),
        new ComponentParameterDescriptor("CssClass", typeof(string), false, false, false, false, null)
    ],
    []
);
```

**Stap 2: Render DesignText in DesignerCanvasNode.**

In `DesignerCanvasNode.razor`, in de rendering-logica, voeg een special case toe:

```csharp
if (string.Equals(Node.ComponentType, "DesignText", StringComparison.Ordinal))
{
    // Render as raw HTML text element
    var text = Node.Parameters.TryGetValue("Text", out var textParam) ? textParam?.Literal?.GetValue<string>() ?? "Tekst" : "Tekst";
    var tag = Node.Parameters.TryGetValue("Tag", out var tagParam) ? tagParam?.Literal?.GetValue<string>() ?? "p" : "p";

    builder.OpenElement(sequence++, tag);
    if (Node.Parameters.TryGetValue("CssClass", out var cssParam))
    {
        var cssClass = cssParam?.Literal?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(cssClass))
            builder.AddAttribute(sequence++, "class", cssClass);
    }
    builder.AddContent(sequence++, text);
    builder.CloseElement();
}
```

**Stap 3: Voeg toe aan het palette.**

In de palette-categorisatie, voeg "DesignText" toe aan de "HTML" categorie zodat het bovenaan verschijnt.

**Stap 4: Maak inline editing extra intuïtief.**

DesignText moet automatisch in inline-edit mode gaan bij dubbelklik. De bestaande inline-edit logica in `DesignerCanvasNode` ondersteunt dit al voor "Text", "Label", etc. — controleer dat "Text" in `InlineEditableNames` staat (het staat er al).

---

## Fase 3 — Expression-item (dynamische C# waarde)

### Doel
Gebruikers moeten een `@variabele` expressie kunnen invoegen die in de geëxporteerde code een dynamische waarde toont.

### Implementatie

Registreer `DesignExpression` als pseudo-component:

```csharp
RegisterDesignerPseudoComponent(
    "DesignExpression",
    "Expressie",
    "code",
    "HTML",
    [
        new ComponentParameterDescriptor("Expression", typeof(string), false, false, false, false, "@DateTime.Now"),
        new ComponentParameterDescriptor("Tag", typeof(string), false, false, false, false, "span")
    ],
    []
);
```

In `DesignerCanvasNode.razor`, render het als een code-stijl preview:

```csharp
if (string.Equals(Node.ComponentType, "DesignExpression", StringComparison.Ordinal))
{
    var expression = Node.Parameters.TryGetValue("Expression", out var exprParam) ? exprParam?.Literal?.GetValue<string>() ?? "@..." : "@...";
    var tag = Node.Parameters.TryGetValue("Tag", out var tagParam) ? tagParam?.Literal?.GetValue<string>() ?? "span" : "span";

    builder.OpenElement(sequence++, tag);
    builder.AddAttribute(sequence++, "class", "designer-expression-preview");
    builder.AddContent(sequence++, expression);
    builder.CloseElement();
}
```

CSS:

```css
.designer-expression-preview {
    background: var(--agt-alpha-primary-10);
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-color-primary-500);
    font-family: var(--agt-font-mono, monospace);
    font-size: 0.85rem;
    padding: 2px 6px;
}
```

In de `ProjectExporter`, genereer DesignExpression als:

```razor
<@Tag>@Expression</@Tag>
```

Bijvoorbeeld: `<span>@DateTime.Now</span>`.

---

## Fase 4 — Expression editor met autocomplete voor data binding

### Huidige staat
De PropertyPanel heeft entity/field dropdowns voor data binding (regels 200-226), en een "Vrije invoer" toggle die naar een gewone AgtTextField wisselt. Er is geen autocomplete en geen expression picker.

### Fix

**Stap 1: Verbeter de "Vrije invoer" modus met suggesties.**

Wanneer de gebruiker in vrije-invoermodus typt en het veld begint met `@`, toon een suggestielijst:

In `PropertyPanel.razor`, vervang de vrije-invoer textfield met een autocomplete:

```razor
@if (IsManualBindingMode(parameter))
{
    <div class="designer-expression-editor">
        <AgtTextField Label="Expressie"
                      AriaLabel="Binding expressie"
                      Value="@GetExpressionText(parameter)"
                      ValueChanged="@(v => OnExpressionChanged(parameter, v))"
                      Placeholder="@entities.Klant.Select(k => k.Naam)" />
        @if (_expressionSuggestions.Count > 0)
        {
            <div class="designer-expression-suggestions">
                @foreach (var suggestion in _expressionSuggestions)
                {
                    <button type="button" class="designer-expression-suggestion"
                            @onclick="@(() => ApplyExpressionSuggestion(parameter, suggestion))">
                        @suggestion
                    </button>
                }
            </div>
        }
    </div>
}
```

**Stap 2: Genereer suggesties op basis van het datamodel.**

```csharp
private List<string> _expressionSuggestions = [];

private void OnExpressionChanged(ComponentParameterDescriptor parameter, string? value)
{
    _expressionSuggestions = GenerateExpressionSuggestions(value).ToList();
    if (!string.IsNullOrWhiteSpace(value))
    {
        var paramValue = new DesignParameterValue { Expression = value };
        _ = SetNodeParameter.InvokeAsync((parameter, paramValue));
    }
}

private IEnumerable<string> GenerateExpressionSuggestions(string? input)
{
    if (string.IsNullOrWhiteSpace(input)) yield break;

    var trimmed = input.TrimStart('@');

    // Suggest entity paths
    foreach (var entity in DataModel.Entities)
    {
        var entityPath = $"@entities.{entity.Name}";
        if (entityPath.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
        {
            yield return entityPath;

            // Suggest field paths
            foreach (var field in entity.Fields.Take(5))
            {
                yield return $"{entityPath}.Select(item => item.{field.Name})";
            }
        }
    }
}

private Task ApplyExpressionSuggestion(ComponentParameterDescriptor parameter, string suggestion)
{
    _expressionSuggestions.Clear();
    var paramValue = new DesignParameterValue { Expression = suggestion };
    return SetNodeParameter.InvokeAsync((parameter, paramValue));
}

private string? GetExpressionText(ComponentParameterDescriptor parameter)
{
    if (SelectedNode is null) return null;
    if (!SelectedNode.Parameters.TryGetValue(parameter.Name, out var value)) return null;
    return value?.Expression;
}
```

**Stap 3: CSS voor suggesties.**

```css
.designer-expression-editor {
    position: relative;
}

.designer-expression-suggestions {
    background: var(--agt-surface-1);
    border: 1px solid var(--agt-border-subtle);
    border-radius: var(--agt-border-radius-sm);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
    max-height: 200px;
    overflow-y: auto;
    position: absolute;
    width: 100%;
    z-index: 100;
}

.designer-expression-suggestion {
    background: none;
    border: 0;
    color: var(--agt-text-body);
    cursor: pointer;
    display: block;
    font-family: var(--agt-font-mono, monospace);
    font-size: 0.8rem;
    padding: var(--agt-spacing-1) var(--agt-spacing-2);
    text-align: left;
    width: 100%;
}

.designer-expression-suggestion:hover {
    background: var(--agt-alpha-primary-10);
}
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Components/DesignerShell.razor` | 1 | Zoom controls, tablet toggle, custom width |
| `Components/DesignerShell.razor.cs` | 1 | Zoom state, custom width, `CanvasFrameStyle` |
| `Registry/DesignerComponentRegistry.g.cs` | 2, 3 | DesignText + DesignExpression pseudo-componenten |
| `Components/DesignerCanvasNode.razor` | 2, 3 | Special-case rendering voor DesignText en DesignExpression |
| `Components/PropertyPanel.razor` | 4 | Expression editor met autocomplete |
| `Export/ProjectExporter.cs` | 2, 3 | DesignText → HTML, DesignExpression → @expression |
| `wwwroot/css/designer.css` | 1, 3, 4 | Zoom label, expression preview, suggestions CSS |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Klik zoom-in knop | Canvas wordt 110%, label toont "110%" |
| 2 | Klik zoom-out 3× | Canvas op 70%, layout zichtbaar in kleiner formaat |
| 3 | Klik fit-screen | Terug naar 100% |
| 4 | Selecteer "Tablet" viewport | Canvas max-width 768px |
| 5 | Voer 1024 in custom width veld | Canvas max-width 1024px |
| 6 | Sleep "Tekst" uit palette naar canvas | Tekst-element verschijnt met "Uw tekst hier" |
| 7 | Dubbelklik op tekst | Inline editing actief |
| 8 | Sleep "Expressie" uit palette naar canvas | Expressie-preview met `@DateTime.Now` |
| 9 | Selecteer een AgtDropdown, open Inhoud-tab | Data binding met entity/field dropdowns |
| 10 | Schakel "Vrije invoer" in, typ "@ent" | Suggesties verschijnen met entity-paden |
| 11 | Klik suggestie "@entities.Klant" | Expressie ingevuld, suggesties verdwijnen |
| 12 | Exporteer project | DesignText → `<p>Tekst</p>`, DesignExpression → `<span>@DateTime.Now</span>` |
