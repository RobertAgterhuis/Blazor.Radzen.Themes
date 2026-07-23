# Prompt 78b — Canvas live data: componenten tonen realistische data in design-mode

Afhankelijk van: prompt 78a (layout containment).

Het kernprobleem van de designer is dat componenten op de canvas leeg zijn. Een `AgtTextField` toont een lege input. Een `RadzenDataGrid` toont niets. De canvas ziet eruit als een wireframe in plaats van een werkende app. Deze prompt maakt de canvas WYSIWYG door seeded data te injecteren.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — DesignDataContext service (fundament)

### Doel
Een service die seeded data beschikbaar maakt voor componenten op de canvas, zonder de component-registratie of het design-model te wijzigen.

### Implementatie

Maak `src/Agterhuis.Ui.Designer/Services/DesignDataContext.cs`:

```csharp
using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Services;

/// <summary>
/// Provides seeded data to components rendered on the designer canvas.
/// Cascaded from DesignerShell so canvas-node renderers can inject
/// realistic placeholder values without requiring explicit data bindings.
/// </summary>
public sealed class DesignDataContext
{
    public DesignDataModel DataModel { get; }

    public DesignDataContext(DesignDataModel dataModel)
    {
        ArgumentNullException.ThrowIfNull(dataModel);
        DataModel = dataModel;
    }

    /// <summary>
    /// Returns preview rows for an entity. Canvas shows max 5, preview mode max 25.
    /// </summary>
    public IReadOnlyList<DesignSeedRow> GetRows(string entityName, int maxRows = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        return DesignDataModelSeeder.GeneratePreview(DataModel, entityName)
            .Take(maxRows)
            .ToList();
    }

    /// <summary>
    /// Returns a single sample value for a field. Used by form components
    /// to show a realistic placeholder on the canvas.
    /// </summary>
    public object? GetSampleValue(string entityName, string fieldName)
    {
        var rows = GetRows(entityName, 1);
        if (rows.Count == 0) return null;
        return rows[0].Values.GetValueOrDefault(fieldName);
    }

    /// <summary>
    /// Returns all entity names in the data model.
    /// </summary>
    public IReadOnlyList<string> EntityNames
        => DataModel.Entities.Select(static e => e.Name).ToList();
}
```

### Cascade vanuit DesignerShell

In `DesignerShell.razor.cs`, voeg een veld toe:
```csharp
private DesignDataContext? _designDataContext;
```

In `OnInitialized` of na document-load, initialiseer:
```csharp
_designDataContext = new DesignDataContext(_commands.Document.DataModel);
```

Update ook bij document-wisseling (na `ApplyLoadedDocument`, `OnNewDocumentCreated`, etc.):
```csharp
_designDataContext = new DesignDataContext(_commands.Document.DataModel);
```

In `DesignerShell.razor`, wrap de canvas-sectie (zowel edit-mode als preview-mode) in:
```razor
<CascadingValue Value="@_designDataContext" IsFixed="false">
    @* bestaande canvas + preview content *@
</CascadingValue>
```

---

## Fase 2 — Design-time defaults in DesignerCanvasNode

### Doel
Wanneer een component op de canvas geen expliciete waarde heeft voor bepaalde parameters, injecteer realistische placeholders zodat het component er "gevuld" uitziet.

### Implementatie

In `DesignerCanvasNode.razor`, voeg een `[CascadingParameter]` toe:
```csharp
[CascadingParameter]
private DesignDataContext? DataContext { get; set; }
```

Pas de parameter-dictionary opbouw aan. Zoek de plek waar parameters voor `DynamicComponent` worden opgebouwd en voeg design-time defaults toe:

```csharp
private void InjectDesignTimeDefaults(Dictionary<string, object?> parameters)
{
    // Form-componenten: voeg placeholder toe als er geen Value/Placeholder is
    if (IsFormComponent(Node.ComponentType))
    {
        if (!parameters.ContainsKey("Placeholder") && parameters.TryGetValue("Label", out var labelObj))
        {
            var label = labelObj?.ToString() ?? Node.ComponentType;
            parameters["Placeholder"] = $"Bijv. {label.ToLowerInvariant()}...";
        }
    }
}

private static bool IsFormComponent(string componentType)
    => componentType is "AgtTextField" or "AgtNumericField" or "AgtDatePicker"
        or "AgtDropdown" or "AgtTextArea" or "AgtCheckboxField"
        or "RadzenTextBox" or "RadzenNumeric" or "RadzenDatePicker"
        or "RadzenDropDown" or "RadzenTextArea";
```

Roep `InjectDesignTimeDefaults` aan na het opbouwen van de parameter-dictionary, vóór het renderen van de `DynamicComponent`.

---

## Fase 3 — Vertaal slotnamen naar gebruikersvriendelijke labels

### Doel
Technische slotnamen ("ChildContent", "HeaderActions") zijn onbegrijpelijk voor niet-technische gebruikers. Vertaal ze.

### Implementatie

In `DesignerCanvasNode.razor` (of een aparte static helper), voeg een dictionary toe:

