# Prompt 67 — Fix dode UI-controls in designer

Na uitvoering van prompts 65 en 66 zijn meerdere UI-controls in de designer defect: menu's sluiten niet, thema wisselt niet, en diverse interactiepatronen missen event-handling. Deze prompt repareert elke defecte control met de exacte root cause en fix.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Menu's sluiten niet (click-outside ontbreekt)

### Root cause

De custom menu's (Bestand, Instellingen, pagina-context-menu) worden getoond via booleans (`_fileMenuOpen`, `_settingsMenuOpen`, `_pageMenuIndex`). Deze togglen alleen bij klikken op de menu-knop zelf. Er is **geen** click-outside handler: zodra een menu open is, kan het alleen sluiten door opnieuw op dezelfde knop te klikken. Dit is onontdekt gebleven omdat Radzen's eigen `RadzenDropDown` (binnenin de menu's) zijn eigen popup-overlay beheert maar de parent menu open laat staan.

### Betrokken bestanden

- `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor` — menu-markup (regels ~64-82, ~113-126, ~157-169)
- `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs` — `ToggleFileMenu()`, `ToggleSettingsMenu()`, `TogglePageMenu()` (regels ~1052-1073)
- `src/Agterhuis.Ui.Designer/wwwroot/designer-interop.js` — nieuw: click-outside listener
- `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css` — backdrop styling

### Fix

**Stap 1: Voeg een invisible backdrop toe aan DesignerShell.razor.**

Wanneer **enig** menu open is (`_fileMenuOpen || _settingsMenuOpen || _pageMenuIndex is not null`), render een transparante full-screen `<div>` met `position: fixed; inset: 0; z-index: 89` (net onder de menu z-index van 90) die bij klik `CloseAllMenus()` aanroept:

```razor
@if (_fileMenuOpen || _settingsMenuOpen || _pageMenuIndex is not null)
{
    <div class="designer-menu-backdrop" @onclick="CloseAllMenus"></div>
}
```

**Stap 2: Voeg `CloseAllMenus()` toe aan DesignerShell.razor.cs:**

```csharp
private void CloseAllMenus()
{
    _fileMenuOpen = false;
    _settingsMenuOpen = false;
    _pageMenuIndex = null;
}
```

**Stap 3: Sluit menu's automatisch na actie.** Elke menu-item `@onclick` handler die een actie uitvoert (Nieuw, Opslaan, Exporteren, etc.) moet ook `CloseAllMenus()` aanroepen. Voeg aan het **begin** van deze methoden `CloseAllMenus();` toe:

- `OpenNewDocumentDialog()`
- `OnOpenDocument()`
- `OnSaveDocument()`
- `OpenVersionHistoryAsync()`
- `OnExportDocument()`
- Pagina-menu items: `BeginRenamePage()`, `DuplicatePageAsync()`, `RemovePageAsync()`

**Stap 4: CSS voor de backdrop in `designer.css`:**

```css
.designer-menu-backdrop {
    position: fixed;
    inset: 0;
    z-index: 89;
    background: transparent;
    cursor: default;
}
```

**Stap 5: Sluit menus bij Escape-toets.** In `OnPageKeyDown`, voeg toe:

```csharp
if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
{
    if (_fileMenuOpen || _settingsMenuOpen || _pageMenuIndex is not null)
    {
        CloseAllMenus();
        await InvokeAsync(StateHasChanged);
        return;
    }
}
```

## Fase 2 — Thema wisselt niet

### Root cause

`OnCanvasThemeChanged(string value)` (regel ~856) zet `_canvasTheme` correct en roept `CanvasThemeChanged.InvokeAsync(_canvasTheme)` aan. Maar in `Designer.razor` (de host-pagina) is `CanvasThemeChanged` **niet gebonden**:

```razor
<DesignerShell Store="@DesignStore"
               Registry="@Registry"
               DefaultCanvasTheme="plum-dark" />
```

Wanneer een `EventCallback` niet gebonden is, retourneert `InvokeAsync` direct `Task.CompletedTask` zonder re-render te triggeren. Het gevolg: `_canvasTheme` wordt intern bijgewerkt maar de UI herrendert niet, dus `data-agt-theme="@_canvasTheme"` op de canvas-div verandert niet visueel.

### Fix

**Optie A (minimaal, aanbevolen):** Voeg een expliciete `StateHasChanged()` toe aan de handler. Wijzig `OnCanvasThemeChanged(string value)` in `DesignerShell.razor.cs`:

```csharp
private async Task OnCanvasThemeChanged(string value)
{
    _canvasTheme = string.IsNullOrWhiteSpace(value) ? "plum-dark" : value;
    await CanvasThemeChanged.InvokeAsync(_canvasTheme);
    await InvokeAsync(StateHasChanged);
}
```

**Optie B (extra):** Sluit ook het instellingen-menu na thema-selectie zodat de gebruiker het resultaat meteen ziet. Voeg `CloseAllMenus();` toe als eerste regel in de methode.

Pas ook de `object` overload aan:

```csharp
private Task OnCanvasThemeChanged(object value) => OnCanvasThemeChanged(value?.ToString() ?? "plum-dark");
```

(Deze is ongewijzigd, maar de `string` overload is nu `async Task` dus de chaining werkt correct.)

### Bewijs

De canvas-div op regel ~240 van `DesignerShell.razor`:
```razor
<div class="designer-canvas" data-agt-theme="@_canvasTheme" @ref="_canvasRef">
```
Dit attribuut stuurt de hele thema-CSS. Zonder re-render verandert het niet.

## Fase 3 — RadzenDropDown in menu-context: selectie-events

### Root cause

De `RadzenDropDown` voor het thema (regel ~119) en voor opgeslagen documenten (regel ~76) zit in een custom menu (`<div class="designer-menu">`). RadzenDropDown maakt zijn eigen popup overlay aan (Radzen's popup service). Wanneer de gebruiker een optie selecteert:
1. Radzen's popup sluit (correct)
2. Het `Change` event vuurt (correct)
3. Maar de parent `designer-menu` **blijft open** (geen auto-close)

Dit geeft het gevoel dat "er niets gebeurt" — de gebruiker ziet nog steeds het menu.

### Fix

In de `Change` handler van beide dropdowns, sluit het parent menu:

**Thema-dropdown (Instellingen-menu):** Al opgelost via Fase 2 optie B — `CloseAllMenus()` in `OnCanvasThemeChanged`.

**Opgeslagen-documenten dropdown (Bestand-menu):** Voeg `CloseAllMenus();` toe aan `OnSavedSelectionChanged`:

```csharp
private async Task OnSavedSelectionChanged(object value)
{
    CloseAllMenus(); // Sluit het Bestand-menu
    var selected = value?.ToString();
    if (string.IsNullOrWhiteSpace(selected))
    {
        return;
    }
    // ... rest van de bestaande code
}
```

## Fase 4 — TogglePreviewMode mist StateHasChanged

### Root cause

`TogglePreviewMode()` (regel ~481) is `void` en roept geen `StateHasChanged()` aan:

```csharp
private void TogglePreviewMode()
{
    _previewMode = !_previewMode;
    _liveAnnouncement = _previewMode ? "Preview modus actief." : "Bewerkmodus actief.";
}
```

Wanneer aangeroepen via de `RadzenButton.Click` EventCallback, triggert Blazor automatisch een re-render. Maar wanneer aangeroepen vanuit `OnPageKeyDown` (Ctrl+P), wordt `StateHasChanged` al expliciet aangeroepen (regel ~1103-1104). Dit lijkt correct te werken, maar voor robuustheid:

### Fix

Maak de methode consistent en expliciet:

```csharp
private void TogglePreviewMode()
{
    _previewMode = !_previewMode;
    _liveAnnouncement = _previewMode ? "Preview modus actief." : "Bewerkmodus actief.";
    CloseAllMenus();
    StateHasChanged();
}
```

## Fase 5 — "Voeg rij toe" select mist value-binding

### Root cause

De `<select>` (regel ~280) mist een `value`-binding:

```razor
<select id="designer-layout-split" @onchange="OnRowLayoutChanged">
```

Dit werkt bij eerste gebruik (default "12"), maar na een `StateHasChanged` reset de `<select>` visueel naar de eerste optie terwijl `_selectedRowLayout` een andere waarde kan hebben.

### Fix

Voeg `value="@_selectedRowLayout"` toe:

```razor
<select id="designer-layout-split" value="@_selectedRowLayout" @onchange="OnRowLayoutChanged">
```

### Extra controle

Verifieer dat `AddRowLayoutAsync()` daadwerkelijk een rij toevoegt. Test door:
1. Open de designer
2. Selecteer "2 kolommen (6+6)" in de select
3. Klik "Rij toevoegen"
4. Controleer of een `RadzenRow` met twee `RadzenColumn` nodes (Size 6 elk) verschijnt op de canvas

Als het visueel niet verschijnt maar de nodes WEL in het document zitten, is het een CSS-probleem in de canvas-node rendering.

## Fase 6 — Palette click-to-add: flashNode JS interop

### Root cause

`OnPaletteItemClickedAsync` (regel ~361) roept `JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId)` aan. De `flashNode` functie in `designer-interop.js` (regel ~313) zoekt een DOM-element op basis van `nodeId`. Als het nieuwe component nog niet gerenderd is op het moment van de JS-aanroep (Blazor heeft `StateHasChanged` nog niet verwerkt), vindt `flashNode` niets.

### Fix

Verplaats de `flashNode` aanroep naar NA de StateHasChanged:

```csharp
private async Task OnPaletteItemClickedAsync(string componentType)
{
    var location = ResolvePaletteClickInsertLocation();
    if (!AddFromPalette(location, componentType))
    {
        return;
    }

    _uiFeedback = "Component toegevoegd.";
    _liveAnnouncement = "Component toegevoegd via klik.";
    await AutoSaveAsync();
    await InvokeAsync(StateHasChanged);
    // Flash NA render zodat het DOM-element bestaat:
    await JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId);
}
```

Doe hetzelfde voor `OnDropRequested` (regel ~300) en `OnInlineAddRequested` (regel ~1990) — verplaats de `flashNode` call na `StateHasChanged`.

## Fase 7 — Panel-resize is volledig kapot

### Root cause

`setupResizablePanels` wordt aangeroepen in `OnInitializedAsync` (regel ~212 van `DesignerShell.razor.cs`). In Blazor WebAssembly draait `OnInitializedAsync` **vóór** de eerste render — de DOM-elementen bestaan nog niet. `document.querySelector('.designer-page')` retourneert `null`, de functie stopt met `if (!designerLayout) return;`, en de dividers krijgen **nooit** hun `mousedown` event listeners. Panel-resize werkt dus helemaal niet.

Daarnaast: de dividers voor palette en code-panel zijn conditioneel gerenderd (`@if (!_paletteCollapsed)` en `@if (!_codeCollapsed)`). Als een panel wordt in-/uitgeklapt, worden de divider DOM-elementen verwijderd en opnieuw aangemaakt door Blazor. Zelfs als de initialisatie op het juiste moment zou draaien, verliest de nieuwe divider zijn event listener.

Een derde probleem: de resize-richting voor het property-panel is geïnverteerd. De formule `newSize = startSize + delta` (waar delta = clientX verschuiving) werkt correct voor het palette (links), maar voor het property-panel (rechts) moet het OMGEKEERD: slepen naar links moet het panel groter maken.

### Betrokken bestanden

- `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs` — `OnInitializedAsync` (regel ~212)
- `src/Agterhuis.Ui.Designer/wwwroot/designer-resize-interop.js` — complete herschrijving nodig
- `src/Agterhuis.Ui.Designer/wwwroot/designer-interop.js` — `setupResizablePanels` wrapper

### Fix

**Stap 1: Verplaats `setupResizablePanels` naar `OnAfterRenderAsync`.**

Voeg een `OnAfterRenderAsync` override toe (of voeg toe aan de bestaande als die er is):

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JS.InvokeVoidAsync("designerInterop.setupResizablePanels");
    }
}
```

Verwijder de `setupResizablePanels` aanroep uit `OnInitializedAsync`.

**Stap 2: Gebruik event delegation in plaats van directe event listeners.**

Herschrijf `setupResizablePanels` in `designer-resize-interop.js` om event delegation te gebruiken op `.designer-page` in plaats van individuele divider-elementen. Zo werken dividers die later door Blazor worden toegevoegd automatisch:

```javascript
export const setupResizablePanels = () => {
  const STORAGE_KEY = 'designer-panel-sizes';
  const CONFIG = {
    'palette-canvas':  { dir: 'vertical', min: 150, max: 400, def: 220, prop: 'palette-width',  invert: false },
    'canvas-property': { dir: 'vertical', min: 200, max: 500, def: 320, prop: 'property-width', invert: true },
    'canvas-code':     { dir: 'horizontal', min: 100, maxPct: 0.6, def: 250, prop: 'code-height', invert: false },
  };

  const designerLayout = document.querySelector('.designer-page');
  if (!designerLayout) return;

  // Herstel opgeslagen maten
  const loadSizes = () => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return saved ? JSON.parse(saved) : {};
    } catch { return {}; }
  };
  const saveSizes = (sizes) => {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(sizes)); } catch {}
  };

  const saved = loadSizes();
  for (const [, cfg] of Object.entries(CONFIG)) {
    const key = cfg.prop.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
    const val = saved[key] ?? cfg.def;
    designerLayout.style.setProperty('--designer-' + cfg.prop, val + 'px');
  }

  // Event delegation: luister op de parent
  designerLayout.addEventListener('mousedown', (e) => {
    const divider = e.target.closest('.designer-divider[data-divider]');
    if (!divider || e.button !== 0) return;

    const dividerType = divider.getAttribute('data-divider');
    const cfg = CONFIG[dividerType];
    if (!cfg) return;

    e.preventDefault();
    const cssVar = '--designer-' + cfg.prop;
    const isHorizontal = cfg.dir === 'horizontal';
    const startPos = isHorizontal ? e.clientY : e.clientX;
    const startSize = parseInt(designerLayout.style.getPropertyValue(cssVar)) || cfg.def;
    const maxSize = cfg.maxPct ? Math.round(window.innerHeight * cfg.maxPct) : cfg.max;

    const overlay = document.createElement('div');
    overlay.style.cssText = 'position:fixed;inset:0;z-index:10000;cursor:' +
      (isHorizontal ? 'row-resize' : 'col-resize');
    document.body.appendChild(overlay);
    divider.style.backgroundColor = 'var(--agt-color-primary-500)';

    const onMove = (me) => {
      const delta = isHorizontal ? me.clientY - startPos : me.clientX - startPos;
      const adjusted = cfg.invert ? startSize - delta : startSize + delta;
      const clamped = Math.max(cfg.min, Math.min(maxSize, adjusted));
      designerLayout.style.setProperty(cssVar, clamped + 'px');
    };

    const onUp = () => {
      overlay.remove();
      divider.style.backgroundColor = '';
      const key = cfg.prop.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
      const sizes = loadSizes();
      sizes[key] = parseInt(designerLayout.style.getPropertyValue(cssVar));
      saveSizes(sizes);
      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup', onUp);
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
  });
};
```

Belangrijkste verschil met de oude code:
1. **Event delegation** op `.designer-page` — werkt voor alle huidige en toekomstige dividers
2. **`invert: true`** voor `canvas-property` — slepen naar links maakt het property-panel groter
3. **`e.preventDefault()`** op mousedown — voorkomt tekst-selectie tijdens slepen

**Stap 3: Maak dividers visueel duidelijker.**

Voeg een hover-effect toe aan de divider CSS:

```css
.designer-divider:hover {
    background: var(--agt-color-primary-300);
}

