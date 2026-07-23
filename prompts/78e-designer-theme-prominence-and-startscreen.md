# Prompt 78e — Thema-switching prominentie en startscherm upgrade

Afhankelijk van: prompt 78a (dropdown fix — thema-dropdown moet correct werken).

De thema-kiezer is een klein dropdown-element op de toolbar dat gebruikers niet opvalt. Het startscherm toont templates in kleine kaartjes zonder visuele groepering. Deze prompt maakt thema-switching prominent en het startscherm aantrekkelijker.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Dark/light toggle naast thema-dropdown

### Doel
Een visuele toggle (zon/maan icoon) direct naast de thema-dropdown zodat dark/light switching een enkele klik is in plaats van een dropdown-selectie.

### Implementatie

**Stap 1: Voeg een toggle-knop toe aan de toolbar.**

In `DesignerShell.razor`, direct na de thema-dropdown:

```razor
<RadzenButton Icon="@(_isDarkMode ? "light_mode" : "dark_mode")"
              ButtonStyle="ButtonStyle.Light"
              Size="ButtonSize.Small"
              title="@(_isDarkMode ? "Licht thema" : "Donker thema")"
              Click="@ToggleDarkLight" />
```

**Stap 2: Voeg `_isDarkMode` computed property toe in `DesignerShell.razor.cs`.**

```csharp
private bool _isDarkMode => _canvasTheme?.EndsWith("-dark", StringComparison.OrdinalIgnoreCase) ?? true;
```

**Stap 3: Implementeer of verifieer `ToggleDarkLight`.**

Als deze methode al bestaat, controleer dat ze correct werkt:

```csharp
private async Task ToggleDarkLight()
{
    if (string.IsNullOrWhiteSpace(_canvasTheme)) return;

    _canvasTheme = _isDarkMode
        ? _canvasTheme.Replace("-dark", "-light", StringComparison.OrdinalIgnoreCase)
        : _canvasTheme.Replace("-light", "-dark", StringComparison.OrdinalIgnoreCase);

    await CanvasThemeChanged.InvokeAsync(_canvasTheme);
}
```

### Verificatie
- Toggle-knop toont maanicoon in dark mode, zonicoon in light mode
- Klik wisselt alleen light↔dark, behoudt theme-family (ocean-dark → ocean-light)
- Dropdown en toggle zijn visueel samenhangend (naast elkaar, zelfde hoogte)

---

## Fase 2 — Thema-dropdown met visuele preview

### Doel
De thema-dropdown toont momenteel platte tekst ("plum-dark", "ocean-light"). Voeg een kleurbol of swatch toe zodat de gebruiker een visueel idee heeft van het thema.

### Implementatie

**Stap 1: Vervang de platte dropdown door een dropdown met template.**

In `DesignerShell.razor`, vervang de thema-dropdown:

```razor
<RadzenDropDown TValue="string"
                Data="CanvasThemeGroupOptions"
                @bind-Value="_canvasThemeFamily"
                Change="@(async (object _) => await OnThemeFamilyChanged(_canvasThemeFamily))"
                Style="min-width: 150px;"
                Placeholder="Thema">
    <Template Context="item">
        <div style="display: flex; align-items: center; gap: 8px;">
            <span class="designer-theme-swatch"
                  style="background: @(GetThemeSwatchColor(item));"></span>
            <span>@FormatThemeFamilyName(item)</span>
        </div>
    </Template>
</RadzenDropDown>
```

**Stap 2: Groepeer thema's op family (niet op variant).**

In `DesignerShell.razor.cs`:

```csharp
/// <summary>
/// Theme families (niet per variant). Dark/light wordt via de toggle geregeld.
/// </summary>
private string[] CanvasThemeGroupOptions
    => AgtTheme.All.Select(t => t.FamilyId).OrderBy(id => id).ToArray();

private string _canvasThemeFamily = "plum";

private async Task OnThemeFamilyChanged(string family)
{
    _canvasThemeFamily = family ?? "plum";
    _canvasTheme = _isDarkMode
        ? $"{_canvasThemeFamily}-dark"
        : $"{_canvasThemeFamily}-light";
    await CanvasThemeChanged.InvokeAsync(_canvasTheme);
}
```

