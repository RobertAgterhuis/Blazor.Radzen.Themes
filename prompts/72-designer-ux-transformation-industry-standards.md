# Prompt 72 — Designer UX-transformatie: van structuur-editor naar visuele designer

Na prompts 67–71 werken alle controls correct, maar de designer voelt nog steeds aan als een **gestructureerde formulier-editor** in plaats van een **visuele designer**. Dit prompt transformeert de UX fundamenteel op basis van concrete patronen uit Webflow, Retool, Figma en GrapeJS.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Kernprobleem — waarom het niet "goed voelt"

| Wat de gebruiker ziet | Wat Webflow/Figma doet | Impact |
|---|---|---|
| Elk component heeft een zichtbare border + achtergrond + node-bar, zelfs in rust | Canvas toont de component exact zoals de eindpagina eruitziet; chrome verschijnt alleen bij hover/selectie | Onze canvas ziet eruit als een formulier met dozen, niet als een pagina |
| 5 panels tegelijk zichtbaar (palet, canvas, properties, data, structuurboom) + optioneel code | Max 3 panels: links navigator/palet, midden canvas, rechts inspector | Scherm voelt overvol en overweldigend |
| Palet is een verticale tekstlijst met grip-dots | Palet is een visueel grid van icoon-kaartjes, compact en scanbaar | Moeilijk het juiste component te vinden |
| "Voeg rij toe" is een native `<select>` + knop | Visuele layout-thumbnails die je klikt of sleept | Voelt als een developer-tool in plaats van een designer |
| Banners stapelen bovenaan (recovery, offline, draft) | Eén subtiele statusbalk of toast | Duwt de workspace naar beneden |
| Route-preview als eigen regel | Verborgen in pagina-instellingen of breadcrumb | Verspilt verticale ruimte |
| Structuurboom en data-panel als aparte panelen rechts | Navigator (boom) links als tab naast palet; data als tab in inspector | Visuele ruis |

---

## Fase 1 — Canvas WYSIWYG-modus: content first, chrome second

### Doel
In rusttoestand toont de canvas de componenten **exact zoals ze er in de eindpagina uitzien** — geen borders, geen node-bars, geen achtergrondkleur-per-node. Chrome (outline, label, toolbar) verschijnt alleen bij hover en selectie, precies zoals Webflow.

### Wijzigingen

**In `designer.css`, wijzig `.designer-canvas-node`:**

```css
/* RUST-TOESTAND: onzichtbare wrapper — de component IS de visuele laag */
.designer-canvas-node {
    border: 1px solid transparent;                     /* was: var(--agt-input-border) */
    border-radius: var(--agt-border-radius-sm);
    margin-bottom: 0;                                  /* was: var(--agt-spacing-3) */
    outline: 2px solid transparent;
    outline-offset: -2px;
    overflow: visible;                                 /* was: clip — clip knipt de label af */
    position: relative;
    transition: border-color 120ms ease, outline-color 120ms ease, box-shadow 120ms ease;
}

/* HOVER: subtiele blauwe omlijning — Webflow-model */
.designer-canvas-node--hover {
    border-color: var(--agt-color-primary-200);
    outline-color: var(--agt-color-primary-200);
}

/* SELECTIE: prominente blauwe omlijning — Webflow-model */
.designer-canvas-node--selected {
    background: transparent;                           /* was: var(--agt-alpha-primary-5) */
    border-color: var(--agt-color-primary-500);
    outline-color: var(--agt-color-primary-500);
    box-shadow: 0 0 0 1px var(--agt-color-primary-500);   /* dunner: was 3px */
}
```

**Verberg de node-bar standaard — toon alleen bij hover/selectie:**

```css
.designer-canvas-node__bar {
    align-items: center;
    background: transparent;                           /* was: color-mix(...) */
    border-bottom: none;                               /* was: 1px solid */
    display: flex;
    min-height: 0;                                     /* was: 1.4rem */
    opacity: 0;                                        /* VERBORGEN in rust */
    padding: 0 var(--agt-spacing-1);
    position: absolute;                                /* overlay, niet in flow */
    right: 0;
    top: 0;
    transition: opacity 120ms ease;
    z-index: 5;
}

.designer-canvas-node--hover > .designer-canvas-node__bar,
.designer-canvas-node--selected > .designer-canvas-node__bar {
    background: color-mix(in srgb, var(--agt-surface-1) 85%, transparent);
    min-height: 1.2rem;
    opacity: 1;
}
```

