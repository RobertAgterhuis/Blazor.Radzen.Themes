# Prompt 79b — Events-tab: event handlers koppelen en genereren

De Events-tab in de PropertyPanel toont momenteel "Logica in geëxporteerde code (v1-scope)." voor elke `EventCallback`. Gebruikers kunnen geen interactiviteit toevoegen. Radzen Studio laat je klikken op + naast een event, een method-naam invoeren, en genereert automatisch een handler.

Dit prompt implementeert een werkende Events-tab die event handlers registreert in het design-model en meegenereert in de export.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Uitbreiding design-model met event bindings

### Probleem
`DesignParameterValue` kan een `Literal` of `Expression` opslaan. Er is geen concept voor event handler namen. EventCallback parameters worden in de PropertyPanel gefilterd:

```csharp
.Where(static parameter => !parameter.IsEventCallback) // regel 371 PropertyPanel.razor
```

### Fix

**Stap 1: Voeg EventHandlerName toe aan DesignParameterValue.**

In `DesignParameterValue.cs`:

```csharp
public sealed class DesignParameterValue
{
    public JsonNode? Literal { get; set; }
    public string? Expression { get; set; }

    // NIEUW:
    /// <summary>
    /// Name of the event handler method to generate in exported code.
    /// Only used for EventCallback parameters.
    /// Example: "OnSaveClicked" or "OnStatusChanged"
    /// </summary>
    public string? EventHandlerName { get; set; }
}
```

**Stap 2: Update serialisatie.**

In `DesignDocumentSerializer` en `DesignDocumentDeserializer`, voeg `eventHandlerName` toe aan de JSON-structuur:

```json
{
  "Click": { "eventHandlerName": "OnSaveClicked" }
}
```

---

## Fase 2 — Events-tab in PropertyPanel

### Doel
De Interactie-tab toont nu EventCallback parameters met een + knop om een handler-naam in te voeren.

### Implementatie

**Stap 1: Verwijder de EventCallback filter.**

In `PropertyPanel.razor`, in `GroupedParameters` (regel ~371), verwijder:

```csharp
// VERWIJDER DEZE REGEL:
.Where(static parameter => !parameter.IsEventCallback)
```

**Stap 2: Vervang de EventCallback editor.**

In `PropertyPanel.razor`, vervang het `EditorKind.EventCallback` case (regel ~276):

```razor
case EditorKind.EventCallback:
    @{
        var handlerName = GetEventHandlerName(parameter);
        @if (string.IsNullOrWhiteSpace(handlerName))
        {
            <div class="designer-event-handler">
                <RadzenButton Text="Handler toevoegen"
                              Icon="add"
                              ButtonStyle="ButtonStyle.Base"
                              Variant="Variant.Text"
                              Click="@(() => AddEventHandler(parameter))" />
            </div>
        }
        else
        {
            <div class="designer-event-handler">
                <AgtTextField Label="Methode-naam"
                              AriaLabel="Event handler naam"
                              Value="@handlerName"
                              ValueChanged="@(v => SetEventHandler(parameter, v))" />
                <RadzenButton Icon="close"
                              ButtonStyle="ButtonStyle.Base"
                              Variant="Variant.Text"
                              title="Handler verwijderen"
                              Click="@(() => RemoveEventHandler(parameter))" />
            </div>
        }
    }
    break;
```

**Stap 3: Voeg helpers toe.**

```csharp
private string? GetEventHandlerName(ComponentParameterDescriptor parameter)
{
    if (SelectedNode is null) return null;
    if (!SelectedNode.Parameters.TryGetValue(parameter.Name, out var value)) return null;
    return value?.EventHandlerName;
}

private Task AddEventHandler(ComponentParameterDescriptor parameter)
{
    var defaultName = $"On{parameter.Name.Replace("Changed", string.Empty, StringComparison.Ordinal)}";
    // Make it unique by appending the component display name
    if (SelectedDescriptor is not null)
    {
        var componentShort = SelectedDescriptor.DisplayName.Replace("Agt", string.Empty, StringComparison.Ordinal)
            .Replace("Radzen", string.Empty, StringComparison.Ordinal);
        defaultName = $"On{componentShort}{parameter.Name.Replace("Changed", string.Empty, StringComparison.Ordinal)}";
    }

    return SetEventHandler(parameter, defaultName);
}

private Task SetEventHandler(ComponentParameterDescriptor parameter, string? name)
{
    if (string.IsNullOrWhiteSpace(name))
        return RemoveEventHandler(parameter);

    var value = new DesignParameterValue { EventHandlerName = name.Trim() };
    return SetNodeParameter.InvokeAsync((parameter, value));
}

private Task RemoveEventHandler(ComponentParameterDescriptor parameter)
    => SetNodeParameter.InvokeAsync((parameter, null));
```

