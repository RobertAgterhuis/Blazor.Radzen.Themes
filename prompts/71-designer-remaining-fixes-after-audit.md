# Prompt 71 — Designer: resterende fixes na audit van prompts 67–70

Audit van prompts 67–70 tegen de huidige codebase toont dat het overgrote deel correct is geïmplementeerd. Vier concrete problemen blijven over — twee zijn functionele bugs, twee zijn onvoltooide UX-verbeteringen.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Bug 1 — Kolommen in RadzenRow tonen niet naast elkaar op canvas (KRITIEK)

### Symptoom
Wanneer een `RadzenRow` met meerdere `RadzenColumn` children op de canvas staat, worden de kolommen **onder** elkaar getoond in plaats van naast elkaar. Het lijkt alsof er rijen worden toegevoegd in plaats van kolommen.

### Oorzaak
`DesignerCanvasNode.RenderSlot()` (regel ~378 van `DesignerCanvasNode.razor`) wraps alle child-nodes in een `<div class="designer-slot">`. Deze div is een gewoon block-level element — géén flex-container. De structuur op het DOM:

```html
<div class="rz-row" style="display:flex; flex-wrap:wrap;">   <!-- RadzenRow output -->
  <div class="designer-slot">                                 <!-- wrapper — blokkeert flex -->
    <div class="designer-slot__hint">ChildContent</div>
    <div class="designer-canvas-node">RadzenColumn 1</div>    <!-- stacked verticaal -->
    <div class="designer-canvas-node">RadzenColumn 2</div>    <!-- stacked verticaal -->
  </div>
</div>
```

De `designer-slot` div is het enige flex-item van de `rz-row`. De kolom-wrappers (`designer-canvas-node`) zijn children van `designer-slot`, niet van `rz-row`, dus de flex-layout van RadzenRow heeft geen effect op hen.

### Fix

**Stap 1: Maak `designer-slot` een flex-container wanneer het parent-component een RadzenRow is.**

In `DesignerCanvasNode.razor`, voeg een data-attribuut toe aan het slot-element dat het parent-componenttype aangeeft. Wijzig `RenderSlot()`:

```csharp
private void RenderSlot(RenderTreeBuilder builder, string slotName, IReadOnlyList<DesignNode> children)
{
    var sequence = 0;
    var firstDropzoneId = BuildDropzoneId(slotName, 0);

    builder.OpenElement(sequence++, "div");
    
    // Voeg een class toe als het parent-component een Row is
    var isRowSlot = string.Equals(Node.ComponentType, "RadzenRow", StringComparison.Ordinal);
    builder.AddAttribute(sequence++, "class", isRowSlot ? "designer-slot designer-slot--row" : "designer-slot");
    builder.AddAttribute(sequence++, "data-slot-name", slotName);
    // ... rest ongewijzigd
```

**Stap 2: Voeg CSS toe voor de flex row-slot.**

In `designer.css`:

```css
.designer-slot--row {
    display: flex;
    flex-wrap: wrap;
    gap: var(--agt-spacing-2);
}

/* Verberg de slot-hint in row-context — de rij-structuur is visueel duidelijk */
.designer-slot--row > .designer-slot__hint {
    display: none;
}
```

**Stap 3: Geef `designer-canvas-node` de juiste flex-basis wanneer het een RadzenColumn wraps.**

De `Size` parameter van RadzenColumn bepaalt de kolombreedte (1–12 grid). Zet dit als inline style op de wrapper:

In `DesignerCanvasNode.razor`, op de root div (regel 9), voeg een style-attribuut toe:

```razor
<div class="designer-canvas-node @(IsSelected ? "designer-canvas-node--selected" : string.Empty) @(IsHovered && !IsSelected ? "designer-canvas-node--hover" : string.Empty)"
     data-agt-design-node-id="@Node.Id"
     data-agt-design-component="@Node.ComponentType"
     style="@GetColumnFlexStyle()"
     @onmouseenter="OnMouseEnter"
     @onmouseleave="OnMouseLeave"
     @onmouseenter:stopPropagation="true"
     @onmouseleave:stopPropagation="true">
```

Voeg de helper toe in de `@code` sectie:

