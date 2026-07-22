# Prompt 78 — Designer WYSIWYG Transformatie: van structuur-editor naar live-app-editor

De designer werkt technisch maar voelt als een developer XML-structuur-editor. Deze prompt transformeert hem naar een echte WYSIWYG designer waar een niet-technische gebruiker direct ziet hoe de app eruitziet — inclusief data, theming, en navigatie.

Referentie: `docs/designer/BLUNT-UX-AUDIT-REPORT.md` voor de volledige analyse.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Canvas WYSIWYG: componenten tonen live data (KRITIEK)

### Probleem
Componenten op de canvas tonen lege velden. Een `AgtTextField` is een dashed box met een label erboven. Een `RadzenDataGrid` is leeg. De gebruiker kan niet zien hoe de app eruitziet.

### Doel
Elk component toont realistische placeholder-data of seeded data uit het `DesignDataModel`, zodat de canvas eruitziet als een werkende app.

### Implementatie

**Stap 1: Voeg een `DesignDataContext` service toe.**

Maak `src/Agterhuis.Ui.Designer/Services/DesignDataContext.cs`:

```csharp
namespace Agterhuis.Ui.Designer.Services;

/// <summary>
/// Provides seeded data to components rendered on the designer canvas.
/// Components check for this cascading value to show realistic preview data.
/// </summary>
public sealed class DesignDataContext
{
    public DesignDataModel DataModel { get; }
    public bool IsDesignMode { get; } = true;

    public DesignDataContext(DesignDataModel dataModel)
    {
        DataModel = dataModel;
    }

    /// <summary>
    /// Gets preview rows for a specific entity.
    /// Returns up to 10 rows for canvas preview, 25 for full preview mode.
    /// </summary>
    public IReadOnlyList<DesignSeedRow> GetPreviewRows(string entityName, int maxRows = 10)
        => DesignDataModelSeeder.GeneratePreview(DataModel, entityName)
            .Take(maxRows)
            .ToList();

    /// <summary>
    /// Gets a single sample value for a field — used by form components
    /// to show a realistic placeholder/value on the canvas.
    /// </summary>
    public object? GetSampleValue(string entityName, string fieldName)
    {
        var rows = GetPreviewRows(entityName, 1);
        if (rows.Count == 0) return null;
        return rows[0].Values.GetValueOrDefault(fieldName);
    }
}
```

**Stap 2: Cascade `DesignDataContext` vanuit `DesignerShell`.**

In `DesignerShell.razor`, wrap de canvas-sectie in een `CascadingValue`:

```razor
<CascadingValue Value="@_designDataContext" IsFixed="false">
    @* bestaande canvas content *@
</CascadingValue>
```

In `DesignerShell.razor.cs`, initialiseer:

```csharp
private DesignDataContext? _designDataContext;

// In OnInitialized of na document load:
_designDataContext = new DesignDataContext(_commands.Document.DataModel);
```

**Stap 3: Pas `DesignerCanvasNode` aan om default parameter-waarden te injecteren.**

Wanneer een component op de canvas geen expliciete waarde heeft voor bepaalde parameters, injecteer dan sample-data. In `DesignerCanvasNode.razor`, bij het opbouwen van de parameter dictionary voor `DynamicComponent`:

```csharp
private Dictionary<string, object?> BuildEffectiveParameters()
{
    var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

    // Bestaande logica: kopieer Node.Parameters
    foreach (var (key, value) in Node.Parameters)
    {
        parameters[key] = value.GetResolvedValue();
    }

    // NIEUW: als DesignDataContext beschikbaar is, vul ontbrekende velden aan
    if (DesignDataContext is not null)
    {
        InjectDesignTimeDefaults(parameters);
    }

    return parameters;
}

private void InjectDesignTimeDefaults(Dictionary<string, object?> parameters)
{
    var descriptor = Registry.GetDescriptor(Node.ComponentType);
    if (descriptor is null) return;

    // Voor form-componenten: toon een placeholder als er geen Value is
    if (IsFormComponent(Node.ComponentType) && !parameters.ContainsKey("Placeholder"))
    {
        var label = parameters.GetValueOrDefault("Label") as string ?? Node.ComponentType;
        parameters["Placeholder"] = $"Vul {label.ToLowerInvariant()} in...";
    }

    // Voor DataGrid: injecteer een lege Items-binding met preview-data
    // (dit vereist een DataGrid-specifieke aanpak — zie fase 2)
}

private static bool IsFormComponent(string componentType)
    => componentType is "AgtTextField" or "AgtNumericField" or "AgtDatePicker"
        or "AgtDropdown" or "AgtSwitch" or "AgtTextArea" or "AgtCheckboxField"
        or "RadzenTextBox" or "RadzenNumeric" or "RadzenDatePicker"
        or "RadzenDropDown" or "RadzenCheckBox" or "RadzenSwitch";
```