**Verwijder de nesting-diepte borders — deze creëren visuele ruis:**

```css
/* VERWIJDER of neutraliseer: */
.designer-canvas-node .designer-canvas-node {
    background: transparent;       /* was: color-mix(...) */
    border-left: none;             /* was: 3px solid */
}

.designer-canvas-node .designer-canvas-node .designer-canvas-node {
    background: transparent;
    border-left: none;
}
```

**Verwijder de preview-padding — laat de component de volledige ruimte innemen:**

```css
.designer-canvas-node__preview {
    padding: 0;                    /* was: var(--agt-spacing-3) */
    position: relative;
}
```

### Canvas achtergrond

De canvas zelf krijgt een subtiel wit/licht oppervlak zodat het aanvoelt als een "pagina":

```css
.designer-canvas {
    background: var(--agt-surface-0);
    border-radius: var(--agt-border-radius-md);
    min-height: 60vh;
    padding: var(--agt-spacing-4);
}
```

### Verificatie
- Canvas toont componenten zonder zichtbare borders/achtergronden in rusttoestand.
- Hover over een component → blauwe outline + label verschijnt.
- Selecteer een component → prominente blauwe outline + floating toolbar.
- De canvas ziet er nu uit als een echte pagina in plaats van een boom van dozen.

---

## Fase 2 — Panelconsolidatie: van 5 naar 3 panels

### Doel
Reduceer het aantal gelijktijdig zichtbare panels van 5 naar 3, conform de industriestandaard: **links palet/navigator (tabs), midden canvas, rechts inspector**.

### Huidige layout
```
[Palet] | [Canvas] | [Properties] [Data] [Structuur]
                                    ↑ drie panels rechts = onoverzichtelijk
```

### Gewenste layout (Webflow/Retool-model)
```
[Palet │ Navigator] | [Canvas] | [Inspector]
   tabs links           ↑           tabs: Properties + Data
                    focus area
```

### Wijzigingen

**Stap 1: Combineer Palet + Structuurboom als tabs in het linkerpanel.**

In `DesignerShell.razor`, vervang de aparte `designer-panel--palette` en `designer-panel--tree` door één panel met tabs:

```razor
<aside class="designer-panel designer-panel--left @(_leftPanelCollapsed ? "designer-panel--collapsed" : string.Empty)" aria-label="Palet en navigator">
    <div class="designer-panel__header">
        @if (!_leftPanelCollapsed)
        {
            <div class="designer-panel-tabs" role="tablist">
                <button type="button" role="tab" class="designer-panel-tab @(_leftTab == LeftPanelTab.Palette ? "designer-panel-tab--active" : string.Empty)" @onclick="() => _leftTab = LeftPanelTab.Palette">
                    <RadzenIcon Icon="widgets" /> Palet
                </button>
                <button type="button" role="tab" class="designer-panel-tab @(_leftTab == LeftPanelTab.Navigator ? "designer-panel-tab--active" : string.Empty)" @onclick="() => _leftTab = LeftPanelTab.Navigator">
                    <RadzenIcon Icon="account_tree" /> Navigator
                </button>
            </div>
        }
        <button type="button" class="designer-panel__toggle" @onclick="ToggleLeftPanelCollapsed">
            <RadzenIcon Icon="@(_leftPanelCollapsed ? "chevron_right" : "chevron_left")" />
        </button>
    </div>

    @if (!_leftPanelCollapsed)
    {
        @if (_leftTab == LeftPanelTab.Palette)
        {
            @* Bestaande palette-inhoud: filter + categorieën *@
            <input class="designer-filter" type="text" placeholder="Zoek component..." value="@_paletteFilter" @oninput="OnPaletteFilterChanged" />
            @foreach (var category in PaletteByCategory)
            {
                @* ... bestaande palette rendering ... *@
            }
        }
        else
        {
            @* Bestaande structuurboom-inhoud *@
            <div class="designer-tree" aria-label="Navigator">
                @RenderTreeNodes(TreeRoots)
            </div>
            @* Tree context menu ... *@
        }
    }
</aside>
```

**Stap 2: Integreer Data-panel als tab in het rechterpanel (Inspector).**

In plaats van een apart data-panel, voeg een "Data" tab toe aan het rechter inspector-panel:

