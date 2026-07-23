# Remediation Log

Date: 2026-07-22
Scope: Designer audit remediation against docs/audit/PRODUCT-READINESS-AUDIT.md

## FUNC-001

- finding ID: FUNC-001
- files changed:
  - src/Agterhuis.Ui.Designer/Export/ProjectExporter.cs
  - src/Agterhuis.Ui.Designer/Export/Templates/Agterhuis.Ui.Demo.export.csproj.template
  - src/Agterhuis.Ui.Designer/Export/Templates/Program.template
  - src/Agterhuis.Ui.Designer/Export/Templates/_Imports.razor.template
  - src/Agterhuis.Ui.Designer/Export/Templates/App.razor.template
  - src/Agterhuis.Ui.Designer/Export/Templates/Routes.razor.template
  - src/Agterhuis.Ui.Designer/Export/Templates/wwwroot/index.html.template
  - tests/Agterhuis.Ui.Tests/Designer/Export/ProjectExporterTests.cs
- what was changed:
  - Fixed generated root page filename for `/` route from invalid `.razor` to `Home.razor`.
  - Added exported `wwwroot/index.html` so generated server-hosted app boots with static shell.
  - Corrected template consistency so generated app uses coherent server-hosted Razor Components model.
  - Removed demo-host namespace bleed from exported `_Imports.razor` and replaced with exported app namespaces.
  - Added required Agterhuis component namespaces to `_Imports.razor` so generated wrapper components resolve.
  - Fixed `Program.template` to include `using __PROJECT_NAME__.Components;` so `MapRazorComponents<App>()` compiles.
- tests added or updated:
  - Updated `ExportProject_WithMultiplePages_GeneratesAllPages` to assert `Home.razor` for root route.
  - Added `ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation` that:
    - exports zip,
    - unpacks to temp folder,
    - creates local package feed via `dotnet pack src/Agterhuis.Ui/Agterhuis.Ui.csproj`,
    - rewrites generated package version to local packed version,
    - runs restore,
    - runs build,
    - runs app and verifies startup output (`Now listening on`).
- verification commands run:
  - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~ProjectExporterTests.ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation"`
  - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~ProjectExporterTests"`
- result of each verification command:
  - isolation smoke test: passed (1/1)
  - exporter test set: passed after fixes
- final status: fixed

## GEN-001