**Stap 4: Verberg structuur-chrome in resting state.**

In `designer.css`, maak de canvas-node volledig transparant in resting state — geen border, geen label, geen bar. Chrome verschijnt ALLEEN bij hover of selectie:

```css
/* Resting state: onzichtbaar chrome */
.designer-canvas-node {
    border: 1px solid transparent;
    border-radius: var(--agt-border-radius-sm);
    margin-bottom: 0;
    outline: none;
    position: relative;
    transition: border-color 120ms ease, box-shadow 120ms ease;
}

/* Hover: subtiele blauwe outline */
.designer-canvas-node--hover {
    outline: 2px solid var(--agt-color-primary-200);
    outline-offset: -1px;
}

/* Selected: stevige blauwe outline + label */
.designer-canvas-node--selected {
    outline: 2px solid var(--agt-color-primary-500);
    outline-offset: -1px;
    box-shadow: 0 0 0 1px var(--agt-color-primary-500);
}
```

**Stap 5: Vertaal slotnamen naar gebruikersvriendelijke labels.**

In `DesignerCanvasNode.razor`, maak een dictionary die technische slotnamen vertaalt:

```csharp
private static readonly Dictionary<string, string> SlotDisplayNames = new(StringComparer.Ordinal)
{
    ["ChildContent"] = "Inhoud",
    ["HeaderActions"] = "Knoppen rechtsboven",
    ["Logo"] = "Logo",
    ["Sidebar"] = "Zijmenu",
    ["Header"] = "Koptekst",
    ["Footer"] = "Voettekst",
    ["Columns"] = "Kolommen",
    ["Template"] = "Sjabloon",
    ["EmptyTemplate"] = "Lege weergave"
};

private static string GetSlotDisplayName(string slotName)
    => SlotDisplayNames.GetValueOrDefault(slotName, slotName);
```

Gebruik `GetSlotDisplayName()` overal waar slotnamen getoond worden (slot hints, breadcrumb, property panel).

### Verificatie
- Voeg een AgtTextField toe → toont label + placeholder "Vul klantnaam in..." op canvas.
- Voeg een AgtSwitch toe → toont label + switch in "uit" stand, geen lege wireframe.
- Hover over component → subtiele blauwe outline verschijnt.
- Geen hover → component ziet eruit als de echte app.
- Slotnamen tonen "Inhoud" i.p.v. "CHILDCONTENT".

---

## Fase 2 — Layout containment: alle layout-componenten werken op canvas (KRITIEK)

### Probleem
`AgtSidebarLayout` rendert buiten de canvas omdat `RadzenSidebar` `position: fixed` gebruikt. Dit geldt potentieel ook voor modals en dialogen.

### Implementatie

**Stap 1: CSS containment op canvas-node voor layout-componenten.**

In `designer.css`, voeg een `contain` rule toe op de canvas-node die een layout-component wraps:

```css
/* Containment voor layout-componenten die position:fixed gebruiken */
.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"],
.designer-canvas-node[data-agt-design-component="RadzenDialog"],
.designer-canvas-node[data-agt-design-component="RadzenPanel"] {
    contain: layout style paint;
    overflow: hidden;
    position: relative;
}

/* Override RadzenSidebar fixed positioning binnen designer */
.designer-canvas-node .rz-sidebar {
    position: relative !important;
    height: auto !important;
    min-height: 200px;
    width: 100% !important;
    z-index: auto !important;
}

/* AgtSidebarLayout specifiek: geef een minimale hoogte zodat de layout zichtbaar is */
.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] {
    min-height: 300px;
}

.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] .agt-sidebar-layout {
    height: 100%;
    min-height: inherit;
}

.designer-canvas-node[data-agt-design-component="AgtSidebarLayout"] .agt-sidebar-layout__main {
    min-height: 200px;
}
```

