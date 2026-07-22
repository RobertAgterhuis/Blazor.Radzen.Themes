# Designer Functional Integrity Audit

Datum: 2026-07-22  
Scope: designer functionele ketens in componenten, wrappers, interop, registry en tests.

## 1. Samenvatting

Telling op basis van de action coverage matrix in dit document.

| Status | Aantal |
|---|---:|
| OK | 30 |
| Partieel | 7 |
| Defect | 3 |
| Niet geimplementeerd | 1 |

Kernuitkomst:
- 3 concrete defecten met reproduceerbare chain-breaks zijn bevestigd.
- 7 acties werken functioneel, maar met beperkingen of ontbrekende end-to-end borging.
- Grootste risico zit in state-sync tussen Data-panel en Shell, plus type-ongelijkheid bij kolom-resize.

## 2. Bevindingen Per Severity

### Critical

#### DFI-001 - Data panel entity-selectie wordt niet teruggeschreven naar Shell-state
- Status: Fixed (2026-07-22)
- Component: DesignerShell / DesignerDataPanel
- Bestanden:
  - [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor#L547](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor#L547)
  - [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L52](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L52)
  - [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L704](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L704)
- Reproduceerstappen:
  1. Open designer en ga naar Data-tab.
  2. Wijzig entiteit in de dropdown.
  3. Observeer dat callback wel vuurt, maar Shell veld `_selectedEntityName` niet wijzigt.
- Verwacht gedrag:
  - `SelectedEntityNameChanged` update ook lokale Shell state zodat render/model-keten consistent blijft.
- Actueel gedrag (voor fix):
  - `OnSelectedEntityChanged` riep alleen externe callback aan en muteerde `_selectedEntityName` niet.
- Breekpunt in keten:
  - UI event -> handler -> state update **breekt** (state update ontbreekt).
- Vermoedelijke root cause:
  - Handler is een pass-through geworden na refactor en mist lokale state-mutatie.
- Voorgestelde fixrichting:
  - In `OnSelectedEntityChanged(string value)`: `_selectedEntityName = value;` toevoegen, daarna callback invoken en `StateHasChanged` waar nodig.
- Testdekking:
  - Regressietest toegevoegd: [tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs](tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs) test `DesignerShell_FindingDfi001_SelectedEntityCallbackUpdatesShellState`.
- Wijzigingssamenvatting:
  - Handler in [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L704](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L704) update nu lokale state, invoke't callback en forceert render.
- Resterend risico:
  - Laag. Gedrag is afgedekt met regressietest.

### High

#### DFI-002 - Kolom-resize schrijft string-waarde, renderer verwacht int
- Status: Fixed (2026-07-22)
- Component: DesignerCanvasNode / DesignerShell
- Bestanden:
  - [src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L277](src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L277)
  - [src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L288](src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L288)
  - [src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L199](src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L199)
  - [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor#L446](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor#L446)
- Reproduceerstappen:
  1. Selecteer een `RadzenColumn` node.
  2. Klik resize-handle links/rechts.
  3. Node quick parameter schrijft `Size` als string (`"6"`).
  4. `GetColumnFlexStyle` leest alleen `int` uit `JsonValue`; parsing faalt en fallback wordt 12.
- Verwacht gedrag:
  - Kolombreedte verandert conform +/-1 en blijft render-consistent.
- Actueel gedrag (voor fix):
  - Breedte viel terug op 100% (12/12) in styleberekening.
- Breekpunt in keten:
  - UI event -> handler -> state update **ja**, render update **defect door type mismatch**.
- Vermoedelijke root cause:
  - `ApplyColumnSizeDelta` gebruikt `ToString`, terwijl renderpad strikt `int` leest.
- Voorgestelde fixrichting:
  - `NodeQuickParameterChanged` payload type uitbreiden of `Size` als numeriek literal opslaan.
  - Alternatief: tolerant parser in `GetColumnFlexStyle` die ook numerieke strings accepteert.
- Testdekking:
  - Regressietest toegevoegd: [tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs](tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs) test `DesignerCanvasNode_FindingDfi002_ColumnResizeKeepsNumericSizeFlow`.
- Wijzigingssamenvatting:
  - Parser in [src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L190](src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor#L190) accepteert nu ook numerieke string literals voor `Size`.
  - Resize-flow gebruikt dezelfde parser zodat render/update-keten consistent blijft.
- Resterend risico:
  - Laag. Edgecases met niet-numerieke strings vallen gecontroleerd terug.

#### DFI-003 - Code-tab edits persisteren niet naar model
- Status: Fixed (2026-07-22, via read-only contract)
- Component: DesignerCodePanel
- Bestanden:
  - [src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L324](src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L324)
  - [src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L335](src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L335)
  - [src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L14](src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L14)
- Reproduceerstappen:
  1. Open code panel op tab `Razor (preview)`.
  2. Wijzig code in Monaco callback `OnCodeEditorChanged`.
  3. Observeer dat geen document-callback of model-update plaatsvindt.
- Verwacht gedrag:
  - Ofwel read-only zonder editpad, ofwel edits worden geparsed en via `OnDocumentChanged` doorgezet.
- Actueel gedrag:
  - Handler doet alleen preview refresh en comment geeft expliciet aan dat parse/update niet geimplementeerd is.
- Breekpunt in keten:
  - UI event -> handler -> state update **beperkt**, model update/persist **breekt**.
- Vermoedelijke root cause:
  - Functionaliteit bewust onaf (stub), maar UI-pad is nog steeds aanwezig via JS hook.
- Voorgestelde fixrichting:
  - Kortetermijn: expliciet read-only contract en disable edit-sync pad. (uitgevoerd)
  - Langetermijn: echte Razor->model parseflow of transformatiepad implementeren.
- Testdekking:
  - Regressietest toegevoegd: [tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs](tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs) test `DesignerCodePanel_FindingDfi003_RazorPreviewIsHardReadOnly`.
- Wijzigingssamenvatting:
  - `OnCodeEditorChanged` in [src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L324](src/Agterhuis.Ui.Designer/Components/DesignerCodePanel.razor#L324) is nu contractueel no-op.
  - Code-tab toont expliciete read-only hint voor gebruikers.
- Resterend risico:
  - Middel: parserfunctionaliteit blijft open als aparte feature.
  - Roadmap vastgelegd in [docs/designer/RAZOR-PARSER-ROADMAP.md](docs/designer/RAZOR-PARSER-ROADMAP.md).

### Medium

#### DFI-004 - OnPageDragOver is leeg (functioneel niet defect, wel anti-pattern)
- Status: Fixed (2026-07-22)
- Component: DesignerShell
- Bestand: [src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L2633](src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs#L2633)
- Reproduceerstappen:
  1. Page-tab drag reorder gebruiken.
  2. Werkt door `@ondragover:preventDefault` in markup, niet door handler.
- Verwacht gedrag:
  - Handler implementeert expliciete intentie of wordt verwijderd.
- Actueel gedrag:
  - Lege handler, gedrag hangt volledig op markup-attribute.
- Root cause:
  - Restant van eerdere implementatie.
- Fixrichting:
  - Verwijderen of documenteren met comment waarom leeg.
- Testdekking:
  - Reorder flow aanwezig in [tests/Agterhuis.Ui.Tests/DesignerPageTests.cs](tests/Agterhuis.Ui.Tests/DesignerPageTests.cs).
  - Regressietest toegevoegd: [tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs](tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs) test `DesignerShell_FindingDfi004_PageDragOverSetsExplicitIntentState`.
- Wijzigingssamenvatting:
  - `OnPageDragOver` zet nu expliciet `_pageDragOverIndex` en `DropEffect` op `move`.
  - Page-tab markup gebruikt `designer-page-tab--drag-over` als visuele DnD intentie-state.
- Resterend risico:
  - Laag. Functionele reorder-flow bleef intact en handler is niet langer leeg/no-op.

### Low

Geen aanvullende low severity defects bevestigd in deze run.

## 3. Action Coverage Matrix

| Actie | UI event | Handler | State update | Render update | Model update | Persist/restore | Status | Opmerking |
|---|---|---|---|---|---|---|---|---|
| Bestand menu toggle | `@onclick` | `ToggleFileMenu` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Settings menu toggle | `@onclick` | `ToggleSettingsMenu` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Close menus backdrop | `@onclick` | `CloseAllMenus` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Save document | `@onclick` / Ctrl+S | `OnSaveDocument` | Ja | Ja | Ja | Ja | OK | |
| Export dialog open | `@onclick` / Ctrl+E | `OnExportDocument` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Export confirm | `@onclick` | `ConfirmExportAsync` | Ja | Ja | N.v.t. | Bestand | OK | |
| Undo | `@onclick` / Ctrl+Z | `OnUndo` | Ja | Ja | Ja | Ja | OK | |
| Redo | `@onclick` / Ctrl+Y | `OnRedo` | Ja | Ja | Ja | Ja | OK | |
| Preview toggle | `@onclick` / Ctrl+P | `TogglePreviewMode` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Interaction toggle | `@onclick` / Ctrl+I | `ToggleInteractionMode` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Viewport mobile/tablet/desktop | `@onclick` | `SetViewport` | Ja | Ja | N.v.t. | Nee | Partieel | Geen persist van viewport state. |
| Open file picker | `@onclick` | `OnOpenDocument` | Ja | Ja | Ja | Ja | OK | |
| Import file InputFile | `OnChange` | `OnDesignFileChanged` | Ja | Ja | Ja | Ja | OK | |
| Open saved doc dropdown | `Change` | `OnSavedSelectionChanged` | Ja | Ja | Ja | Ja | OK | |
| Template start select | `@onclick` | `OnTemplateStartSelected` | Ja | Ja | Ja | Ja | OK | |
| Startscreen open saved | `Click` | `OpenSavedFromStartAsync` | Ja | Ja | Ja | Ja | OK | |
| Startscreen import | `OnChange` | `OnStartScreenImportRequested` | Ja | Ja | Ja | Ja | OK | |
| Palette filter | `@oninput` | `OnPaletteFilterChanged` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Palette click add | `@onclick` | `OnPaletteItemClickedAsync` | Ja | Ja | Ja | Ja | OK | |
| Palette drag start | `@ondragstart` | `OnPaletteDragStart` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Root dropzone drop | `@ondrop` | `OnDropRequested` | Ja | Ja | Ja | Ja | OK | |
| Slot dropzone drop | `@ondrop` | `OnDropRequested` | Ja | Ja | Ja | Ja | OK | |
| Node select click | `@onclick` | `OnSelectNode` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Node inline edit commit | blur/enter | `OnInlineEditCommitted` | Ja | Ja | Ja | Ja | OK | |
| Node quick resize | `@onclick` | `ApplyColumnSizeDelta` -> `OnInlineEditCommitted` | Ja | Ja | Ja | Ja | OK | Gefixt via tolerante `Size` parsing en regressietest. |
| Tree context duplicate/delete/up/down | `@onclick` | `OnTreeContext*Async` | Ja | Ja | Ja | Ja | OK | |
| Wrap in card/row | `@onclick` | `WrapNode*Async` | Ja | Ja | Ja | Ja | OK | |
| Issue severity filters | `@onchange` | inline bool assign | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Issue auto-fix | `@onclick` | `ApplyIssueFixAsync` | Ja | Ja | Ja | Ja | OK | |
| Data panel entity select | `ValueChanged` | `OnEntityChanged` -> Shell `OnSelectedEntityChanged` | Ja | Ja | N.v.t. | N.v.t. | OK | Gefixt: parent state sync aanwezig. |
| Data panel row count | `ValueChanged` | `OnRowCountChanged` | Ja | Ja | Ja | Via shell autosave | OK | |
| Data panel seed | `ValueChanged` | `OnSeedChanged` | Ja | Ja | Ja | Via shell autosave | OK | |
| Data panel generate form | `Click` | `OnGenerateFormRequested` | Ja | Ja | Ja | Ja | OK | |
| Data import preview | `Click` | `PreviewImportAsync` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Data import apply | `Click` | `ImportPreviewAsync` | Ja | Ja | Ja | Ja | OK | |
| Property panel advanced toggle | `ValueChanged` | `OnAdvancedModeChanged` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Property section tabs | `@onclick` | `SetActiveSection` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Property reset param | `Click` | `OnResetParameter` | Ja | Ja | Ja | Ja | OK | |
| Property bool/string/enum/date | `ValueChanged` | `On*Changed` methods | Ja | Ja | Ja | Ja | OK | |
| Code tab switch | `@onclick` | `CurrentTab` set | Ja | Ja | N.v.t. | N.v.t. | OK | |
| JSON editor apply | `@onclick`/JS callback | `ApplyModelAsync`/`OnJsonEditorChanged` | Ja | Ja | Ja | Ja | OK | |
| Razor code editor change | JS callback | `OnCodeEditorChanged` | Ja (no-op) | Ja | N.v.t. (contractueel) | N.v.t. | OK | Hard read-only contract; modelmutaties alleen via JSON-tab. |
| Command palette open/search/execute | click/keys | `OpenDesignerCommandPaletteAsync` + execute | Ja | Ja | Afhankelijk command | Afhankelijk command | OK | |
| Shortcuts overlay | click/Ctrl+/Esc | `ToggleShortcutsOverlay` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| Toast dismiss | `@onclick` | `DismissToast` | Ja | Ja | N.v.t. | N.v.t. | OK | |
| JS drop callback | JSInvokable | `OnJavaScriptDrop` | Ja | Ja | Ja | Ja | Partieel | Parallel DnD pad, beperkt gebruikt en niet expliciet testafgedekt. |
| Page tab drag reorder | DnD | `OnPageDropAsync` + `OnPageDragOver` | Ja | Ja | Ja | Ja | OK | Dragover-handler heeft nu expliciete intentie-state en drop effect. |
| Route redirect start `/designer` | navigation | `DesignerStart` redirect | Ja | Ja | N.v.t. | N.v.t. | Niet geimplementeerd | Buiten `DesignerShell` runtimeketen; aparte routecomponent doet redirect. |

## 4. Testgaten En Toegevoegde Tests

Bestaande suites dekken veel happy-path gedrag, maar misten drie defect-ketens.

Nieuw toegevoegd (audit-veilig):
- [tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs](tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs)
  - `DesignerShell_FindingDfi001_SelectedEntityCallbackUpdatesShellState`
  - `DesignerCanvasNode_FindingDfi002_ColumnResizeKeepsNumericSizeFlow`
  - `DesignerCodePanel_FindingDfi003_RazorPreviewIsHardReadOnly`
  - `DesignerShell_FindingDfi004_PageDragOverSetsExplicitIntentState`

Aanvullende gaten die nog niet met test zijn afgedicht:
- JS-only pad `OnJavaScriptDrop` krijgt geen directe bUnit/e2e assert op parity met Blazor DnD.

## 5. Prioriteitenlijst Voor Fixronde

1. DFI-001 (Critical): herstel state-sync voor geselecteerde entiteit tussen DataPanel en Shell.
2. DFI-002 (High): harmoniseer typecontract voor kolom-`Size` (numeriek end-to-end).
3. DFI-003 (High): maak code-tab expliciet read-only of implementeer echte modelsync.
4. DFI-004 (Medium): ruim lege dragover-handler op of documenteer intent. (afgerond)

## 6. Runtime Sanity Check Notitie

Lokale `dotnet run` sessies van de demo faalden in deze omgeving eerder al (zie terminals), waardoor handmatige runtime smoke niet volledig uitvoerbaar was in deze run. De audit baseert runtime-conclusies daarom op code-tracing en testgedrag; fixronde moet een expliciete handmatige sanity herhalen zodra demo-run stabiel start.

## 7. Fixronde 2026-07-22

Gefixte IDs:
- DFI-001
- DFI-002
- DFI-003
- DFI-004

Openstaande IDs:
- Geen open IDs uit deze auditrun.

Parser roadmap:
- [docs/designer/RAZOR-PARSER-ROADMAP.md](docs/designer/RAZOR-PARSER-ROADMAP.md)