```csharp
private static readonly Dictionary<string, string> SlotDisplayNames = new(StringComparer.Ordinal)
{
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
    ["SummaryTemplate"] = "Samenvatting"
};

private static string GetSlotDisplayName(string slotName)
    => SlotDisplayNames.GetValueOrDefault(slotName, slotName);
```

Gebruik `GetSlotDisplayName()` overal waar slotnamen worden getoond:
- De `__hint` span in `RenderSlot()` (momenteel toont het de raw slotnaam)
- De breadcrumb
- De navigator/tree

### Verificatie fase 1–3
- Voeg een AgtTextField met Label="Klantnaam" toe → canvas toont placeholder "Bijv. klantnaam..."
- Voeg een AgtSidebarLayout toe → slots tonen "Inhoud", "Zijmenu", "Logo", "Acties"
- Breadcrumb toont "Inhoud" i.p.v. "ChildContent"
- Navigator-boom toont vertaalde slotnamen

---

## Fase 4 — Preview-mode met seeded data

### Doel
Preview-mode moet de app tonen zoals een eindgebruiker die ziet: formulieren met ingevulde waarden, tabellen met data, dropdowns met opties.

### Implementatie

**Optie A (aanbevolen): Verbeter `DesignRenderer` met data-injectie.**

Maak een nieuw component `src/Agterhuis.Ui.Designer/Components/DesignPreviewRenderer.razor`:

```razor
@using Agterhuis.Ui.Designer.Model
@using Agterhuis.Ui.Designer.Registry
@using Agterhuis.Ui.Designer.Services

<CascadingValue Value="@DataContext" IsFixed="false">
    <div class="agt-design-renderer agt-design-renderer--preview" data-agt-design-route="@Page?.Route">
        @if (Page is not null)
        {
            @for (var index = 0; index < Page.Nodes.Count; index++)
            {
                <DesignerNodeHost Node="Page.Nodes[index]"
                                  Path="@($"Page/Nodes[{index}]")"
                                  Registry="ResolvedRegistry" />
            }
        }
    </div>
</CascadingValue>

@code {
    [Parameter, EditorRequired]
    public DesignPage? Page { get; set; }

    [Parameter]
    public DesignerComponentRegistry? Registry { get; set; }

    [Parameter]
    public DesignDataContext? DataContext { get; set; }

    private DesignerComponentRegistry ResolvedRegistry
        => Registry ?? DesignerComponentRegistry.Instance;
}
```

**In `DesignerShell.razor`, vervang `DesignRenderer` in preview-mode:**

```razor
@if (_previewMode)
{
    <section class="designer-panel designer-panel--canvas designer-panel--preview" aria-label="Preview canvas">
        <div class="designer-canvas-frame @CanvasFrameClass" style="@CanvasFrameStyle">
            <div class="designer-canvas" data-agt-theme="@_canvasTheme">
                <DesignPreviewRenderer Page="@ActivePage"
                                       Registry="@Registry"
                                       DataContext="@_designDataContext" />
            </div>
        </div>
    </section>
}
```

**In `DesignerCanvasNode`, reageer op preview-context:**

Wanneer `DataContext` beschikbaar is EN de component in preview-mode rendert, injecteer rijkere data:
- Form-componenten: toon een sample-waarde als `Value` (niet alleen placeholder)
- DataGrid: injecteer `Data` met preview-rijen uit de eerste entity
- Dropdowns: injecteer `Data` met enum-waarden uit het datamodel

De exacte implementatie hangt af van hoe `DesignerCanvasNode` bepaalt of het in preview-mode is. Opties:
1. Een extra `[CascadingParameter] bool IsPreviewMode` vanuit `DesignerShell`
2. Detectie via de aanwezigheid van `DesignPreviewRenderer` als parent

### Verificatie fase 4
- Klik Preview → formuliervelden tonen sample-waarden uit het datamodel
- DataGrid in preview toont 5 rijen met dossiernummers, statussen, datums
- Wissel terug naar bewerken → velden tonen placeholders, niet data

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Services/DesignDataContext.cs` | 1 | NIEUW |
| `Components/DesignerShell.razor` | 1, 4 | CascadingValue wrapper, preview renderer |
| `Components/DesignerShell.razor.cs` | 1 | `_designDataContext` veld + initialisatie |
| `Components/DesignerCanvasNode.razor` | 2, 3 | CascadingParameter, InjectDesignTimeDefaults, SlotDisplayNames |
| `Components/DesignPreviewRenderer.razor` | 4 | NIEUW |

## Verificatie — integratietest

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. Handmatige test:

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Voeg AgtTextField met Label="Klantnaam" toe | Placeholder "Bijv. klantnaam..." |
| 2 | Voeg AgtSwitch met Label="Actief" toe | Switch zichtbaar met label, niet lege box |
| 3 | Bekijk slotnamen in navigator | "Inhoud" i.p.v. "ChildContent" |
| 4 | Bekijk breadcrumb na selectie | Vertaalde slotnamen |
| 5 | Klik Preview | Velden tonen sample-data |
| 6 | Klik Bewerken | Velden tonen placeholders |