**Stap 2: Voeg `data-agt-design-component` toe aan de canvas-node wrapper.**

In `DesignerCanvasNode.razor`, controleer dat het root-element dit attribuut al heeft (het staat in de summary als bestaand). Zo niet, voeg toe:

```razor
<div ... data-agt-design-component="@Node.ComponentType" ...>
```

**Stap 3: Voeg een canvas-scope class toe op de canvas container.**

In `DesignerShell.razor`, voeg `designer-canvas-scope` toe aan de canvas container:

```razor
<div class="designer-canvas designer-canvas-scope" data-agt-theme="@_canvasTheme" @ref="_canvasRef">
```

Dan in CSS:

```css
.designer-canvas-scope .rz-sidebar {
    position: relative !important;
}

.designer-canvas-scope .rz-dialog-wrapper {
    position: absolute !important;
}
```

### Verificatie
- Voeg AgtSidebarLayout toe aan canvas → sidebar rendert BINNEN de canvas-node.
- Header, sidebar, content zijn zichtbaar binnen de component-grenzen.
- Selecteer de layout → blauwe outline rond het hele component.
- Resize het browservenster → layout past zich aan binnen de canvas, niet op viewport-niveau.

---

## Fase 3 — Rijke templates met seeded data (HOOG)

### Probleem
6 templates, elk 2-3 lege velden. Dashboard toont "Toon hier KPI's of grafieken." Het is beschamend vergeleken met Retool/Budibase die 20-50 volledig gestylde templates bieden.

### Implementatie

**Stap 1: Refactor `DesignDocumentTemplates` naar rijke templates.**

Herschrijf de template-builders zodat ze realistische, volledig ingevulde schermen opleveren. Elk template moet:
- Minstens 8-12 componenten bevatten
- Correcte layout met RadzenRow/RadzenColumn
- Realistische labels en placeholders gebaseerd op het datamodel
- Waar mogelijk: gebonden aan een entity uit het datamodel

Voorbeeld: het Dashboard-template moet bevatten:
- Een `AgtPageHeader` met titel "Schadedossier Dashboard"
- Een `RadzenRow` met 4x `RadzenColumn` Size=3, elk een `AgtCard` met een KPI (titel + getal)
- Een `RadzenRow` met een `RadzenColumn` Size=8 voor een `RadzenDataGrid` en Size=4 voor een statusoverzicht
- Alle labels moeten domein-specifiek zijn (Schadedossiers, Klanten, Werkorders)

**Stap 2: Voeg nieuwe template-soorten toe.**

Voeg toe aan `DesignDocumentTemplateKind`:

```csharp
public enum DesignDocumentTemplateKind
{
    Blank,
    FormPage,
    ListCrud,
    MasterDetail,
    Wizard,
    Dashboard,
    // NIEUW:
    SidebarApp,      // Volledige app met sidebar navigatie + meerdere pagina's
    SettingsPage,     // Instellingen-formulier met secties en toggles
    DetailPage,       // Detail-weergave met tabbladen en gerelateerde data
    KanbanBoard,      // Kolommen-gebaseerd overzicht (Nieuw → In behandeling → Gereed)
    LoginPage,        // Login-formulier met branding
    TableWithFilters  // Geavanceerde tabel met zoek, filter, paginering
}
```

**Stap 3: Maak de SidebarApp-template multi-page.**

De `SidebarApp` template moet meerdere pagina's bevatten:

```csharp
private static DesignDocument BuildSidebarApp(string name)
    => new()
    {
        Name = name,
        Version = "1.0",
        DataModel = DesignDataModelSeeder.CreateDefault(),
        Pages =
        [
            CreateBasePage("/", "Dashboard", CreateSidebarDashboardNodes()),
            CreateBasePage("/dossiers", "Schadedossiers", CreateSidebarListNodes()),
            CreateBasePage("/dossier/nieuw", "Nieuw dossier", CreateSidebarFormNodes()),
            CreateBasePage("/instellingen", "Instellingen", CreateSidebarSettingsNodes())
        ]
    };
```