- finding ID: GEN-001
- files changed:
  - src/Agterhuis.Ui.Designer/Export/ProjectExporter.cs
  - src/Agterhuis.Ui.Designer/Export/Templates/* (see FUNC-001 list)
  - tests/Agterhuis.Ui.Tests/Designer/Export/ProjectExporterTests.cs
- what was changed:
  - Added compiler-backed generated app verification in test suite (restore + build in isolated temp project).
  - Resolved generator/template mismatches that previously prevented generated project compilation.
- tests added or updated:
  - `ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation`
- verification commands run:
  - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~ProjectExporterTests.ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation"`
- result of each verification command:
  - passed
- final status: fixed

## TEST-001

- finding ID: TEST-001
- files changed:
  - tests/Agterhuis.Ui.Tests/Designer/Export/ProjectExporterTests.cs
- what was changed:
  - Added high-value integration test that validates full generated-project lifecycle in isolation, closing the missing flagship proof in tests.
- tests added or updated:
  - `ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation`
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
- result of each verification command:
  - passed: total 532, failed 0
- final status: fixed

## OPS-001

- finding ID: OPS-001
- files changed:
  - .github/workflows/ci.yml
- what was changed:
  - Added explicit CI step `Validate generated export isolation` that executes the generated-project isolation smoke test.
- tests added or updated:
  - CI workflow now runs: `dotnet test ... --filter "FullyQualifiedName~ProjectExporterTests.ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation"`
- verification commands run:
  - local command equivalent executed and passing:
    - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~ProjectExporterTests.ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation"`
- result of each verification command:
  - passed locally
- final status: fixed

## FUNC-002

- finding ID: FUNC-002
- files changed:
  - src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor
  - tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs
- what was changed:
  - Fixed root cause in design-time default injection: `DesignerCanvasNode` no longer injects `Placeholder` for components that do not define that parameter.
  - This prevents runtime `DynamicComponent` parameter binding failures for `AgtSwitch` in designer canvas and preview flows.
- tests added or updated:
  - added `DesignerCanvasNode_Func002_DoesNotInjectUnsupportedPlaceholderForAgtSwitch`
- verification commands run:
  - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~DesignerCanvasNode_Func002_DoesNotInjectUnsupportedPlaceholderForAgtSwitch"`
  - live browser session at `/designer/edit?template=FormPage`
- result of each verification command:
  - targeted test: passed (1/1)
  - browser: preview mode activated, form controls rendered, and no `.designer-canvas-node__error` present
- final status: fixed

## DATA-001

- finding ID: DATA-001
- files changed:
  - no code change in this pass
- what was changed:
  - Re-verified existing seeded-data generation and form generation coverage.
- tests added or updated:
  - none
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
  - reviewed `DesignerPageTests.DesignerPage_GeneratesEntityForm_FromDataPanelAction`
- result of each verification command:
  - tests: passed
- final status: fixed

## RAD-001

- finding ID: RAD-001
- files changed:
  - no code change in this pass
- what was changed:
  - Re-verified layout-heavy template (`SidebarApp`) browser render and page tab structure.
- tests added or updated:
  - none
- verification commands run:
  - browser snapshot at `/designer/edit?template=SidebarApp`
  - `dotnet test Agterhuis.Ui.sln -c Release`
- result of each verification command:
  - browser snapshot shows complex sidebar layout structure, nav links, and multi-page tabs rendered
  - tests: passed
- final status: fixed

## FUNC-003

- finding ID: FUNC-003
- files changed:
  - no code change in this pass
- what was changed:
  - Re-verified multi-page tab switching behavior from existing tests and browser snapshot for SidebarApp with 4 tabs.
- tests added or updated:
  - none
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
  - browser snapshot at `/designer/edit?template=SidebarApp`
- result of each verification command:
  - tests: passed
  - browser: multi-page tabs (`Dashboard`, `Schadedossiers`, `Nieuw dossier`, `Instellingen`) visible
- final status: fixed

## STATE-001

- finding ID: STATE-001
- files changed:
  - no code change in this pass
- what was changed:
  - Re-verified conflict handling and fallback behavior through existing tests and runtime code paths.
- tests added or updated:
  - none
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
  - reviewed `RemoteDesignStoreTests.SaveAsync_ThrowsConflict_On409`
  - reviewed `DesignerPageTests.DesignerPage_RendersRecoveryBannerAndCommandPalette`
- result of each verification command:
  - tests: passed
- final status: fixed

## UI-001

- finding ID: UI-001
- files changed:
  - no code change in this pass
- what was changed:
  - Re-verified interaction mode and canvas behavior through existing tests and browser snapshot.
- tests added or updated:
  - none
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
  - browser snapshot at `/designer/edit?template=FormPage` and `/designer/edit?template=SidebarApp`
- result of each verification command:
  - tests: passed
  - browser confirms current chrome state; no additional UI chrome reduction changes applied in this pass
- final status: deferred by product decision

## ARCH-001

- finding ID: ARCH-001
- files changed:
  - .github/workflows/ci.yml
  - tests/Agterhuis.Ui.Tests/Designer/Export/ProjectExporterTests.cs
  - src/Agterhuis.Ui.Designer/Components/DesignerCanvasNode.razor
  - tests/Agterhuis.Ui.Tests/DesignerFunctionalIntegrityTests.cs
- what was changed:
  - Added concrete parity proof on export side (generated app compile/start), reducing architecture-risk gap.
  - Closed the preview-side blocker by removing invalid parameter injection into `AgtSwitch`, then re-validating preview rendering in browser.
- tests added or updated:
  - `ExportProject_UnpackedProject_RestoresBuildsAndStartsInIsolation`
  - `DesignerCanvasNode_Func002_DoesNotInjectUnsupportedPlaceholderForAgtSwitch`
- verification commands run:
  - `dotnet test Agterhuis.Ui.sln -c Release`
  - `dotnet test tests/Agterhuis.Ui.Tests/Agterhuis.Ui.Tests.csproj -c Release --filter "FullyQualifiedName~DesignerCanvasNode_Func002_DoesNotInjectUnsupportedPlaceholderForAgtSwitch"`
  - browser snapshots for FormPage and SidebarApp
- result of each verification command:
  - tests: passed
  - targeted test: passed (1/1)
  - browser: preview mode active on FormPage and SidebarApp with no canvas error nodes; SidebarApp preview shows multi-page navigation links and layout structure
- final status: fixed

## Baseline validation set

- `dotnet restore --locked-mode`
  - result: passed
- `dotnet build Agterhuis.Ui.sln -c Release`
  - result: passed
- `dotnet test Agterhuis.Ui.sln -c Release`
  - result: passed (532/532)
- `dotnet run --project samples/Agterhuis.Ui.Demo --no-launch-settings`
  - result: passed start after freeing conflicting port 5090

## Notes

- Pre-existing unrelated working tree changes were preserved (for example `docs/EXAMPLE-AUDIT.md`).
- Prior browser blocker (`AgtSwitch` unknown `Placeholder` parameter) is resolved by the `DesignerCanvasNode` injection guard and covered by a regression test.