```razor
<aside class="designer-panel designer-panel--right" aria-label="Inspector">
    <div class="designer-inspector-mode" role="tablist">
        <button type="button" role="tab" class="designer-panel-tab @(_rightTab == RightPanelTab.Properties ? "designer-panel-tab--active" : string.Empty)" @onclick="() => _rightTab = RightPanelTab.Properties">
            <RadzenIcon Icon="tune" /> Eigenschappen
        </button>
        <button type="button" role="tab" class="designer-panel-tab @(_rightTab == RightPanelTab.Data ? "designer-panel-tab--active" : string.Empty)" @onclick="() => _rightTab = RightPanelTab.Data">
            <RadzenIcon Icon="storage" /> Data
        </button>
    </div>

    @if (_rightTab == RightPanelTab.Properties)
    {
        <PropertyPanel ... /> @* bestaande binding *@
    }
    else
    {
        <DesignerDataPanel ... /> @* bestaande binding *@
    }
</aside>
```

**Stap 3: Voeg enums toe aan `DesignerShell.razor.cs`:**

```csharp
private enum LeftPanelTab { Palette, Navigator }
private enum RightPanelTab { Properties, Data }

private LeftPanelTab _leftTab = LeftPanelTab.Palette;
private RightPanelTab _rightTab = RightPanelTab.Properties;
private bool _leftPanelCollapsed;

private void ToggleLeftPanelCollapsed()
{
    _leftPanelCollapsed = !_leftPanelCollapsed;
    _ = PersistLayoutStateAsync();
}
```

Verwijder de aparte `_paletteCollapsed`, `_dataCollapsed`, `_treeCollapsed` booleans en vervang door `_leftPanelCollapsed` + tab-state. Update `PersistLayoutStateAsync()` en de restore-logica.

**Stap 4: Verwijder de aparte dividers voor data en tree panels.**

De `designer-grid` layout wordt eenvoudiger — drie kolommen:

```css
.designer-grid {
    display: flex;
    flex: 1;
    gap: 0;
    min-height: 0;
    overflow: hidden;
}
```

Slechts twee dividers: `palette-canvas` en `canvas-property`.

**Stap 5: CSS voor panel-tabs:**

```css
.designer-panel-tabs {
    display: flex;
    gap: 0;
    width: 100%;
}

.designer-panel-tab {
    align-items: center;
    background: transparent;
    border: 0;
    border-bottom: 2px solid transparent;
    color: var(--agt-text-muted);
    cursor: pointer;
    display: inline-flex;
    flex: 1;
    font: inherit;
    font-size: var(--agt-font-size-sm);
    gap: var(--agt-spacing-1);
    justify-content: center;
    padding: var(--agt-spacing-1) var(--agt-spacing-2);
    transition: color 120ms, border-color 120ms;
}

.designer-panel-tab:hover {
    color: var(--agt-text-body);
}

.designer-panel-tab--active {
    border-bottom-color: var(--agt-color-primary-500);
    color: var(--agt-color-primary-500);
    font-weight: 600;
}

.designer-panel-tab .rzi {
    font-size: 1rem;
}
```

### Verificatie
- Links: twee tabs (Palet, Navigator). Klik op Navigator → structuurboom verschijnt.
- Rechts: twee tabs (Eigenschappen, Data). Klik op Data → data-panel verschijnt.
- Canvas heeft nu meer ruimte (geen 5 smalle panels meer).
- Collapsible: linker- en rechterpanel kunnen ingeklapt worden.

---

## Fase 3 — Palette als visueel component-grid

### Doel
Vervang de verticale tekstlijst door een compact **icoon-grid** (2 kolommen) dat scanbaar en visueel herkenbaar is, vergelijkbaar met Webflow's Add Elements panel.

### Wijzigingen

**In `DesignerShell.razor`, wijzig de palette-rendering:**

```razor
@foreach (var category in PaletteByCategory)
{
    <section class="designer-category">
        <h3>@category.Key</h3>
        <div class="designer-palette-grid">
            @foreach (var descriptor in category.Value)
            {
                <button type="button"
                        class="designer-palette-card @(IsPaletteDragging(descriptor.ComponentType) ? "designer-palette-card--dragging" : string.Empty)"
                        draggable="true"
                        @ondragstart='args => OnPaletteDragStart(args, descriptor)'
                        @ondragend="OnDragEnd"
                        @onclick="() => OnPaletteItemClickedAsync(descriptor.ComponentType)"
                        title="@(descriptor.DesignerDescription ?? descriptor.ComponentType)">
                    <RadzenIcon Icon="@(descriptor.DesignerIcon ?? descriptor.Icon)" />
                    <span class="designer-palette-card__name">@(descriptor.DesignerDisplayName ?? descriptor.DisplayName)</span>
                </button>
            }
        </div>
    </section>
}
```