.designer-divider:active {
    background: var(--agt-color-primary-500);
}
```

## Fase 8 — Layout-sectie misleidt niet-technische gebruikers

### Root cause

Het property panel toont een "Layout" sectie met velden "Rij", "Kolom", "Rijspan" en "Kolomspan" voor elk geselecteerd component. Deze velden wijzigen `DesignLayoutSlot` — CSS-grid positionerings-metadata voor de geëxporteerde pagina. Ze hebben **geen enkel visueel effect op de designer-canvas** omdat de canvas geen CSS-grid gebruikt voor node-positionering.

Een niet-technische gebruiker leest "Kolom: 3" als "dit component heeft 3 kolommen" en verwacht visuele feedback. Dit is het probleem dat de user rapporteert als "het canvas reageert niet op eigenschappenwijzigingen."

De daadwerkelijke manier om kolommen toe te voegen aan een `RadzenRow` is via:
- "Kolommen (handmatig)" → "Kolom toevoegen" knop (als de Row geselecteerd is)
- De "Voeg rij toe" quick-layout onderaan de canvas

### Fix

**Stap 1: Verberg de Layout-sectie in simpele modus.**

De Layout-sectie (Rij, Kolom, Rijspan, Kolomspan) is alleen relevant voor developers die met CSS-grid positionering werken. Verberg deze sectie wanneer `_simpleMode` actief is (standaard):

In `PropertyPanel.razor`, wrap de layout sectie:
```razor
@if (!_simpleMode)
{
    <section>
        <h3>Layout</h3>
        <div class="designer-properties__layout-grid">
            @* ... bestaande velden ... *@
        </div>
    </section>
}
```

**Stap 2: Hernoem labels in geavanceerde modus.**

Wijzig de labels om duidelijk te maken dat het positionering betreft, niet structuur:

| Huidig | Nieuw | Tooltip |
|--------|-------|---------|
| Rij | Grid-rij | "CSS grid rij-positie in de geëxporteerde pagina" |
| Kolom | Grid-kolom | "CSS grid kolom-positie in de geëxporteerde pagina" |
| Rijspan | Grid-rijspan | "Hoeveel grid-rijen dit component beslaat" |
| Kolomspan | Grid-kolomspan | "Hoeveel grid-kolommen dit component beslaat" |

**Stap 3: Toon "Kolom toevoegen" prominenter voor Row-nodes.**

Wanneer een `RadzenRow` geselecteerd is in simpele modus, toon bovenaan het property panel een opvallende "Kolom toevoegen" knop (in plaats van verborgen onder "Kolommen (handmatig)"):

```razor
@if (_simpleMode && SupportsColumnsSlot)
{
    <div class="designer-properties__quick-action">
        <RadzenButton Text="Kolom toevoegen" Icon="add" ButtonStyle="ButtonStyle.Primary" Variant="Variant.Flat" Click="OnAddColumnRequested" />
        @if (SplitColumnNode is not null)
        {
            <RadzenButton Text="Kolom splitsen" Icon="vertical_split" ButtonStyle="ButtonStyle.Base" Variant="Variant.Flat" Click="OnSplitColumnRequested" />
        }
    </div>
}
```

**Stap 4: Voeg visuele bevestiging toe bij property-wijzigingen.**

Na elke succesvolle property-wijziging, flash het gewijzigde component op de canvas. Voeg aan `OnNodeParameterChanged` in `DesignerShell.razor.cs` toe:

```csharp
private async Task OnNodeParameterChanged(...)
{
    if (_selectedNodeId is not null && _commands.Execute(...))
    {
        _hasRecoveredDraft = true;
        _uiFeedback = $"Eigenschap '{args.Parameter.Name}' gewijzigd.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId);
    }
}
```

Dit geeft de gebruiker directe visuele feedback dat de wijziging is verwerkt.

## Fase 9 — Overige potentieel dode controls audit

### 9a. Collapsible panels — localStorage persistence

`TogglePaletteCollapsed()`, `ToggleDataCollapsed()`, etc. roepen `PersistLayoutStateAsync()` aan. Controleer dat deze methode daadwerkelijk bestaat en correct werkt:

```csharp
private async Task PersistLayoutStateAsync()
{
    await JS.InvokeVoidAsync("designerInterop.setJson", "agt-designer-layout", new
    {
        paletteCollapsed = _paletteCollapsed,
        dataCollapsed = _dataCollapsed,
        treeCollapsed = _treeCollapsed,
        codeCollapsed = _codeCollapsed
    });
}
```

Controleer ook dat `OnAfterRenderAsync` deze state herstelt uit localStorage bij eerste render.

### 9b. Export-dialoog sluiten

Controleer dat `CloseExportDialog()` bestaat en `_showExportDialog = false` zet + `StateHasChanged` aanroept.

### 9c. New document-dialoog

Controleer dat `CloseNewDocumentDialog()` correct `_showNewDocumentDialog = false` zet en dat de dialoog-knoppen correct gebonden zijn.

### 9d. Onboarding overlay

`DismissOnboardingAsync()` (regel ~487) roept `JS.InvokeVoidAsync("designerInterop.setJson", ...)` aan. Controleer dat `setJson` gedefinieerd is in de JS (bevestigd op regel ~84 van designer-interop.js).

### 9e. Version history dialoog

Controleer dat `CloseVersionHistory()` bestaat en `_showVersionHistory = false` zet.

## Fase 10 — Verificatie

Na alle fixes:

1. `dotnet build -c Release` — zero errors, zero warnings
2. `dotnet test` — groen
3. **Handmatige test checklist:**

| # | Actie | Verwacht resultaat |
|---|-------|-------------------|
| 1 | Klik "Bestand" → klik ergens buiten het menu | Menu sluit |
| 2 | Klik "Bestand" → klik "Nieuw" | Menu sluit, dialoog opent |
| 3 | Klik "Instellingen" → selecteer een thema | Menu sluit, canvas toont nieuw thema |
| 4 | Klik "Instellingen" → druk Escape | Menu sluit |
| 5 | Selecteer "2 kolommen (6+6)" → klik "Rij toevoegen" | Nieuwe rij met 2 kolommen verschijnt op canvas |
| 6 | Klik op een component in het palet | Component verschijnt op canvas met flash-animatie |
| 7 | Klik "Preview" | Alle editor-chrome verdwijnt, alleen canvas zichtbaar |
| 8 | Klik op pagina-tab "..." menu → klik erbuiten | Menu sluit |
| 9 | Klap Palet dicht en ververs pagina | Palet blijft dicht (localStorage) |
| 10 | Open export-dialoog → klik "Annuleren" | Dialoog sluit |
| 11 | Open "Nieuw document" dialoog → klik "Annuleren" | Dialoog sluit |
| 12 | Sleep de divider tussen palet en canvas naar rechts | Palet wordt breder |
| 13 | Sleep de divider tussen canvas en properties naar links | Property-panel wordt breder |
| 14 | Klap palet in en weer uit → sleep divider | Divider werkt nog steeds (event delegation) |
| 15 | Hover over een divider | Divider kleurt subtiel op (visuele affordance) |