Voeg ook een initialisatie toe in `OnParametersSet` of bij document-load:

```csharp
_canvasThemeFamily = _canvasTheme?.Split('-')[0] ?? "plum";
```

**Stap 3: Voeg swatch-kleuren toe.**

```csharp
private static string GetThemeSwatchColor(string familyId)
    => familyId switch
    {
        "plum" => "#8b5cf6",
        "ocean" => "#0ea5e9",
        "dagobah" => "#22c55e",
        "dathomir" => "#ef4444",
        "hoth" => "#94a3b8",
        "tatooine" => "#f59e0b",
        "imperial" => "#6366f1",
        "azure" => "#3b82f6",
        "ms365" => "#0078d4",
        "volt" => "#eab308",
        "autotaalglas" => "#10b981",
        "autotaalglascontrast" => "#14b8a6",
        "autotaalglasportal" => "#059669",
        "autotaalglasmono" => "#64748b",
        _ => "#6b7280"
    };

private static string FormatThemeFamilyName(string familyId)
    => familyId switch
    {
        "plum" => "Plum",
        "ocean" => "Ocean",
        "dagobah" => "Dagobah",
        "dathomir" => "Dathomir",
        "hoth" => "Hoth",
        "tatooine" => "Tatooine",
        "imperial" => "Imperial",
        "azure" => "Azure",
        "ms365" => "Microsoft 365",
        "volt" => "Volt",
        "autotaalglas" => "Autotaalglas",
        "autotaalglascontrast" => "ATG Contrast",
        "autotaalglasportal" => "ATG Portal",
        "autotaalglasmono" => "ATG Mono",
        _ => familyId
    };
```

**Stap 4: CSS voor swatch.**

In `designer.css`:

```css
.designer-theme-swatch {
    border-radius: 50%;
    display: inline-block;
    flex-shrink: 0;
    height: 14px;
    width: 14px;
}
```

### Verificatie
- Dropdown toont kleurbollen naast themanamen
- Selecteer "Ocean" → canvas wordt ocean-dark (of ocean-light als light mode actief was)
- Dark/light toggle werkt onafhankelijk van family-selectie
- Alle 14 families tonen correcte swatch-kleur

---

## Fase 3 — Startscherm visuele upgrade

### Doel
Het startscherm toont templates in een vlakke grid. Voeg categorisering, grotere preview-kaarten, en een visuele hiërarchie toe.

### Implementatie

**Stap 1: Groepeer templates per categorie.**

In `DesignerStartScreen.razor`, groepeer de templates:

```razor
<section class="designer-start__templates">
    <h3 class="designer-start__category-title">Basis</h3>
    <div class="designer-start__template-grid">
        @foreach (var template in BasicTemplates)
        {
            @* bestaande template-card markup *@
        }
    </div>

    <h3 class="designer-start__category-title">App-patronen</h3>
    <div class="designer-start__template-grid">
        @foreach (var template in AppTemplates)
        {
            @* bestaande template-card markup *@
        }
    </div>
</section>
```

In `DesignerStartScreen.razor.cs` (of `@code` blok):

```csharp
private static readonly DesignDocumentTemplateKind[] BasicKinds =
    [DesignDocumentTemplateKind.Blank, DesignDocumentTemplateKind.FormPage,
     DesignDocumentTemplateKind.ListCrud, DesignDocumentTemplateKind.Dashboard];

private static readonly DesignDocumentTemplateKind[] AppKinds =
    [DesignDocumentTemplateKind.MasterDetail, DesignDocumentTemplateKind.Wizard,
     DesignDocumentTemplateKind.SidebarApp, DesignDocumentTemplateKind.SettingsPage,
     DesignDocumentTemplateKind.TableWithFilters];

private IEnumerable<TemplateOption> BasicTemplates
    => TemplateOptions.Where(t => BasicKinds.Contains(t.Kind));

private IEnumerable<TemplateOption> AppTemplates
    => TemplateOptions.Where(t => AppKinds.Contains(t.Kind));
```

