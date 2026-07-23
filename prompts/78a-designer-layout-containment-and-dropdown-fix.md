# Prompt 78a — Layout containment + RadzenDropDown popup fix

Twee onafhankelijke bugs die beide de designer onbruikbaar maken voor dagelijks gebruik.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Bug 1 — RadzenDropDown popup sluit niet na selectie

### Symptoom
Alle `RadzenDropDown` componenten in de designer (thema-kiezer, opgeslagen documenten, template-kiezer) openen hun popup correct, maar na het selecteren van een optie blijft de popup open. De waarde verandert WEL, maar de popup verdwijnt niet.

### Oorzaak
De dropdowns gebruikten `Change` (een Radzen-specifiek `EventCallback<object>`) in plaats van `@bind-Value` of `ValueChanged`. Zonder `ValueChanged` beheert Radzen zijn interne popup-state niet — het weet niet dat de waarde is gewijzigd en sluit de popup niet.

Een eerdere fix schakelde over naar `ValueChanged`, maar de callback-handlers (`OnCanvasThemeChanged`, `OnSavedSelectionChanged`) riepen `CloseAllMenus()` en `await InvokeAsync(StateHasChanged)` aan. Deze expliciete re-render tijdens de `ValueChanged` callback verstoort Radzen's interne popup-close JS-logica.

### Fix

**Stap 1: Gebruik `@bind-Value` op alle `RadzenDropDown` instanties.**

`@bind-Value` laat Radzen de volledige popup-lifecycle beheren: waarde zetten → popup sluiten via JS → `Change` event vuren. De `Change` event wordt alleen gebruikt voor side-effects (parent notificeren, document laden), NIET voor waarde-management.

In `DesignerShell.razor`, wijzig alle 4 RadzenDropDown instanties:

**Thema-kiezer (toolbar, regel ~143):**
```razor
<RadzenDropDown TValue="string"
                Data="CanvasThemeOptions"
                @bind-Value="_canvasTheme"
                Change="@(async (object _) => await CanvasThemeChanged.InvokeAsync(_canvasTheme))"
                Style="min-width: 150px;"
                Placeholder="Thema" />
```

**Opgeslagen documenten (Bestand menu, regel ~69):**
```razor
<RadzenDropDown id="designer-saved-docs"
                TValue="string"
                Data="SavedDocumentNames"
                @bind-Value="_selectedSavedName"
                Placeholder="Selecteer"
                Change="@(async (object _) => await OnSavedSelectionChanged(_selectedSavedName))" />
```

**Hardcoded color token (issues panel, regel ~357):**
```razor
<RadzenDropDown TValue="string"
                Data="HardcodedColorTokenOptions"
                TextProperty="Label"
                ValueProperty="Value"
                @bind-Value="_hardcodedColorFixToken" />
```

**Template-kiezer (nieuw document dialoog, regel ~693):**
```razor
<RadzenDropDown id="designer-template-kind"
                TValue="DesignDocumentTemplateKind"
                Data="TemplateOptions"
                @bind-Value="_selectedTemplateKind"
                TextProperty="DisplayName"
                ValueProperty="Kind" />
```

**Stap 2: Vereenvoudig `OnCanvasThemeChanged` in `DesignerShell.razor.cs`.**

Verwijder `CloseAllMenus()` en `await InvokeAsync(StateHasChanged)` — Blazor rendert automatisch na een `EventCallback`. De methode wordt nog gebruikt door `ToggleDarkLight()` en `PropertyPanel.SetCanvasTheme`, die NIET via de Radzen dropdown gaan:

```csharp
private async Task OnCanvasThemeChanged(string value)
{
    _canvasTheme = string.IsNullOrWhiteSpace(value) ? "plum-dark" : value;
    await CanvasThemeChanged.InvokeAsync(_canvasTheme);
}
```

**Stap 3: Verwijder de ongebruikte `OnCanvasThemeChanged(object)` overload.**

Deze wordt nergens meer aangeroepen:
```csharp
// VERWIJDER DEZE METHODE:
// private Task OnCanvasThemeChanged(object value) => OnCanvasThemeChanged(value?.ToString() ?? "plum-dark");
```

**Stap 4: Verwijder de ongebruikte `OnTemplateChanged(object)` methode.**

```csharp
// VERWIJDER DEZE METHODE:
// private Task OnTemplateChanged(object value) { ... }
```

**Stap 5: Wijzig `OnSavedSelectionChanged` signatuur naar `string?`.**

```csharp
private async Task OnSavedSelectionChanged(string? value)
{
    var selected = value;
    if (string.IsNullOrWhiteSpace(selected))
    {
        return;
    }

    _selectedSavedName = selected;
    CloseAllMenus(); // Sluit het Bestand-menu NA waarde-toekenning
    var envelope = await Store.LoadAsync(selected);
    // ... rest ongewijzigd
}
```