**CSS:**

```css
.designer-palette-grid {
    display: grid;
    gap: var(--agt-spacing-1);
    grid-template-columns: 1fr 1fr;
}

.designer-palette-card {
    align-items: center;
    background: var(--agt-surface-0);
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-body);
    cursor: grab;
    display: flex;
    flex-direction: column;
    font: inherit;
    font-size: var(--agt-font-size-xs);
    gap: var(--agt-spacing-1);
    justify-content: center;
    min-height: 4rem;
    padding: var(--agt-spacing-2) var(--agt-spacing-1);
    text-align: center;
    transition: border-color 120ms, background 120ms, box-shadow 120ms;
}

.designer-palette-card .rzi {
    color: var(--agt-color-primary-500);
    font-size: 1.4rem;
}

.designer-palette-card:hover {
    background: var(--agt-alpha-primary-5);
    border-color: var(--agt-color-primary-300);
    box-shadow: 0 2px 8px var(--agt-shadow-1);
}

.designer-palette-card:active {
    background: var(--agt-alpha-primary-10);
    transform: scale(0.97);
}

.designer-palette-card--dragging {
    opacity: 0.3;
}

.designer-palette-card__name {
    line-height: 1.2;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    width: 100%;
}
```

Verwijder de oude `.designer-palette-item` regels (en de `__grip`, `__meta` subklassen).

### Verificatie
- Palet toont componenten als 2-kolom grid van kaartjes met icoon + naam.
- Hover → kaartje licht op.
- Drag → kaartje wordt transparant.
- Klik → component wordt aan canvas toegevoegd.
- Filter werkt nog steeds.

---

## Fase 4 — Layout-presets als visuele kaarten

### Doel
Vervang de `<select>` + knop ("Voeg rij toe") door visuele layout-thumbnails die je in één klik toevoegt.

### Wijzigingen

**In `DesignerShell.razor`, vervang het `.designer-quick-layout` blok:**

```razor
<div class="designer-layout-presets">
    <span class="designer-layout-presets__label">Rij toevoegen</span>
    <div class="designer-layout-presets__grid">
        <button type="button" class="designer-layout-preset" title="1 kolom (12)" @onclick='() => AddRowLayoutPreset("12")'>
            <div class="designer-layout-preset__preview">
                <div class="designer-layout-preset__col" style="flex:1;"></div>
            </div>
            <span>1 kolom</span>
        </button>
        <button type="button" class="designer-layout-preset" title="2 kolommen (6+6)" @onclick='() => AddRowLayoutPreset("6+6")'>
            <div class="designer-layout-preset__preview">
                <div class="designer-layout-preset__col" style="flex:1;"></div>
                <div class="designer-layout-preset__col" style="flex:1;"></div>
            </div>
            <span>2 kolommen</span>
        </button>
        <button type="button" class="designer-layout-preset" title="3 kolommen (4+4+4)" @onclick='() => AddRowLayoutPreset("4+4+4")'>
            <div class="designer-layout-preset__preview">
                <div class="designer-layout-preset__col" style="flex:1;"></div>
                <div class="designer-layout-preset__col" style="flex:1;"></div>
                <div class="designer-layout-preset__col" style="flex:1;"></div>
            </div>
            <span>3 kolommen</span>
        </button>
        <button type="button" class="designer-layout-preset" title="Zijbalk + hoofd (4+8)" @onclick='() => AddRowLayoutPreset("4+8")'>
            <div class="designer-layout-preset__preview">
                <div class="designer-layout-preset__col" style="flex:1;"></div>
                <div class="designer-layout-preset__col" style="flex:2;"></div>
            </div>
            <span>Zijbalk + hoofd</span>
        </button>
        <button type="button" class="designer-layout-preset" title="Hoofd + zijbalk (8+4)" @onclick='() => AddRowLayoutPreset("8+4")'>
            <div class="designer-layout-preset__preview">
                <div class="designer-layout-preset__col" style="flex:2;"></div>
                <div class="designer-layout-preset__col" style="flex:1;"></div>
            </div>
            <span>Hoofd + zijbalk</span>
        </button>
    </div>
</div>
```

