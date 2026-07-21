# Prompt 59 — Designer UI-componenten extractie naar de Designer RCL

De designer-UI (palet, canvas, property panel, toolbar, structuurboom, data-panel, code-panel) zit grotendeels in `samples/Agterhuis.Ui.Demo/Components/Pages/Designer.razor` — een 1000+ regels monoliet in de demo-app. Dat maakt de designer onbruikbaar buiten de demo. Extraheer de UI naar herbruikbare Razor-componenten in de `Agterhuis.Ui.Designer`-assembly, zodat de demo-app een dunne host wordt en de designer in elke Blazor-app kan worden ingebed.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Inventariseer en plan de extractie

Analyseer `Designer.razor` en de gerelateerde bestanden in de demo-app. Identificeer logische componentgrenzen. Commit een plan als commentaar in het eerste commit-bericht.

Verwachte componenten in `Agterhuis.Ui.Designer.Components`:

| Component | Verantwoordelijkheid |
|---|---|
| `DesignerShell` | De volledige designer-layout: toolbar + grid van panelen + code-panel. Accepteert een `DesignDocument` en `IDesignStore`. Dit is het enige component dat een consumer hoeft te plaatsen. |
| `DesignerToolbar` | Nieuw/openen/opslaan/exporteren, undo/redo, viewport, canvas-theme. |
| `DesignerPalette` | Componentenlijst uit registry, gegroepeerd, filterbaar, drag-source. |
| `DesignerCanvas` | Slot-gebaseerd grid, dropzones, selectie, lege staat. Rendert de actieve pagina. |
| `DesignerCanvasNode` | Bestaat al — controleer of het al in de Designer RCL zit of in de demo. |
| `PropertyPanel` | Bestaat al — controleer locatie en verplaats indien nodig. |
| `DesignerDataPanel` | Bestaat al — controleer locatie. |
| `DesignerTreePanel` | Structuurboom met selectie-sync. |
| `DesignerCodePanel` | Bestaat al in de Designer RCL — controleer of het compleet is. |

## Fase 2 — Extraheer componenten

### Regels
- **Verplaats, herschrijf niet**: de bestaande logica (command-stack, drag & drop, selectie, autosave) blijft intact. Je verplaatst code naar nieuwe bestanden, niet herschrijven.
- **Eén publiek entry-point**: `<DesignerShell>` is het enige component dat een consumer in zijn pagina zet. Alle andere componenten zijn `internal` of genest.
- **Parameters, niet services**: de `DesignerShell` accepteert `IDesignStore`, `DesignerComponentRegistry`, en optionele configuratie (standaard canvas-theme, standaard viewport) als cascading values of parameters. De demo-app levert deze via DI.
- **JS-interop meeverhuizen**: `designer-interop.js` en `designer-resize-interop.js` verhuizen naar `wwwroot/` van de Designer RCL (zodat ze als static web assets meeliften met het NuGet-package). Update de script-referenties.
- **CSS meeverhuizen**: designer-specifieke CSS (`.designer-page`, `.designer-grid`, `.designer-panel`, etc.) verhuist naar de Designer RCL `wwwroot/css/`. De demo-app behoudt alleen app-specifieke styling.

### Stap voor stap
1. Maak de nieuwe component-bestanden aan in `src/Agterhuis.Ui.Designer/Components/`.
2. Verplaats de relevante Razor-markup en `@code`-blokken uit `Designer.razor` naar de juiste componenten.
3. Definieer duidelijke `[Parameter]`- en `[CascadingParameter]`-contracten tussen componenten. Gebruik `EventCallback` voor communicatie naar boven (selectie, commands).
4. Verplaats JS-bestanden en CSS naar de Designer RCL.
5. Reduceer `Designer.razor` in de demo-app tot:

```razor
@page "/designer/edit"
@layout DesignerLayout

<DesignerShell Store="@DesignStore"
               Registry="@Registry"
               DefaultCanvasTheme="plum-dark" />

@code {
    [Inject] private IDesignStore DesignStore { get; set; } = default!;
    [Inject] private DesignerComponentRegistry Registry { get; set; } = default!;
}
```


## Fase 3 — DI-registratie en consumer-API

- Voeg een `IServiceCollection.AddDesigner()` extensie-methode toe aan de Designer RCL die de benodigde services registreert (command-stack factory, registry, etc.).
- Documenteer in `docs/designer/CONSUMING.md` hoe een consumer de designer in zijn eigen app inbedt: NuGet-referentie, `AddDesigner()` in `Program.cs`, `<DesignerShell>` op een pagina, `IDesignStore`-implementatie leveren.

## Fase 4 — Verifieer dat niets is gebroken

- Alle bestaande tests moeten ongewijzigd groen blijven.
- De demo-app moet identiek functioneren als voor de extractie.
- Grep op `samples/Agterhuis.Ui.Demo` voor directe referenties naar designer-interne types die nu in de RCL zitten — er mogen geen interne types meer direct worden gebruikt.

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- De demo-app start en de designer werkt identiek aan de situatie vóór extractie
- `Designer.razor` in de demo-app is < 30 regels
- JS- en CSS-bestanden zitten in de Designer RCL `wwwroot/`
- `docs/designer/CONSUMING.md` bestaat en beschrijft de consumer-API
- Rapporteer: de componentstructuur (boom), de parameter-contracten per component, en de totale regels verplaatst vs. nieuw