### Verificatie
- Open thema-dropdown → selecteer "ocean-dark" → popup sluit, canvas schakelt naar ocean-thema.
- Open thema-dropdown → selecteer "plum-light" → popup sluit, canvas schakelt naar plum-light.
- Open Bestand → selecteer opgeslagen document → popup sluit, document laadt.
- Open template-kiezer in Nieuw document dialoog → selecteer "Dashboard" → popup sluit.

---

## Bug 2 — AgtSidebarLayout rendert buiten de canvas

### Symptoom
Wanneer een `AgtSidebarLayout` op de canvas wordt geplaatst, rendert de sidebar-sectie buiten de canvas-grenzen — op viewport-niveau in plaats van binnen de component-node.

### Oorzaak
`RadzenSidebar` (uit het Radzen.Blazor NuGet-pakket) gebruikt `position: fixed` in zijn CSS. `position: fixed` positioneert ten opzichte van de viewport, niet ten opzichte van het parent-element. Het `overflow: hidden` op de canvas-container heeft geen effect op fixed-positioned children.

### Fix

**Stap 1: Voeg CSS containment toe voor layout-componenten.**

In `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css`, voeg toe:

```css
/* ── Layout containment ───────────────────────────────────────────── */
/* Voorkom dat componenten met position:fixed uit de canvas breken.   */

.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] {
    contain: layout style paint;
    min-height: 300px;
    overflow: hidden;
    position: relative;
}

.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] .agt-sidebar-layout {
    height: 100%;
    min-height: inherit;
}

.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] .agt-sidebar-layout__main {
    min-height: 200px;
}

/* Override RadzenSidebar fixed positioning binnen designer-canvas */
.designer-canvas .rz-sidebar {
    height: auto !important;
    min-height: 200px;
    position: relative !important;
    width: 100% !important;
    z-index: auto !important;
}

/* Zelfde aanpak voor dialogen en panels die fixed/absolute gebruiken */
.designer-canvas .rz-dialog-wrapper {
    position: absolute !important;
}

.designer-canvas .rz-overlay-panel {
    position: absolute !important;
}
```

**Stap 2: Controleer dat `data-agt-design-component` aanwezig is op de canvas-node.**

In `DesignerCanvasNode.razor`, controleer dat het root-element dit attribuut heeft:

```razor
<div class="designer-canvas-node ..."
     data-agt-design-node-id="@Node.Id"
     data-agt-design-component="@Node.ComponentType"
     ...>
```

Als dit attribuut al bestaat (wat uit de codebase-analyse blijkt), hoeft niets gewijzigd te worden. Verifieer dit door te zoeken naar `data-agt-design-component` in het bestand.

**Stap 3: Voeg `designer-canvas` class toe op de canvas-container (als ontbrekend).**

In `DesignerShell.razor`, controleer dat de canvas-div de class `designer-canvas` heeft:

```razor
<div class="designer-canvas" data-agt-theme="@_canvasTheme" @ref="_canvasRef">
```

### Verificatie
- Voeg AgtSidebarLayout toe aan canvas → sidebar rendert BINNEN de canvas-node.
- Header, sidebar-slot, en content-slot zijn alle drie zichtbaar binnen de component-grenzen.
- Selecteer de layout → blauwe outline omvat het hele component.
- Resize het browservenster → layout past zich aan binnen de canvas, niet op viewport-niveau.
- Voeg componenten toe in de sidebar-slot → ze verschijnen BINNEN de sidebar-sectie.
- Voeg componenten toe in de content-slot → ze verschijnen RECHTS van de sidebar.

---

## Samenvatting wijzigingen per bestand

| Bestand | Bug | Wijziging |
|---------|-----|-----------|
| `Components/DesignerShell.razor` | #1 | 4× `RadzenDropDown` van `Change`/`ValueChanged` → `@bind-Value` |
| `Components/DesignerShell.razor.cs` | #1 | `OnCanvasThemeChanged` vereenvoudigd, ongebruikte overloads verwijderd |
| `wwwroot/css/designer.css` | #2 | CSS containment voor layout-componenten, RadzenSidebar override |
| `Components/DesignerCanvasNode.razor` | #2 | Verifieer `data-agt-design-component` attribuut (waarschijnlijk al aanwezig) |

## Verificatie — integratietest

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. Handmatige test:

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Open thema-dropdown, selecteer ocean-dark | Popup sluit, canvas wordt ocean |
| 2 | Open thema-dropdown opnieuw, selecteer plum-light | Popup sluit, canvas wordt plum-light |
| 3 | Klik dark/light toggle | Thema wisselt, geen stuck popup |
| 4 | Voeg AgtSidebarLayout toe | Layout rendert binnen canvas |
| 5 | Voeg content toe in sidebar-slot | Content verschijnt in sidebar-sectie |
| 6 | Voeg content toe in content-slot | Content verschijnt rechts van sidebar |