**Voeg `AddRowLayoutPreset` toe aan `DesignerShell.razor.cs`:**

```csharp
private Task AddRowLayoutPreset(string layout)
{
    _selectedRowLayout = layout;
    return AddRowLayoutAsync();
}
```

**CSS:**

```css
.designer-layout-presets {
    border-top: 1px dashed var(--agt-input-border);
    margin-top: var(--agt-spacing-4);
    padding-top: var(--agt-spacing-3);
}

.designer-layout-presets__label {
    color: var(--agt-text-muted);
    display: block;
    font-size: var(--agt-font-size-xs);
    font-weight: 600;
    letter-spacing: 0.5px;
    margin-bottom: var(--agt-spacing-2);
    text-transform: uppercase;
}

.designer-layout-presets__grid {
    display: flex;
    flex-wrap: wrap;
    gap: var(--agt-spacing-2);
}

.designer-layout-preset {
    align-items: center;
    background: var(--agt-surface-0);
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-muted);
    cursor: pointer;
    display: flex;
    flex-direction: column;
    font: inherit;
    font-size: var(--agt-font-size-xs);
    gap: var(--agt-spacing-1);
    padding: var(--agt-spacing-2);
    transition: border-color 120ms, background 120ms;
    width: 5.5rem;
}

.designer-layout-preset:hover {
    background: var(--agt-alpha-primary-5);
    border-color: var(--agt-color-primary-300);
    color: var(--agt-text-body);
}

.designer-layout-preset__preview {
    display: flex;
    gap: 2px;
    height: 1.5rem;
    width: 100%;
}

.designer-layout-preset__col {
    background: var(--agt-color-primary-200);
    border-radius: 2px;
    min-width: 0;
}

.designer-layout-preset:hover .designer-layout-preset__col {
    background: var(--agt-color-primary-400);
}
```

Verwijder de oude `.designer-quick-layout` CSS en de bijbehorende `<select>` + knop markup.

### Verificatie
- Onder de canvas: 5 visuele kaartjes met layout-thumbnails.
- Klik op "2 kolommen" → rij met 2 kolommen verschijnt direct.
- Hover → kaartje licht op, kolom-preview wordt prominenter.
- Geen native `<select>` meer zichtbaar.

---

## Fase 5 — Banners consolideren en route-preview verplaatsen

### Doel
Reduceer verticale ruimte-verspilling door banners samen te voegen en de route-preview te verplaatsen.

### Wijzigingen

**Stap 1: Consolideer banners in één statusregel.**

Vervang de drie aparte banner-blokken (recovery choice, offline warning, recovered draft) door één conditionele statusbar:

```razor
@{
    var statusMessage = GetStatusMessage();
}
@if (statusMessage is not null)
{
    <div class="designer-status-bar" role="status" aria-live="polite">
        <RadzenIcon Icon="@statusMessage.Icon" />
        <span>@statusMessage.Text</span>
        @if (statusMessage.Actions is not null)
        {
            <div class="designer-status-bar__actions">
                @statusMessage.Actions
            </div>
        }
        @if (statusMessage.Dismissable)
        {
            <button type="button" class="designer-status-bar__dismiss" @onclick="DismissStatusBar">&times;</button>
        }
    </div>
}
```

Voeg de helper toe:

```csharp
private record StatusBarMessage(string Icon, string Text, RenderFragment? Actions, bool Dismissable);

private StatusBarMessage? GetStatusMessage()
{
    if (_showDraftRecoveryChoice)
        return new("history", "Lokale conceptversie gevonden.", RenderDraftRecoveryActions(), false);
    if (!string.IsNullOrWhiteSpace(_offlineWarning))
        return new("cloud_off", _offlineWarning, null, true);
    if (_hasRecoveredDraft)
        return new("restore", "Hersteld werk gevonden — sla op om te bewaren.", null, true);
    return null;
}
```

**CSS:**

```css
.designer-status-bar {
    align-items: center;
    background: var(--agt-alpha-primary-5);
    border-bottom: 1px solid var(--agt-input-border);
    color: var(--agt-text-body);
    display: flex;
    font-size: var(--agt-font-size-sm);
    gap: var(--agt-spacing-2);
    padding: var(--agt-spacing-1) var(--agt-spacing-3);
}

.designer-status-bar__actions {
    display: flex;
    gap: var(--agt-spacing-1);
    margin-left: auto;
}

.designer-status-bar__dismiss {
    background: transparent;
    border: 0;
    color: var(--agt-text-muted);
    cursor: pointer;
    font-size: 1.2rem;
    margin-left: var(--agt-spacing-2);
    padding: 0 0.3rem;
}
```

