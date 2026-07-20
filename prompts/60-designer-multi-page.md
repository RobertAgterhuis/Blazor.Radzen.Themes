# Prompt 60 — Designer multi-page support

Het `DesignDocument`-model ondersteunt meerdere pagina's (`Pages`-lijst), maar de UI gebruikt consequent `Pages[0]`. Canvas, property panel, structuurboom, code-panel en alle command-aanroepen zijn hardcoded op de eerste pagina. Voeg volledige multi-page ondersteuning toe aan de designer-UI.

## Uitvoeringscontext (GitHub Copilot agent mode)

Agent mode met terminal-toegang, repo-root `D:\repositories\Blazor.Radzen.Themes`; `.github/copilot-instructions.md` bindend; fasen met groene build + eigen commit; bestaande infra hergebruiken. Zie `prompts/PROMPT-CONVENTIONS.md`.

---

## Fase 1 — Geselecteerde pagina-state

- Voeg een `_selectedPageIndex` (of `_selectedPageId`) state toe aan de designer.
- Introduceer een `ActivePage` property die de geselecteerde pagina retourneert (met fallback naar `Pages[0]` als de index ongeldig is).
- **Vervang ELKE `Pages[0]`-referentie** in de designer-componenten door `ActivePage`. Grep op `Pages[0]` in de hele solution en vervang — er mogen nul hardcoded `Pages[0]`-referenties overblijven buiten tests en de AddNodeCommand-helper.

## Fase 2 — Page navigator UI

- Voeg een page-tab-balk toe boven het canvas (of als horizontale tabs in de toolbar-area).
- Elke tab toont de paginanaam (of route als naam leeg is).
- Klik op een tab wisselt de actieve pagina — canvas, property panel, tree en code-panel updaten direct.
- Visuele indicator voor de actieve tab (accent-underline of achtergrondkleur).
- Als er meer pagina's zijn dan in de breedte passen: horizontaal scrollbaar met pijl-knoppen.

### Tab-acties
- **"+" knop** rechts van de tabs: voegt een nieuwe lege pagina toe (via het bestaande `AddPageCommand`). De nieuwe pagina wordt direct actief.
- **Rechter-muisklik op een tab** (of via een `⋮` menu-icoon): contextmenu met:
  - Hernoemen (inline editing van de tab-tekst)
  - Dupliceren (diepe kopie van de pagina + nodes, met unieke route)
  - Verwijderen (met `AgtConfirmDialog` bevestiging; blokkeer als het de laatste pagina is)
  - Omhoog/omlaag verplaatsen (herschik pagina-volgorde)
- **Slepen van tabs**: pagina-volgorde herschikken via drag & drop op de tab-balk.

## Fase 3 — Route-management

- Elke pagina heeft een unieke route. Bij aanmaken: auto-genereer route (`/page-2`, `/page-3`, etc.).
- Route bewerken via het property panel (bestaande "Route"-veld werkt al voor de actieve pagina).
- **Validatie**: duplicate routes zijn een fout — toon inline in de route-editor EN in het toekomstige Issues-panel (prompt 61).
- Route-preview: toon onder de tab-balk of in een tooltip hoe de routes eruitzien in de geëxporteerde app.

## Fase 4 — Commands page-aware maken

- Controleer alle commands (`AddNodeCommand`, `MoveNodeCommand`, `RemoveNodeCommand`, `DuplicateNodeCommand`, `SetNodeParameterCommand`, `SetNodeLayoutSlotCommand`, `ReorderSiblingCommand`): ze moeten opereren op de ACTIEVE pagina, niet op `Pages[0]`.
- `AddPageCommand` bestaat al — verifieer dat het correct werkt met de page-navigator.
- Voeg toe: `RemovePageCommand`, `DuplicatePageCommand`, `ReorderPageCommand`, `RenamePageCommand`. Alle undo-baar via de command-stack.
- Bij het verwijderen van de actieve pagina: wissel automatisch naar de vorige of volgende pagina.

## Fase 5 — Code-panel en export page-aware

- De code-tab toont de Razor-code van de ACTIEVE pagina (niet altijd pagina 0).
- De JSON model-tab toont het volledige document (alle pagina's) — maar markeert de actieve pagina visueel (bijv. een `// ← active` commentaar of een highlight in de JSON).
- Export genereert een `.razor`-bestand PER pagina (dit zou al moeten werken als `RazorCodeGenerator` over alle pagina's itereert — verifieer).
- De gegenereerde `NavMenu` in het exportproject bevat links naar alle pagina's.

## Fase 6 — Tests

- bUnit: voeg een pagina toe, wissel actieve pagina, verifieer dat canvas en property panel de juiste pagina tonen.
- Command-tests: RemovePage, DuplicatePage, ReorderPage, RenamePage + undo/redo.
- Validatie: duplicate routes worden gedetecteerd.
- Export: multi-page document exporteert correct met meerdere `.razor`-bestanden.
- Grep-bewijs: nul `Pages[0]`-referenties in de designer runtime-code (tests mogen het wel gebruiken voor setup).

## Verificatie

- `dotnet build -c Release` zero warnings
- `dotnet test` groen
- Grep op `Pages\[0\]` in `src/Agterhuis.Ui.Designer/` en `samples/Agterhuis.Ui.Demo/Components/Pages/Designer.razor`: nul hits (buiten test-setup)
- Handmatig: nieuw document → pagina toevoegen → tab-wisseling → canvas toont de juiste pagina → hernoemen → dupliceren → verwijderen → undo brengt verwijderde pagina terug → export bevat beide pagina's als aparte `.razor`-bestanden
- Rapporteer: het aantal vervangen `Pages[0]`-referenties, de nieuwe command-set, en eventuele edge-cases bij pagina-verwijdering
