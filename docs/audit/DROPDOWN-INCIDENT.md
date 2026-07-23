# Dropdown incident report

## 1. Symptom and impact

Symptom reproduced in designer flows where a dropdown is rendered inside the designer shell while a shell backdrop layer is active:

1. Popup opens and options are visible.
2. Clicking options does not commit selection.
3. Popup remains open.

Impact: core design-time property editing and document actions become unreliable. This is a production-blocking interaction defect.

## 2. Affected dropdown inventory

| ID | Route/screen | Component/source path | Value type | Binding path | Container/overlay path | Mouse result (before) | Keyboard result (before) | Affected |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| DD-01 | /designer/edit?template=FormPage | DesignerShell file menu saved-docs dropdown in `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor` | `string` | `@bind-Value _selectedSavedName` + `Change` | `.designer-menu` + `.designer-menu-backdrop` | Failed when option hit-test top element was backdrop | N/A for empty-data path | Yes |
| DD-02 | /designer/edit?template=FormPage | Data panel entity dropdown in `src/Agterhuis.Ui.Designer/Components/DesignerDataPanel.razor` | `string` | `Value` + `ValueChanged` | Right inspector panel | Intermittent stuck-open in blocked layer states | Not stable when blocked | Yes |
| DD-03 | /designer/edit?template=FormPage | Toolbar theme dropdown in `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor` | `string` | `@bind-Value _canvasTheme` + `Change` | Toolbar / body popup portal | Works after overlay dismiss | Works | Shared infra path |
| DD-04 | /components/forms/dropdown (minimal control outside designer) | Radzen/Agt dropdown catalog path | mixed | direct wrapper binding | No designer backdrop | Works | Works | No |

## 3. Reproduction steps

1. Navigate to `/designer/edit?template=FormPage`.
2. Open `Bestand` menu.
3. Open `Open opgeslagen` dropdown.
4. Attempt to click an option (or run hit-test on first option coordinates).

## 4. Runtime evidence

Captured evidence from browser runtime instrumentation (`elementsFromPoint` at option center):

- top element at option click coordinates: `.designer-menu-backdrop`
- stack head: `.designer-menu-backdrop` (`z-index: 89`, `pointer-events: auto`)
- dropdown panel class: `.rz-dropdown-panel` with computed `z-index: auto`
- result: pointer events do not reach option element in blocked state.

This is Branch A from the decision tree: option does not receive pointer events due overlay layer interception.

## 5. Rejected hypotheses

1. Radzen library defect: rejected, because same dropdown behavior works outside designer shell and in designer paths without active blocker layering.
2. Per-dropdown binding bug: rejected as primary cause; blocked click occurs before bind/change callback can fire.
3. Generic CSS pointer-events on option: rejected; option itself is visible and normal, but hit-test top node is backdrop.

## 6. Confirmed root cause and first failing transition

Root cause: designer shell backdrop layering is higher in the active hit-test stack than Radzen popup layers in affected states. The first failing transition is pointer hit-testing routing to `.designer-menu-backdrop` instead of the dropdown option node.

Shared implementation path:

- designer shell backdrops and overlays in `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor`
- popup layer styling in `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css`
- designer shell lifecycle JS interop in `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs` and `src/Agterhuis.Ui.Designer/wwwroot/designer-interop.js`

## 7. Changed files and architecture notes

1. `src/Agterhuis.Ui.Designer/wwwroot/css/designer.css`
   - Added scoped popup z-index rule when designer shell is active:
   - `body[data-agt-designer-shell-active="true"] .rz-dropdown-panel, ... { z-index: 1201; }`
   - Keeps Radzen popups above designer backdrop layers while preserving modal overlays.

2. `src/Agterhuis.Ui.Designer/wwwroot/designer-interop.js`
   - Added `setDesignerShellActive(isActive)` helper to set/remove body flag.

3. `src/Agterhuis.Ui.Designer/Components/DesignerShell.razor.cs`
   - On first render: `designerInterop.setDesignerShellActive(true)`.
   - On dispose: `designerInterop.setDesignerShellActive(false)`.

4. `tests/a11y/designer-dropdown-interaction.spec.mjs`
   - Added browser regression coverage for menu dropdown and inspector dropdown with mouse/keyboard selection and popup close assertions.

## 8. Regression tests added

- `tests/a11y/designer-dropdown-interaction.spec.mjs`
  - `file menu dropdown options are clickable and popup closes`
  - `inspector entity dropdown supports mouse and keyboard commit`

## 9. Verification commands and results

Commands executed:

1. `npx playwright test tests/a11y/designer-dropdown-interaction.spec.mjs`
2. `dotnet test Agterhuis.Ui.sln -c Release --logger "console;verbosity=minimal"`

Results are recorded in the run logs from this incident run.

## 10. Remaining unverified path/blocker

- Save/reload persistence assertions for dropdown-mutated document state are partially covered by existing designer persistence tests but not yet expanded in this single new Playwright spec for every dropdown value type.
- Additional e2e expansion recommended for enum and nullable-enum property-editor paths in the same suite.