**Stap 2: Verplaats route-preview naar pagina-tab tooltip.**

Verwijder de aparte `<div class="designer-route-preview">` regel. Toon route-info als `title` attribuut op de pagina-tab:

```razor
<button type="button" class="designer-page-tab__button" @onclick="() => SelectPage(pageIndex)" title="@page.Route">
    @GetPageLabel(page)
</button>
```

Verwijder de `.designer-route-preview` CSS-regel.

### Verificatie
- Maximaal één statusbar zichtbaar (niet drie gestapelde banners).
- Route-preview is niet meer als aparte regel zichtbaar.
- Hover op een pagina-tab → tooltip toont de route.

---

## Fase 6 — Canvas dropzones als insertion-lines (Webflow-model)

### Doel
Vervang de blok-dropzones door **dunne horizontale lijnen** die verschijnen tussen componenten wanneer er gedragged wordt. Dit is het Webflow-patroon en voelt visueel veel lichter.

### Wijzigingen

**CSS — maak dropzones onzichtbaar tot er gedragged wordt:**

```css
/* Basis dropzone: onzichtbaar, maar clickable */
.designer-dropzone,
.designer-root-dropzone {
    height: 0;
    overflow: visible;
    padding: 0;
    position: relative;
    transition: height 120ms ease;
}

/* Tijdens drag: toon als insertion-line */
.designer-page--drag-in-transit .designer-dropzone,
.designer-page--drag-in-transit .designer-root-dropzone {
    height: 4px;
    margin: var(--agt-spacing-1) 0;
}

/* Active dropzone: blauwe lijn */
.designer-dropzone--active,
.designer-root-dropzone--active {
    background: var(--agt-color-primary-500);
    border-radius: 2px;
    height: 4px !important;
}

/* "+" indicator op actieve dropzone */
.designer-dropzone--active::after,
.designer-root-dropzone--active::after {
    align-items: center;
    background: var(--agt-color-primary-500);
    border-radius: 50%;
    color: white;
    content: "+";
    display: flex;
    font-size: 12px;
    font-weight: 700;
    height: 16px;
    justify-content: center;
    left: 50%;
    position: absolute;
    top: 50%;
    transform: translate(-50%, -50%);
    width: 16px;
}
```

Verwijder de oude dropzone-animatie (`dropzone-pulse`) en de brede dropzone-styling die het voelde als extra ruimte tussen componenten.

### Verificatie
- Geen drag actief → dropzones zijn onzichtbaar (0px hoog).
- Begin met slepen → dunne ruimtes verschijnen tussen alle componenten.
- Hover over een dropzone → blauwe lijn met "+" icoon.
- Drop → component verschijnt, lijn verdwijnt.

---

## Fase 7 — Toolbar polish

### Doel
De toolbar is functioneel maar kan visueel scherper. Kleine aanpassingen voor een professioneler gevoel.

### Wijzigingen

**Stap 1: Groepeer toolbar-items visueel.**

Voeg subtiele verticale scheiders toe tussen groepen:

```css
.designer-toolbar__group {
    align-items: center;
    display: flex;
    gap: var(--agt-spacing-1);
}

/* Verticale scheider tussen groepen */
.designer-toolbar__group + .designer-toolbar__group::before {
    background: var(--agt-input-border);
    content: "";
    height: 1.2rem;
    margin: 0 var(--agt-spacing-2);
    width: 1px;
}
```

**Stap 2: Maak de Instellingen-menu nuttiger.**

Voeg thema-selectie toe aan het Instellingen-menu (verplaatst uit property panel pagina-eigenschappen):

```razor
@if (_settingsMenuOpen)
{
    <div class="designer-menu" role="menu" aria-label="Instellingen">
        <div class="designer-menu__row">
            <label>Thema</label>
            <RadzenDropDown TValue="string" Data="CanvasThemeOptions" Value="@_canvasTheme" Change="OnCanvasThemeChanged" />
        </div>
        <hr class="designer-menu__divider" />
        <button type="button" class="designer-menu__item" role="menuitem" @onclick="ToggleShortcutsOverlay">Sneltoetsen (Ctrl+/)</button>
        <button type="button" class="designer-menu__item" role="menuitem" @onclick="OpenDesignerCommandPaletteAsync">Zoeken (Ctrl+K)</button>
    </div>
}
```