Elke pagina gebruikt `AgtSidebarLayout` als root met:
- Logo slot: bedrijfsnaam
- Sidebar slot: navigatie-links naar de andere pagina's
- HeaderActions slot: gebruikersnaam + logout-knop
- ChildContent slot: de pagina-specifieke inhoud

**Stap 4: Update `DesignerStartScreen` voor de nieuwe templates.**

Voeg iconen en beschrijvingen toe voor de nieuwe templates. Groepeer ze visueel:
- **Starten:** Leeg canvas, Invulformulier, Overzichtstabel
- **Applicaties:** Sidebar App, Dashboard, Master-detail
- **Pagina's:** Login, Instellingen, Detail, Kanban
- **Structuur:** Wizard, Tabel met filters

### Verificatie
- Open de designer → startscherm toont 12 templates in gegroepeerde grid.
- Kies "Sidebar App" → 4 pagina's worden aangemaakt met sidebar-navigatie.
- Kies "Dashboard" → canvas toont 4 KPI-kaarten + datatabel, niet lege slots.
- Elke template toont een live preview in de template-card.

---

## Fase 4 — Seeded data in preview en canvas (HOOG)

### Probleem
Preview mode toont exact dezelfde lege componenten als de canvas. Er is geen verschil tussen bewerken en preview. De rijke seed-data uit `DesignDataModelSeeder` (kentekens, dossiernummers, facturen) wordt nergens getoond.

### Implementatie

**Stap 1: Maak een `DesignPreviewRenderer` die data injecteert.**

Maak `src/Agterhuis.Ui.Designer/Components/DesignPreviewRenderer.razor`:

Dit component doet wat `DesignRenderer` doet, maar wraps elke component in een `CascadingValue<DesignDataContext>` en resolvet data-bindings naar seeded data.

**Stap 2: Implementeer data-binding resolutie.**

Wanneer een component een parameter heeft met een data-binding (bijv. `{binding:Schadedossier.Dossiernummer}`), los die op naar de seeded data:

```csharp
private object? ResolveBinding(string bindingExpression, DesignDataContext context)
{
    // Parse "Schadedossier.Dossiernummer" → entity + field
    var parts = bindingExpression.Split('.');
    if (parts.Length != 2) return null;

    return context.GetSampleValue(parts[0], parts[1]);
}
```

**Stap 3: Toon seeded data in DataGrid-componenten.**

Wanneer een `RadzenDataGrid` op de canvas staat en een data-binding heeft naar een entity, genereer automatisch `Items` met seeded data:

```csharp
if (Node.ComponentType is "RadzenDataGrid" or "AgtDataGrid")
{
    var entityBinding = GetEntityBinding(Node);
    if (entityBinding is not null && context is not null)
    {
        var rows = context.GetPreviewRows(entityBinding, 10);
        // Converteer naar dynamische objecten die RadzenDataGrid kan tonen
        parameters["Data"] = ConvertToDataGridItems(rows);
    }
}
```

**Stap 4: Gebruik `DesignPreviewRenderer` in preview-mode.**

In `DesignerShell.razor`, vervang `DesignRenderer` in de preview-sectie:

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

### Verificatie
- Voeg een DataGrid toe, bind aan "Schadedossier" → canvas toont 10 rijen met dossiernummers, statussen, datums.
- Klik Preview → formuliervelden tonen ingevulde waarden, tabellen tonen data.
- Wissel tussen bewerken en preview → preview toont data, bewerken toont bewerkbare velden met placeholders.

---

## Fase 5 — Theme switching prominent en instant (MEDIUM)

### Probleem
Theme switching zit verstopt in Instellingen submenu. Geen visuele preview van het thema-effect. Geen quick-toggle voor dark/light.

### Implementatie

**Stap 1: Voeg een theme-selector toe aan de toolbar.**

In `DesignerShell.razor`, voeg direct naast de viewport-toggle een theme-dropdown toe:

```razor
<div class="designer-toolbar__theme">
    <RadzenDropDown TValue="string"
                    Data="CanvasThemeOptions"
                    Value="@_canvasTheme"
                    Change="OnCanvasThemeChanged"
                    Style="min-width: 140px;"
                    Placeholder="Thema" />
    <button type="button"
            class="designer-toolbar__darkmode"
            title="@(_canvasTheme.EndsWith("-dark") ? "Licht thema" : "Donker thema")"
            @onclick="ToggleDarkLight">
        <RadzenIcon Icon="@(_canvasTheme.EndsWith("-dark") ? "light_mode" : "dark_mode")" />
    </button>
</div>
```

**Stap 2: Implementeer `ToggleDarkLight`.**

```csharp
private async Task ToggleDarkLight()
{
    var parts = _canvasTheme.Split('-');
    var family = parts[0]; // e.g. "plum"
    var isDark = parts.Length > 1 && parts[1] == "dark";
    var newTheme = isDark ? family : $"{family}-dark";
    await OnCanvasThemeChanged(newTheme);
}
```

**Stap 3: Verwijder de theme-dropdown uit het Instellingen submenu.**

Nu het op de toolbar staat, is de submenu-locatie overbodig. Verwijder de theme-gerelateerde items uit het Instellingen-menu.

**Stap 4: Voeg CSS toe voor de toolbar theme-selector.**

```css
.designer-toolbar__theme {
    align-items: center;
    display: flex;
    gap: var(--agt-spacing-1);
}

.designer-toolbar__darkmode {
    align-items: center;
    background: transparent;
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-body);
    cursor: pointer;
    display: flex;
    font: inherit;
    height: 2rem;
    justify-content: center;
    padding: 0;
    width: 2rem;
}

.designer-toolbar__darkmode:hover {
    background: var(--agt-alpha-primary-10);
}
```

### Verificatie
- Toolbar toont theme-dropdown + dark/light toggle-knop.
- Klik dark/light toggle → canvas schakelt instant tussen plum en plum-dark.
- Kies "ocean" uit dropdown → alle componenten op canvas veranderen naar ocean-kleuren.
- Export → exporteert met het geselecteerde thema.

---

## Fase 6 — Multi-screen met navigatie (MEDIUM)

### Probleem
Page tabs bestaan maar templates zijn single-page. Geen manier om tussen pagina's te navigeren in preview. Geen navigatie-component.

### Implementatie

**Stap 1: Voeg een `AgtNavLink` component toe aan het component-register.**

Maak `src/Agterhuis.Ui/Components/Navigation/AgtNavLink.razor`:

```razor
<a class="agt-nav-link @(IsActive ? "agt-nav-link--active" : string.Empty) @CssClass"
   href="@Href"
   @onclick="OnClick"
   @onclick:preventDefault="true">
    @if (!string.IsNullOrEmpty(Icon))
    {
        <RadzenIcon Icon="@Icon" />
    }
    <span>@Text</span>
</a>
```

Parameters: `Text`, `Href`, `Icon`, `IsActive`, `CssClass`.

**Stap 2: Registreer in `DesignerComponentRegistry`.**

Voeg `AgtNavLink` toe aan de registry met categorie "Navigatie", icoon "link", en designer display name "Navigatielink".

**Stap 3: Voeg "Nieuwe pagina vanuit template" toe.**

In `DesignerShell.razor`, bij de "+" knop voor pagina's, toon een dropdown met template-opties in plaats van direct een lege pagina:

```razor
<div class="designer-page-tabs__add-menu">
    <button @onclick="() => OnAddPageAsync()">Lege pagina</button>
    @foreach (var template in TemplateDefinitions.Where(t => t.Kind != DesignDocumentTemplateKind.SidebarApp))
    {
        <button @onclick="() => OnAddPageFromTemplateAsync(template.Kind)">@template.DisplayName</button>
    }
</div>
```

**Stap 4: Preview-mode navigatie.**

In preview-mode, wanneer een `AgtNavLink` wordt geklikt, navigeer naar de bijbehorende pagina-tab:

```csharp
private void OnPreviewNavigate(string route)
{
    var targetPage = _commands.Document.Pages
        .FirstOrDefault(p => string.Equals(p.Route, route, StringComparison.OrdinalIgnoreCase));
    if (targetPage is not null)
    {
        _activePageIndex = _commands.Document.Pages.IndexOf(targetPage);
        StateHasChanged();
    }
}
```