> **Let op**: Als prompt 78c (nieuwe template-soorten) nog niet is uitgevoerd, gebruik dan alleen de bestaande 6 template-soorten en pas de categorisering aan.

**Stap 2: Vergroot de template-preview kaarten.**

In `designer.css`, pas de template-grid aan:

```css
.designer-start__template-grid {
    display: grid;
    gap: var(--agt-spacing-4);
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
}

.designer-start__template-card {
    aspect-ratio: 4 / 3;
    border: 1px solid var(--agt-border-subtle);
    border-radius: var(--agt-border-radius-md);
    cursor: pointer;
    overflow: hidden;
    transition: border-color 200ms, box-shadow 200ms;
}

.designer-start__template-card:hover {
    border-color: var(--agt-color-primary-500);
    box-shadow: 0 2px 12px var(--agt-alpha-primary-15);
}

.designer-start__template-card__preview {
    height: 70%;
    overflow: hidden;
    pointer-events: none;
    transform: scale(0.5);
    transform-origin: top left;
    width: 200%;
}

.designer-start__template-card__label {
    align-items: center;
    display: flex;
    gap: var(--agt-spacing-2);
    height: 30%;
    padding: var(--agt-spacing-2) var(--agt-spacing-3);
}

.designer-start__category-title {
    color: var(--agt-text-muted);
    font-size: 0.75rem;
    font-weight: 600;
    letter-spacing: 0.05em;
    margin-bottom: var(--agt-spacing-2);
    margin-top: var(--agt-spacing-6);
    text-transform: uppercase;
}
```

**Stap 3: Voeg beschrijvingen toe aan template-kaarten.**

Onder de titel in elke kaart, toon een korte beschrijving:

```csharp
private static string GetTemplateDescription(DesignDocumentTemplateKind kind) => kind switch
{
    DesignDocumentTemplateKind.Blank => "Begin met een leeg canvas.",
    DesignDocumentTemplateKind.FormPage => "Invoerformulier met velden en acties.",
    DesignDocumentTemplateKind.ListCrud => "Overzichtstabel met zoek- en filteropties.",
    DesignDocumentTemplateKind.MasterDetail => "Lijst links, detail rechts.",
    DesignDocumentTemplateKind.Wizard => "Stap-voor-stap wizard met voortgang.",
    DesignDocumentTemplateKind.Dashboard => "KPI-kaarten en data-overzichten.",
    DesignDocumentTemplateKind.SidebarApp => "Volledige app met zijmenu-navigatie.",
    DesignDocumentTemplateKind.SettingsPage => "Instellingenpagina met secties.",
    DesignDocumentTemplateKind.TableWithFilters => "Gefilterde tabel met zoekbalk.",
    _ => string.Empty
};
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Components/DesignerShell.razor` | 1, 2 | Dark/light toggle knop, thema-dropdown met template en family-groepering |
| `Components/DesignerShell.razor.cs` | 1, 2 | `_isDarkMode`, `ToggleDarkLight`, `CanvasThemeGroupOptions`, `OnThemeFamilyChanged`, swatch/format helpers |
| `Components/DesignerStartScreen.razor` | 3 | Template-categorisering, grotere kaarten, beschrijvingen |
| `Components/DesignerStartScreen.razor.cs` | 3 | `BasicTemplates`, `AppTemplates`, `GetTemplateDescription` |
| `wwwroot/css/designer.css` | 2, 3 | Swatch CSS, template-grid upgrade |

## Verificatie

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Open designer | Dark/light toggle zichtbaar naast thema-dropdown |
| 2 | Klik toggle | Thema wisselt light↔dark, icoon verandert |
| 3 | Open thema-dropdown | Kleurbollen naast themanamen, 14 families |
| 4 | Selecteer "Dathomir" | Canvas wordt dathomir-dark (of -light), swatch is rood |
| 5 | Open startscherm | Templates in 2 categorieën ("Basis" en "App-patronen") |
| 6 | Kaarten zijn groter | 4:3 aspect ratio met live preview |
| 7 | Hover op kaart | Blauwe border + subtiele schaduw |
| 8 | Beschrijving zichtbaar | Korte tekst onder template-naam |