**CSS voor menu-divider:**

```css
.designer-menu__divider {
    border: 0;
    border-top: 1px solid var(--agt-input-border);
    margin: var(--agt-spacing-1) 0;
}
```

### Verificatie
- Toolbar-groepen gescheiden door subtiele verticale lijnen.
- Instellingen-menu toont thema-selectie bovenaan.

---

## Fase 8 — Empty canvas onboarding verbeteren

### Doel
De huidige onboarding is functioneel maar niet inspirerend. Maak het visueel aantrekkelijker met een grotere, meer uitnodigende layout.

### Wijzigingen

Verbeter de onboarding-tekst en voeg visuele preview-thumbnails toe aan de template-knoppen:

```razor
@if (ActivePage.Nodes.Count == 0)
{
    <div class="designer-canvas-empty">
        <div class="designer-canvas-empty__hero">
            <RadzenIcon Icon="dashboard_customize" />
            <h2>Ontwerp je pagina</h2>
            <p>Kies een startpunt of sleep componenten uit het palet links.</p>
        </div>
        <div class="designer-canvas-empty__templates">
            <button type="button" class="designer-canvas-empty__template" @onclick='() => OnTemplateStartSelected(DesignDocumentTemplateKind.FormPage)'>
                <div class="designer-canvas-empty__template-preview">
                    <div class="designer-canvas-empty__template-line" style="width:60%;"></div>
                    <div class="designer-canvas-empty__template-line" style="width:100%;"></div>
                    <div class="designer-canvas-empty__template-line" style="width:100%;"></div>
                    <div class="designer-canvas-empty__template-line designer-canvas-empty__template-line--button" style="width:30%;"></div>
                </div>
                <span>Formulier</span>
            </button>
            <button type="button" class="designer-canvas-empty__template" @onclick='() => OnTemplateStartSelected(DesignDocumentTemplateKind.Dashboard)'>
                <div class="designer-canvas-empty__template-preview">
                    <div class="designer-canvas-empty__template-row">
                        <div class="designer-canvas-empty__template-card"></div>
                        <div class="designer-canvas-empty__template-card"></div>
                        <div class="designer-canvas-empty__template-card"></div>
                    </div>
                    <div class="designer-canvas-empty__template-chart"></div>
                </div>
                <span>Dashboard</span>
            </button>
            <button type="button" class="designer-canvas-empty__template" @onclick='() => OnTemplateStartSelected(DesignDocumentTemplateKind.Blank)'>
                <div class="designer-canvas-empty__template-preview designer-canvas-empty__template-preview--blank">
                    <RadzenIcon Icon="add" />
                </div>
                <span>Leeg</span>
            </button>
        </div>
        <p class="designer-canvas-empty__hint">Of druk <kbd>Ctrl+K</kbd> om een component te zoeken</p>
    </div>
}
```

**CSS:**