### Verificatie
- Maak een SidebarApp template → 4 pagina's verschijnen als tabs.
- In preview: klik op "Schadedossiers" in de sidebar → navigeert naar de dossiers-pagina.
- Voeg een nieuwe pagina toe → dropdown toont template-opties.
- AgtNavLink verschijnt in het palet onder "Navigatie".

---

## Fase 7 — Export met seeded data switch (MEDIUM)

### Probleem
Export genereert een `DesignDataService` met `random.Next()` die "string.Empty" produceert voor tekstvelden. De rijke data uit `DesignDataModelSeeder` wordt niet meegeëxporteerd. Er is geen toggle voor de ontwikkelaar.

### Implementatie

**Stap 1: Voeg een `UseSeedData` optie toe aan de export-dialog.**

In de export-dialog, voeg een checkbox toe:

```razor
<AgtSwitch Label="Seeded data meegeven"
           Value="@_exportIncludeSeedData"
           ValueChanged="v => _exportIncludeSeedData = v" />
<p class="designer-export__hint">
    Wanneer ingeschakeld bevat het geëxporteerde project realistische voorbeelddata.
    De ontwikkelaar kan dit uitschakelen via <code>appsettings.json</code>.
</p>
```

**Stap 2: Genereer een rijkere `DesignDataService`.**

Pas `ProjectExporter.GenerateDataService()` aan zodat het de daadwerkelijke seed-logica uit `DesignDataModelSeeder` meeneemt:

```csharp
private static string GenerateSeedLiteral(DesignField field, string entityName)
    => (entityName, field.Name, field.Type) switch
    {
        ("Schadedossier", "Dossiernummer", _) => "$\"ATG-2024-{index + 1:00000}\"",
        ("Schadedossier", "Status", _) => "Pick(random, new[] { \"Nieuw\", \"Ingepland\", \"InBehandeling\", \"Gereed\" })",
        ("Klant", "Klantnaam", _) => "$\"Klant {index + 1}\"",
        ("Klant", "Email", _) => "$\"klant{index + 1}@voorbeeld.nl\"",
        // ... alle entity-specifieke mappings uit DesignDataModelSeeder
        _ => field.Type switch { /* fallbacks */ }
    };
```

**Stap 3: Voeg een `appsettings.json` toggle toe aan het geëxporteerde project.**

In de export-template `Program.template`, voeg toe:

```csharp
// Seed data configuratie
builder.Services.AddSingleton<DesignDataService>();

var useSeedData = builder.Configuration.GetValue<bool>("UseSeedData", __USE_SEED_DATA_DEFAULT__);
if (useSeedData)
{
    builder.Services.AddSingleton<IDataProvider, SeedDataProvider>();
}
```

En een `appsettings.json` template:

```json
{
  "UseSeedData": true
}
```

**Stap 4: Genereer een `SeedDataProvider` die de data levert.**

Het geëxporteerde project krijgt een `SeedDataProvider` die de rijke data uit het datamodel servet, en een `IDataProvider` interface zodat de ontwikkelaar later naar een echte API kan switchen.

### Verificatie
- Export met "Seeded data meegeven" AAN → project bevat `SeedDataProvider` met realistische data.
- Open geëxporteerd project → `dotnet run` → app toont kentekens, dossiernummers, facturen.
- Zet `UseSeedData: false` in appsettings → app toont lege state.
- Export met toggle UIT → project bevat geen seed data, alleen lege `IDataProvider`.

---

## Fase 8 — Startscherm upgrade (LAAG)

### Probleem
Het startscherm toont templates als kaarten met een live preview, maar de previews zijn klein en de templates zijn mager. Met de rijke templates uit fase 3 wordt het startscherm automatisch beter, maar er zijn nog verbeteringen.

### Implementatie

**Stap 1: Vergroot de template-card preview.**

In `designer.css`, vergroot de preview-sectie van de template-kaart:

```css
.designer-template-card__preview {
    aspect-ratio: 16 / 10;
    border-top: 1px solid var(--agt-input-border);
    overflow: hidden;
    pointer-events: none;
    position: relative;
}

/* Schaal de preview-inhoud zodat het een miniatuur is */
.designer-template-card__preview > .agt-design-renderer {
    transform: scale(0.4);
    transform-origin: top left;
    width: 250%;
}
```