```csharp
private string? GetColumnFlexStyle()
{
    if (!string.Equals(Node.ComponentType, "RadzenColumn", StringComparison.Ordinal))
    {
        return null;
    }

    // Haal de Size parameter op (standaard 12 = volle breedte)
    var size = 12;
    if (Node.Parameters.TryGetValue("Size", out var sizeValue))
    {
        if (sizeValue is JsonValue jv && jv.TryGetValue<int>(out var s))
        {
            size = Math.Clamp(s, 1, 12);
        }
        else if (sizeValue is int i)
        {
            size = Math.Clamp(i, 1, 12);
        }
    }

    // Bereken percentage, minus gap-ruimte
    var pct = Math.Round(size / 12.0 * 100, 2);
    return $"flex: 0 0 calc({pct}% - var(--agt-spacing-2)); min-width: 0;";
}
```

**Stap 4: Zorg dat dropzones binnen een row-slot ook horizontaal werken.**

De dropzones tussen kolommen moeten als smalle verticale strepen verschijnen, niet als horizontale balken. Voeg CSS toe:

```css
.designer-slot--row > .designer-dropzone,
.designer-slot--row > [class*="designer-root-dropzone"] {
    flex: 0 0 4px;
    min-height: 3rem;
    align-self: stretch;
}
```

### Verificatie
- Voeg een RadzenRow toe aan de canvas.
- Voeg twee RadzenColumn (Size 6) toe → ze verschijnen **naast** elkaar, elk 50% breed.
- Voeg een derde kolom (Size 4) toe → layout past zich aan.
- Sleep een kolom naar een andere positie binnen de rij → herordening werkt.
- Selecteer een kolom → resize handles werken om Size te wijzigen → breedte verandert live.

---

## Bug 2 — AgtSwitch label-klik togglet niet (KRITIEK)

### Symptoom
Klikken op de labeltekst van "Geavanceerd" of "Meer opties" in het property panel doet niets. Alleen klikken op het kleine switch-thumb werkt.

### Oorzaak
Prompt 70 heeft `for="@ResolvedName"` correct verwijderd uit het `<label>` element. De bedoeling was dat implicit label association (input genest in label) het overneemt. Maar `RadzenSwitch` rendert zijn `<input type="checkbox">` binnen een wrapper `<div class="rz-switch">`, en sommige Radzen-versies gebruiken JavaScript-click-handling op de wrapper-div in plaats van op het native input-element. Daardoor triggered een browser-label-click niet altijd de Blazor `ValueChanged` callback.

### Fix

Voeg een expliciete `@onclick` handler toe op het wrapper-element. In `src/Agterhuis.Ui/Components/Forms/AgtSwitch.razor`:

```razor
<div class="agt-field-wrapper @CssClass" role="group">
    <label class="agt-switch" @onclick="OnLabelClicked" @onclick:preventDefault="true">
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

Voeg de handler toe in de `@code` sectie:

```csharp
private async Task OnLabelClicked()
{
    if (Disabled) return;
    await ValueChanged.InvokeAsync(!Value);
}
```

**Belangrijk — double-toggle voorkomen:** de `@onclick:preventDefault="true"` op het `<label>` voorkomt dat de browser NAAST de Blazor-handler ook nog het native label-associatie-gedrag triggert. Test:

1. Klik op labeltekst → exact 1 toggle
2. Klik op switch-thumb → exact 1 toggle (RadzenSwitch handelt dit zelf af via de `@onclick:preventDefault` op de label die het native checkbox-toggle blokkeert; de RadzenSwitch `ValueChanged` vuurt via zijn eigen handler)

**Als de switch-thumb stopt met werken** na deze wijziging (omdat `preventDefault` op het label ook het RadzenSwitch event blokkeert), gebruik dan een meer gerichte aanpak — zet de `@onclick` op de `<span class="agt-switch__label">` in plaats van op het `<label>`:

```razor
<div class="agt-field-wrapper @CssClass" role="group">
    <label class="agt-switch">
        <span class="agt-switch__label" @onclick="OnLabelClicked" @onclick:stopPropagation="true" style="cursor: pointer;">@ResolvedLabel</span>
        <RadzenSwitch Name="@ResolvedName"
                      Value="@Value"
                      ValueChanged="@ValueChanged"
                      Disabled="@Disabled"
                      InputAttributes="@ResolvedInputAttributes" />
        <span class="agt-switch__state" @onclick="OnLabelClicked" @onclick:stopPropagation="true" style="cursor: pointer;" aria-live="polite">@(Value ? OnText : OffText)</span>
    </label>