**Stap 4: Classificeer EventCallback als Interactie.**

In `PropertyPanel.razor`, pas de `Classify` methode aan zodat EventCallbacks onder `InspectorSection.Interaction` vallen (ze worden al gefilterd door `IsInteractionParam` — verwijder alleen de pre-filter uit `GroupedParameters`).

Het `EditorKind`-detectie in `GetEditorKind` handelt EventCallbacks al correct af:
```csharp
if (parameter.IsEventCallback)
    return EditorKind.EventCallback;
```

---

## Fase 3 — Event handlers in export

### Doel
De `ProjectExporter` moet event handlers meegenereren in de geëxporteerde code.

### Implementatie

**Stap 1: Genereer event-attributen op componenten.**

In de template-generatie van `ProjectExporter`, wanneer een node een parameter heeft met `EventHandlerName`:

```csharp
// Bij het genereren van component-markup:
foreach (var (paramName, paramValue) in node.Parameters)
{
    if (!string.IsNullOrWhiteSpace(paramValue.EventHandlerName))
    {
        // EventCallback parameter
        lines.Add($"    {paramName}=\"{paramValue.EventHandlerName}\"");
    }
}
```

**Stap 2: Genereer methode-stubs in de code-behind.**

Voeg een helper toe aan `ProjectExporter` die method stubs genereert:

```csharp
private static IReadOnlyList<string> CollectEventHandlers(DesignDocument document)
{
    var handlers = new Dictionary<string, string>(StringComparer.Ordinal);

    foreach (var page in document.Pages)
    {
        CollectFromNodes(page.Nodes, handlers);
    }

    return handlers.Select(kv =>
    {
        var (name, eventType) = (kv.Key, kv.Value);
        return eventType switch
        {
            "MouseEventArgs" => $"    private void {name}(MouseEventArgs args)\n    {{\n        // TODO: implementeer logica\n    }}",
            _ => $"    private void {name}()\n    {{\n        // TODO: implementeer logica\n    }}"
        };
    }).ToList();
}

private static void CollectFromNodes(IEnumerable<DesignNode> nodes, Dictionary<string, string> handlers)
{
    foreach (var node in nodes)
    {
        foreach (var (_, value) in node.Parameters)
        {
            if (!string.IsNullOrWhiteSpace(value?.EventHandlerName) && !handlers.ContainsKey(value.EventHandlerName))
            {
                handlers[value.EventHandlerName] = "void";
            }
        }

        foreach (var slot in node.Children.Values)
        {
            CollectFromNodes(slot, handlers);
        }
    }
}
```

Voeg de gegenereerde methods toe aan de pagina-code in het geëxporteerde project.

---

## Fase 4 — CSS voor event handler editor

In `designer.css`:

```css
.designer-event-handler {
    align-items: center;
    display: flex;
    gap: var(--agt-spacing-2);
}

.designer-event-handler .agt-text-field {
    flex: 1;
}
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Model/DesignParameterValue.cs` | 1 | `EventHandlerName` property |
| `Serialization/DesignDocumentSerializer.cs` | 1 | eventHandlerName serialisatie |
| `Serialization/DesignDocumentDeserializer.cs` | 1 | eventHandlerName deserialisatie |
| `Components/PropertyPanel.razor` | 2 | EventCallback editor met + knop en naamveld |
| `Export/ProjectExporter.cs` | 3 | Event-attributen + methode-stubs in export |
| `wwwroot/css/designer.css` | 4 | `.designer-event-handler` styling |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Selecteer een AgtButton, open Interactie-tab | "Click" event zichtbaar met "Handler toevoegen" knop |
| 2 | Klik "Handler toevoegen" | Methode-naam veld verschijnt met default "OnButtonClick" |
| 3 | Wijzig naam naar "OnSaveClicked" | Naam wordt opgeslagen |
| 4 | Klik "×" knop | Handler verwijderd, "Handler toevoegen" weer zichtbaar |
| 5 | Sla document op, herlaad | Event handler behouden |
| 6 | Exporteer project | Component heeft `Click="OnSaveClicked"`, code-behind heeft method stub |
| 7 | `dotnet build` geëxporteerd project | Compileert zonder fouten |