**Stap 2: Voeg een "Populair" badge toe aan de meest gebruikte templates.**

```razor
@if (option.Kind is DesignDocumentTemplateKind.SidebarApp or DesignDocumentTemplateKind.Dashboard)
{
    <span class="designer-template-card__badge">Populair</span>
}
```

**Stap 3: Groepeer templates visueel.**

Gebruik `<h2>` headers om de groepen uit fase 3 te scheiden: "Applicaties", "Pagina's", "Structuur".

### Verificatie
- Startscherm toont gegroepeerde, visueel aantrekkelijke template-kaarten.
- Preview-miniaturen tonen de rijke templates met data.
- "Populair" badge op SidebarApp en Dashboard.

---

## Samenvatting wijzigingen per bestand

| Bestand | Fasen | Wijziging |
|---------|-------|-----------|
| `Services/DesignDataContext.cs` | 1, 4 | NIEUW — data provider voor canvas |
| `Components/DesignerCanvasNode.razor` | 1, 2 | Design-time defaults, slot-namen vertaling, data-attribuut |
| `wwwroot/css/designer.css` | 1, 2, 5, 8 | Canvas-node chrome, containment, theme-selector, template-cards |
| `Components/DesignerShell.razor` | 2, 4, 5, 6 | Canvas scope, preview renderer, theme toolbar, page-add menu |
| `Components/DesignerShell.razor.cs` | 4, 5, 6 | DesignDataContext init, ToggleDarkLight, preview navigatie |
| `Components/DesignPreviewRenderer.razor` | 4 | NIEUW — preview met data |
| `Model/DesignDocumentTemplateKind.cs` | 3 | 6 nieuwe template-soorten |
| `Model/DesignDocumentTemplates.cs` | 3 | Rijke templates met layout, data, meerdere pagina's |
| `Components/DesignerStartScreen.razor` | 3, 8 | Template-groepering, badges, grotere previews |
| `Components/Navigation/AgtNavLink.razor` | 6 | NIEUW — navigatie-component |
| `Registry/DesignerComponentRegistry.g.cs` | 6 | AgtNavLink registratie |
| `Export/ProjectExporter.cs` | 7 | Rijke seed data, UseSeedData toggle |
| `Export/Templates/Program.template` | 7 | SeedDataProvider configuratie |
| `Export/Templates/appsettings.json.template` | 7 | NIEUW — UseSeedData toggle |

## Volgorde van uitvoering

1. **Fase 2** — Layout containment (onblokt fase 3 SidebarApp template)
2. **Fase 1** — Canvas WYSIWYG (fundamenteel voor alle andere fasen)
3. **Fase 3** — Rijke templates (gebruikt fase 1 + 2)
4. **Fase 4** — Seeded data preview (bouwt voort op fase 1)
5. **Fase 5** — Theme switching (onafhankelijk)
6. **Fase 6** — Multi-screen navigatie (bouwt voort op fase 3)
7. **Fase 7** — Export met seed data (bouwt voort op fase 4)
8. **Fase 8** — Startscherm (profiteert van alle eerdere fasen)

Elke fase is een eigen commit (of meerdere) met beschrijvende message.

## Verificatie — integratietest na alle fasen

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **End-to-end scenario:**

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Open designer → startscherm | 12 templates, gegroepeerd, met live previews met data |
| 2 | Kies "Sidebar App" | 4 pagina's, sidebar-navigatie, rijke layout |
| 3 | Canvas toont | Componenten met data, geen wireframe-borders in resting state |
| 4 | Hover over component | Blauwe outline + label verschijnt |
| 5 | Selecteer DataGrid | Toont 10 rijen seeded data (dossiernummers, statussen) |
| 6 | Klik Preview | Alle velden ingevuld, tabellen vol, navigatie werkt |
| 7 | Klik "Schadedossiers" in sidebar | Navigeert naar dossiers-pagina |
| 8 | Toggle dark/light op toolbar | Instant thema-wisseling |
| 9 | Kies "ocean" thema | Alle componenten in ocean-kleuren |
| 10 | Export met "Seeded data meegeven" AAN | ZIP bevat SeedDataProvider |
| 11 | `dotnet run` geëxporteerd project | App draait met realistische data |
| 12 | Zet UseSeedData: false | App toont lege state |
