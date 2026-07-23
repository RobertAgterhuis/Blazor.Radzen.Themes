# Prompt 78c — Rijke templates, navigatie-component, en multi-screen

Afhankelijk van: prompt 78a (layout containment — nodig voor SidebarApp template), prompt 78b (live data — templates profiteren van seeded data op canvas).

De huidige 6 templates zijn skeletachtig: 2-3 lege form fields per template. Het Dashboard toont letterlijk "Toon hier KPI's of grafieken." Er is geen navigatie-component voor multi-page apps. Deze prompt voegt rijke templates, een navigatie-component, en multi-screen ondersteuning toe.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — AgtNavLink navigatie-component

### Doel
Een simpel navigatie-link component dat in de palette verschijnt en in de sidebar-slot van AgtSidebarLayout kan worden geplaatst.

### Implementatie

**Stap 1: Maak het component.**

`src/Agterhuis.Ui/Components/Navigation/AgtNavLink.razor`:

```razor
<a class="agt-nav-link @(IsActive ? "agt-nav-link--active" : string.Empty) @CssClass"
   href="@Href"
   @onclick="OnClicked"
   @onclick:preventDefault="true">
    @if (!string.IsNullOrEmpty(Icon))
    {
        <RadzenIcon Icon="@Icon" />
    }
    <span class="agt-nav-link__text">@Text</span>
</a>

@code {
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public string Href { get; set; } = "#";
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public EventCallback<string> OnNavigate { get; set; }

    private async Task OnClicked()
    {
        await OnNavigate.InvokeAsync(Href);
    }
}
```

**Stap 2: Voeg CSS toe.**

`src/Agterhuis.Ui/Components/Navigation/AgtNavLink.razor.css`:

```css
.agt-nav-link {
    align-items: center;
    border-radius: var(--agt-border-radius-sm);
    color: var(--agt-text-body);
    cursor: pointer;
    display: flex;
    gap: var(--agt-spacing-2);
    padding: var(--agt-spacing-2) var(--agt-spacing-3);
    text-decoration: none;
    transition: background 120ms, color 120ms;
}

.agt-nav-link:hover {
    background: var(--agt-alpha-primary-10);
}

.agt-nav-link--active {
    background: var(--agt-alpha-primary-15);
    color: var(--agt-color-primary-500);
    font-weight: 600;
}

.agt-nav-link__text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

**Stap 3: Registreer in de component-registry.**

Voeg `AgtNavLink` toe aan `DesignerComponentRegistry.g.cs` met:
- Category: "Navigatie"
- Icon: "link"
- DisplayName: "Navigatielink"
- DesignerDisplayName: "Navigatielink"
- Parameters: Text (string), Href (string), Icon (string?), IsActive (bool)
- Geen slots (geen ChildContent)

---

## Fase 2 — Verrijk bestaande templates

### Doel
De 6 bestaande templates moeten rijker worden: meer componenten, correcte layout, domein-specifieke labels.

### Implementatie

Herschrijf de template-builders in `DesignDocumentTemplates.cs`. Elke template moet minstens 8-12 componenten bevatten en realistische structuur hebben.

**Dashboard (vervang `BuildDashboard`):**
- `AgtPageHeader`: titel "Schadedossier Dashboard", beschrijving "Overzicht van lopende dossiers en werkorders."
- `RadzenRow` met 4× `RadzenColumn` Size=3, elk een `AgtCard` met:
  - Kolom 1: AgtPageHeader-achtig met titel "Nieuwe dossiers" (simuleer KPI)
  - Kolom 2: titel "In behandeling"
  - Kolom 3: titel "Gereed"
  - Kolom 4: titel "Gefactureerd"
- `RadzenRow` met `RadzenColumn` Size=8: een `AgtCard` met een `AgtPageHeader` titel "Recente dossiers" + een `AgtEmptyState` met "Data wordt geladen..."
- `RadzenColumn` Size=4: een `AgtCard` met titel "Status verdeling" + `AgtEmptyState`

**FormPage (vervang `BuildFormPage`):**
- `AgtPageHeader`: "Nieuw schadedossier", "Vul de gegevens in voor een nieuw dossier."
- `AgtCard` met:
  - `RadzenRow` + 2 kolommen: `AgtTextField` Label="Dossiernummer" + `AgtDatePicker` Label="Schadedatum"
  - `RadzenRow` + 2 kolommen: `AgtDropdown` Label="Schadesoort" + `AgtDropdown` Label="Glastype"
  - `RadzenRow` + 2 kolommen: `AgtDropdown` Label="Actie" + `AgtSwitch` Label="Voorexpertise nodig"
  - `AgtTextArea` Label="Opmerkingen"
  - `AgtFormActions` SaveText="Opslaan" CancelText="Annuleren"

**ListCrud (vervang `BuildListCrud`):**
- `AgtPageHeader`: "Schadedossiers", "Zoek, filter en beheer dossiers."
- `RadzenRow` + 3 kolommen: `AgtTextField` Label="Zoeken" + `AgtDropdown` Label="Status" + `AgtDropdown` Label="Glastype"
- `AgtCard` met `AgtEmptyState` Icon="search" Title="Geen resultaten" Description="Pas de filters aan of maak een nieuw dossier."

**MasterDetail, Wizard**: vergelijkbare verrijking met domein-specifieke velden en layout.

---

## Fase 3 — Nieuwe template-soorten

### Doel
Voeg nieuwe templates toe die veelgebruikte app-patronen dekken.

### Implementatie

**Stap 1: Breid `DesignDocumentTemplateKind` uit.**

```csharp
public enum DesignDocumentTemplateKind
{
    Blank,
    FormPage,
    ListCrud,
    MasterDetail,
    Wizard,
    Dashboard,
    // Nieuw:
    SidebarApp,
    SettingsPage,
    TableWithFilters
}
```

**Stap 2: Implementeer de nieuwe templates.**

**SidebarApp** — het visitekaartje van de designer. Multi-page met sidebar navigatie:
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
            CreateBasePage("/dossiers", "Dossiers", CreateSidebarListNodes()),
            CreateBasePage("/dossier/nieuw", "Nieuw dossier", CreateSidebarFormNodes()),
            CreateBasePage("/instellingen", "Instellingen", CreateSidebarSettingsNodes())
        ]
    };
```