</div>
```

Dit laat de RadzenSwitch-thumb ongestoord werken, en voegt toggle-gedrag toe aan de tekst-spans.

### Verificatie
- Klik op "Geavanceerd" tekst → switch togglet, extra parameters verschijnen/verdwijnen.
- Klik op "Meer opties" tekst → switch togglet.
- Klik op switch-thumb → werkt nog steeds, exact 1 toggle, geen double-toggle.
- Klik op "Aan"/"Uit" tekst → switch togglet.
- Test met `Disabled="true"` → geen van de clicks doet iets.
- Test in Data panel (kolom-toggles), issues filters — alle AgtSwitch-instanties.

---

## Fix 3 — Divider dubbel-klik reset (ontbrekend uit prompt 69, fase 8)

### Symptoom
Dubbelklikken op een panel-divider reset de panelbreedte niet naar de standaardwaarde.

### Fix

In `src/Agterhuis.Ui.Designer/wwwroot/designer-resize-interop.js`, voeg een `dblclick` event toe via event delegation. Voeg dit toe NA de bestaande `mousedown` event delegation, binnen dezelfde `setupResizablePanels` functie:

```javascript
designerLayout.addEventListener('dblclick', (e) => {
    const divider = e.target.closest('.designer-divider[data-divider]');
    if (!divider) return;

    const dividerType = divider.getAttribute('data-divider');
    const cfg = CONFIG[dividerType];
    if (!cfg) return;

    const cssVar = '--designer-' + cfg.prop;
    designerLayout.style.setProperty(cssVar, cfg.def + 'px');

    // Sla de reset-waarde op
    const sizes = loadSizes();
    const key = cfg.prop.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
    sizes[key] = cfg.def;
    saveSizes(sizes);
});
```

Dit gebruikt de bestaande `CONFIG` dict die al `def` waarden bevat (220, 320, 250) en de bestaande `loadSizes`/`saveSizes` helpers.

### Verificatie
- Sleep palette-divider naar 400px breed → dubbelklik op divider → palette springt terug naar 220px.
- Sleep property-divider → dubbelklik → terug naar 320px.
- Ververs pagina → de gereset-waarde is bewaard in localStorage.

---

## Fix 4 — Palette technische namen niet verborgen (onvolledig uit prompt 69, fase 9)

### Symptoom
De technische componentnaam (bijv. "RadzenTextBox") onder palette-items is altijd zichtbaar. Het zou standaard verborgen moeten zijn en alleen bij hover getoond worden.

### Oorzaak
De CSS-regel `.designer-palette-item:hover .designer-palette-item__meta { display: inline; }` bestaat, maar de base-regel mist `display: none`. Daardoor is de meta altijd zichtbaar.

### Fix

In `designer.css`, voeg `display: none` toe aan de bestaande `.designer-palette-item__meta` regel:

```css
.designer-palette-item__meta {
    color: var(--agt-text-muted);
    display: none;               /* <-- TOEVOEGEN */
    font-size: var(--agt-font-size-xs);
}

.designer-palette-item:hover .designer-palette-item__meta {
    display: inline;
}
```

### Verificatie
- Open het palette → technische namen zijn verborgen.
- Hover over een palette-item → technische naam verschijnt.
- Sleep een item → technische naam is niet zichtbaar tijdens drag.

---

## Samenvatting wijzigingen per bestand

| Bestand | Fix |
|---------|-----|
| `DesignerCanvasNode.razor` | #1 (`designer-slot--row` class, `GetColumnFlexStyle()` method) |
| `designer.css` | #1 (`.designer-slot--row` flex layout, dropzone in row-slot), #4 (palette meta `display:none`) |
| `AgtSwitch.razor` | #2 (`@onclick` handler op label/spans) |
| `designer-resize-interop.js` | #3 (`dblclick` event delegation) |

## Volgorde van uitvoering

1. **Bug 1** (kolom-layout) — KRITIEK, visueel het meest impactvol
2. **Bug 2** (switch label klik) — KRITIEK, blokkeert gebruikers
3. **Fix 3** (divider reset) — klein, onafhankelijk
4. **Fix 4** (palette meta) — CSS-only, onafhankelijk

Elke fix is een eigen commit met beschrijvende message.

## Verificatie — integratietest na alle fixes

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **Handmatige checklist:**

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Voeg RadzenRow + 2x RadzenColumn (Size 6) toe | Kolommen naast elkaar, elk ~50% breed |
| 2 | Wijzig een kolom Size van 6 naar 4 | Kolom wordt smaller (33%) |
| 3 | Klik op "Geavanceerd" labeltekst | Switch togglet |
| 4 | Klik op switch-thumb zelf | Switch togglet (geen double-toggle) |
| 5 | Dubbelklik palette-divider | Palette reset naar 220px breed |
| 6 | Hover palette-item | Technische naam verschijnt |
| 7 | Geen hover | Technische naam verborgen |