```css
.designer-canvas-empty {
    align-items: center;
    display: flex;
    flex-direction: column;
    gap: var(--agt-spacing-4);
    justify-content: center;
    min-height: 50vh;
    padding: var(--agt-spacing-6);
    text-align: center;
}

.designer-canvas-empty__hero {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--agt-spacing-2);
}

.designer-canvas-empty__hero .rzi {
    color: var(--agt-color-primary-300);
    font-size: 3rem;
}

.designer-canvas-empty__hero h2 {
    color: var(--agt-text-heading);
    margin: 0;
}

.designer-canvas-empty__hero p {
    color: var(--agt-text-muted);
    margin: 0;
    max-width: 24rem;
}

.designer-canvas-empty__templates {
    display: flex;
    gap: var(--agt-spacing-3);
}

.designer-canvas-empty__template {
    background: var(--agt-surface-0);
    border: 1px solid var(--agt-input-border);
    border-radius: var(--agt-border-radius-md);
    color: var(--agt-text-body);
    cursor: pointer;
    display: flex;
    flex-direction: column;
    font: inherit;
    gap: var(--agt-spacing-2);
    padding: var(--agt-spacing-3);
    transition: border-color 120ms, box-shadow 120ms;
    width: 10rem;
}

.designer-canvas-empty__template:hover {
    border-color: var(--agt-color-primary-400);
    box-shadow: 0 4px 16px var(--agt-shadow-1);
}

.designer-canvas-empty__template-preview {
    background: var(--agt-surface-2);
    border-radius: var(--agt-border-radius-sm);
    display: flex;
    flex-direction: column;
    gap: 4px;
    height: 5rem;
    padding: var(--agt-spacing-2);
}

.designer-canvas-empty__template-line {
    background: var(--agt-color-primary-100);
    border-radius: 2px;
    height: 6px;
}

.designer-canvas-empty__template-line--button {
    background: var(--agt-color-primary-400);
    margin-top: auto;
}

.designer-canvas-empty__template-row {
    display: flex;
    gap: 4px;
}

.designer-canvas-empty__template-card {
    background: var(--agt-color-primary-100);
    border-radius: 2px;
    flex: 1;
    height: 1.5rem;
}

.designer-canvas-empty__template-chart {
    background: var(--agt-color-primary-100);
    border-radius: 2px;
    flex: 1;
}

.designer-canvas-empty__template-preview--blank {
    align-items: center;
    justify-content: center;
}

.designer-canvas-empty__template-preview--blank .rzi {
    color: var(--agt-text-muted);
    font-size: 1.5rem;
}

.designer-canvas-empty__hint {
    color: var(--agt-text-muted);
    font-size: var(--agt-font-size-xs);
    margin: 0;
}

.designer-canvas-empty__hint kbd {
    background: var(--agt-surface-2);
    border: 1px solid var(--agt-input-border);
    border-radius: 3px;
    font-family: inherit;
    font-size: var(--agt-font-size-xs);
    padding: 0.1rem 0.4rem;
}
```

### Verificatie
- Lege canvas toont drie template-kaarten met visuele mini-previews.
- Hover → kaart licht op met subtiele schaduw.
- Klik → template wordt geladen op canvas.
- "Of druk Ctrl+K" hint onderaan.

---

## Samenvatting wijzigingen per bestand

| Bestand | Fasen |
|---------|-------|
| `DesignerShell.razor` | 2 (panel-consolidatie), 3 (palette grid), 4 (layout presets), 5 (banners/route), 7 (toolbar), 8 (onboarding) |
| `DesignerShell.razor.cs` | 2 (enums, tab state), 4 (AddRowLayoutPreset), 5 (GetStatusMessage) |
| `designer.css` | 1 (WYSIWYG canvas), 2 (panel tabs), 3 (palette grid), 4 (layout presets), 5 (status bar), 6 (insertion lines), 7 (toolbar), 8 (onboarding) |

## Volgorde van uitvoering

1. **Fase 1** (Canvas WYSIWYG) — visueel meest impactvol, puur CSS
2. **Fase 6** (Insertion lines) — bouwt voort op fase 1, puur CSS
3. **Fase 2** (Panel-consolidatie) — grootste refactor, onafhankelijk van canvas
4. **Fase 3** (Palette grid) — onderdeel van nieuw linkerpanel
5. **Fase 4** (Layout presets) — vervangt bestaand element
6. **Fase 5** (Banners + route) — kleine cleanup
7. **Fase 7** (Toolbar polish) — cosmetisch
8. **Fase 8** (Onboarding) — vervangt bestaand element

Elke fase is een eigen commit.

## Verificatie — integratietest

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **Visuele checklist:**

| # | Test | Verwacht |
|---|------|---------|
| 1 | Canvas in rust — geen componenten geselecteerd | Componenten zichtbaar ZONDER borders/bars, zoals een echte pagina |
| 2 | Hover over component | Blauwe outline + label verschijnt boven component |
| 3 | Selecteer component | Prominente blauwe outline + floating toolbar |
| 4 | Links panel → Palet tab | 2-kolom grid van component-kaartjes |
| 5 | Links panel → Navigator tab | Structuurboom |
| 6 | Rechts panel → Data tab | Data-panel inhoud |
| 7 | Sleep component uit palet | Insertion lines verschijnen tussen canvas-items |
| 8 | Klik op "2 kolommen" layout-preset | Rij met 2 kolommen verschijnt |
| 9 | Lege canvas | Drie template-kaarten met visuele previews |
| 10 | Status: offline | Eén compacte statusbar, niet drie banners |
| 11 | Toolbar | Groepen gescheiden door verticale lijnen |
| 12 | Instellingen-menu | Thema-dropdown + Sneltoetsen + Zoeken |