Elke pagina gebruikt `AgtSidebarLayout` als root met:
- Logo slot: `AgtPageHeader` met bedrijfsnaam
- Sidebar slot: 4× `AgtNavLink` naar elke pagina
- ChildContent slot: pagina-specifieke inhoud (hergebruik Dashboard/ListCrud/FormPage nodes)

**SettingsPage:**
- `AgtPageHeader`: "Instellingen"
- 3 `AgtCard` secties: "Profiel" (2 textfields), "Notificaties" (3 switches), "Weergave" (1 dropdown + 1 switch)
- `AgtFormActions`

**TableWithFilters:**
- `AgtPageHeader`: "Voertuigen"
- Filterrij: 4 velden (Merk, Bouwjaar, Kleur, ADAS)
- `AgtCard` met `AgtEmptyState` als placeholder voor tabel

**Stap 3: Voeg iconen en beschrijvingen toe in `DesignerStartScreen.razor`.**

Update de `GetIcon`, `GetTitle`, en `GetDescription` methods met de nieuwe template-soorten:
```csharp
DesignDocumentTemplateKind.SidebarApp => "web",
DesignDocumentTemplateKind.SettingsPage => "settings",
DesignDocumentTemplateKind.TableWithFilters => "filter_list",
```

---

## Fase 4 — Multi-screen navigatie in preview

### Doel
In preview-mode moet klikken op een `AgtNavLink` navigeren naar de bijbehorende pagina.

### Implementatie

In `DesignerShell.razor.cs`, voeg een methode toe die als cascading parameter beschikbaar wordt:

```csharp
private void OnPreviewNavigate(string route)
{
    if (!_previewMode) return;

    var targetIndex = _commands.Document.Pages
        .Select((page, index) => (page, index))
        .FirstOrDefault(item => string.Equals(item.page.Route, route, StringComparison.OrdinalIgnoreCase))
        .index;

    if (targetIndex >= 0 && targetIndex < _commands.Document.Pages.Count)
    {
        _activePageIndex = targetIndex;
        StateHasChanged();
    }
}
```

Cascade dit als een `Action<string>` zodat `AgtNavLink` het kan aanroepen wanneer het in preview-context rendert. De exacte mechanisme (CascadingValue vs. EventCallback doorvoeren) hangt af van hoe diep `AgtNavLink` genest is.

**Alternatief (eenvoudiger):** voeg pagina-tabs toe aan de preview-modus zodat de gebruiker handmatig kan wisselen. De tabs bestaan al — ze zijn alleen verborgen in preview. Maak ze zichtbaar.

### Voeg "Pagina vanuit template" toe

Bij de "+" knop voor pagina's, toon een keuzemenu:

```razor
<div class="designer-page-add-menu">
    <button @onclick="() => OnAddPageAsync()">Lege pagina</button>
    <button @onclick="() => OnAddPageFromTemplateAsync(DesignDocumentTemplateKind.FormPage)">Formulierpagina</button>
    <button @onclick="() => OnAddPageFromTemplateAsync(DesignDocumentTemplateKind.ListCrud)">Overzichtstabel</button>
    <button @onclick="() => OnAddPageFromTemplateAsync(DesignDocumentTemplateKind.SettingsPage)">Instellingen</button>
</div>
```

---

## Samenvatting wijzigingen per bestand

| Bestand | Fase | Wijziging |
|---------|------|-----------|
| `Components/Navigation/AgtNavLink.razor` | 1 | NIEUW |
| `Components/Navigation/AgtNavLink.razor.css` | 1 | NIEUW |
| `Registry/DesignerComponentRegistry.g.cs` | 1 | AgtNavLink registratie |
| `Model/DesignDocumentTemplates.cs` | 2, 3 | Verrijkte + nieuwe templates |
| `Model/DesignDocumentTemplateKind.cs` | 3 | 3 nieuwe enum-waarden |
| `Components/DesignerStartScreen.razor` | 3 | Iconen/titels voor nieuwe templates |
| `Components/DesignerShell.razor.cs` | 4 | `OnPreviewNavigate`, `OnAddPageFromTemplateAsync` |
| `Components/DesignerShell.razor` | 4 | Pagina-add menu, preview navigatie |

## Verificatie — integratietest

| # | Actie | Verwacht |
|---|-------|---------|
| 1 | Open startscherm | 9 templates zichtbaar met iconen |
| 2 | Kies "Sidebar App" | 4 pagina's, sidebar met navigatielinks |
| 3 | Kies "Dashboard" | KPI-kaarten + data-sectie, niet "Toon hier KPI's" |
| 4 | Kies "Formulierpagina" | 6+ velden met domein-labels |
| 5 | AgtNavLink in palette | Verschijnt onder categorie "Navigatie" |
| 6 | Sleep AgtNavLink naar canvas | Toont link met tekst en icoon |
| 7 | Preview: klik navigatielink | Navigeert naar de gekoppelde pagina |
| 8 | Klik "+" bij pagina-tabs | Menu met template-opties |
