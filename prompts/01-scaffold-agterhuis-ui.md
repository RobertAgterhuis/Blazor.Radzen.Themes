# Prompt 1 — Scaffold Agterhuis.Ui

Copy everything below the line into Claude Code (or your coding agent), started in `D:\repositories\Agterhuis.Ui`.

---

You are scaffolding **Agterhuis.Ui**: an internal Razor Class Library (RCL) that packages reusable Blazor components (wrapping Radzen), a custom theme, design tokens, and static assets as a NuGet package for Azure Artifacts. The repo root is the current directory and is empty.

## Hard constraints

- Target framework: **net10.0** everywhere.
- `Radzen.Blazor` as a normal **PackageReference** (NO `PrivateAssets=all` — it must flow to consumers). Pin the latest stable 11.x version explicitly (check nuget.org; do not use floating versions).
- Central Package Management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=true`). No versions in csproj files.
- `Directory.Build.props` with: `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `Deterministic=true`, `DebugType=embedded`, `LangVersion=latest`, `AnalysisLevel=latest`, `EnforceCodeStyleInBuild=true`, `RestorePackagesWithLockFile=true`, and `ContinuousIntegrationBuild=true` when `TF_BUILD=true`.
- Component prefix: **Agt** (e.g. `AgtPrimaryButton`). Root namespace/PackageId: `Agterhuis.Ui`.
- Do NOT copy or embed any Radzen premium theme assets. Base the theme on the free `material-base.css` plus our own CSS variables and overrides.
- Commit `packages.lock.json` files after restore.

## Repository layout

```
Agterhuis.Ui/
├── src/Agterhuis.Ui/                  (razorclasslib, Sdk=Microsoft.NET.Sdk.Razor)
│   ├── _Imports.razor
│   ├── Components/{Buttons,Layout,Feedback,Forms,Data}/
│   ├── Extensions/ServiceCollectionExtensions.cs
│   ├── Options/AgtUiOptions.cs
│   └── wwwroot/css/{agt-tokens.css,agt-theme.css,agt-utilities.css}
├── samples/Agterhuis.Ui.Demo/         (blazor, --interactivity Server --all-interactive)
├── tests/Agterhuis.Ui.Tests/          (xunit + bunit)
├── azure-pipelines.yml
├── Directory.Build.props
├── Directory.Packages.props
├── NuGet.config
├── CHANGELOG.md
├── README.md
└── Agterhuis.Ui.sln
```

Demo and test projects get project references to `src/Agterhuis.Ui`. Add all three projects to the solution.

## NuGet package metadata (src/Agterhuis.Ui.csproj)

PackageId/AssemblyName/RootNamespace `Agterhuis.Ui`; Title "Agterhuis Blazor UI"; Description "Internal Agterhuis Blazor design system: reusable Radzen-based components, design tokens, theme and static assets."; Authors/Company "Agterhuis"; `IsPackable=true`, `GeneratePackageOnBuild=false`, `IncludeSymbols=true`, `SymbolPackageFormat=snupkg`, `EmbedUntrackedSources=true`, `PublishRepositoryUrl=true`, `RepositoryType=git`, RepositoryUrl placeholder `https://dev.azure.com/ORGANIZATION/PROJECT/_git/Agterhuis.Ui`.

## Design tokens — REAL brand colors (extracted from blog.agterhuis.net)

The brand is **deep royal purple with vivid gold accents on a dark surface**. Use exactly these values in `wwwroot/css/agt-tokens.css`:

```css
:root {
    /* Brand — purple (measured anchors: #680898 vivid, #560a7f, #480868, #380858, #210132 darkest) */
    --agt-color-primary-50:  #f5ebfb;
    --agt-color-primary-100: #e5cef4;
    --agt-color-primary-200: #c894e3;
    --agt-color-primary-300: #a95ed1;
    --agt-color-primary-400: #8a2bb8;
    --agt-color-primary-500: #680898;   /* vivid brand purple */
    --agt-color-primary-600: #560a7f;
    --agt-color-primary-700: #480868;
    --agt-color-primary-800: #380858;
    --agt-color-primary-900: #2b0840;
    --agt-color-primary-950: #210132;   /* darkest page background */

    /* Brand — gold accent (measured: #f1ce05 bright, #9e8715 muted) */
    --agt-color-accent-300: #f6dc4d;
    --agt-color-accent-400: #f1ce05;    /* primary gold */
    --agt-color-accent-500: #d9b504;
    --agt-color-accent-600: #b08f10;
    --agt-color-accent-700: #9e8715;

    /* Semantic */
    --agt-color-success: #27ae60;
    --agt-color-warning: #e67e22;
    --agt-color-danger:  #e74c3c;
    --agt-color-info:    #3498db;

    /* Neutral */
    --agt-color-white:    #ffffff;
    --agt-color-gray-50:  #f8f9fa;
    --agt-color-gray-100: #f1f3f5;
    --agt-color-gray-200: #e9ecef;
    --agt-color-gray-500: #6c757d;
    --agt-color-gray-700: #343a40;
    --agt-color-gray-900: #17191c;

    /* Typography */
    --agt-font-family: "Segoe UI Variable", "Segoe UI", -apple-system, BlinkMacSystemFont, sans-serif;
    --agt-font-size-xs: 0.75rem; --agt-font-size-sm: 0.875rem; --agt-font-size-md: 1rem;
    --agt-font-size-lg: 1.125rem; --agt-font-size-xl: 1.5rem;

    /* Spacing */
    --agt-spacing-1: 0.25rem; --agt-spacing-2: 0.5rem; --agt-spacing-3: 0.75rem;
    --agt-spacing-4: 1rem; --agt-spacing-6: 1.5rem; --agt-spacing-8: 2rem;

    /* Borders / shadows / motion */
    --agt-border-radius-sm: 0.25rem; --agt-border-radius-md: 0.5rem; --agt-border-radius-lg: 0.75rem;
    --agt-shadow-sm: 0 1px 2px rgb(33 1 50 / 20%);
    --agt-shadow-md: 0 4px 12px rgb(33 1 50 / 30%);
    --agt-transition-fast: 120ms ease-in-out; --agt-transition-normal: 200ms ease-in-out;
}
```

## Theme (`agt-theme.css`)

`@import url("./agt-tokens.css");` then map Radzen variables:

- Light mode (default, `:root`): `--rz-primary: var(--agt-color-primary-500)`, `--rz-primary-light: var(--agt-color-primary-100)`, `--rz-primary-dark: var(--agt-color-primary-700)`, `--rz-secondary: var(--agt-color-accent-500)`, semantic vars mapped, `--rz-text-font-family`, all border-radius vars → `--agt-border-radius-md`, body background `--agt-color-gray-50`, text `--agt-color-gray-900`.
- Dark mode (`[data-agt-theme="dark"]`, matching the blog's look): body background `--agt-color-primary-950`, surfaces `--agt-color-primary-900`/`-800`, text `--agt-color-white`, `--rz-primary: var(--agt-color-primary-400)`, links/highlights `--agt-color-accent-400` (gold).
- Component overrides: `.rz-button` (min-height 2.5rem, font-weight 600, transitions, focus-visible outline `3px solid rgb(104 8 152 / 35%)`), inputs min-height 2.5rem, `.rz-card` border + shadow-sm, `.rz-dialog` radius-lg + shadow-md, datagrid radius + bold headers, and a `prefers-reduced-motion` block.

Also create `agt-utilities.css` with `.agt-page`, `.agt-stack`, `.agt-row`, `.agt-surface`, `.agt-sr-only` using the spacing tokens.

## Components (initial set)

1. `AgtPrimaryButton.razor` — wraps `RadzenButton` (Variant.Filled, ButtonStyle.Primary). Parameters: Text, Icon, AriaLabel, CssClass, BusyText ("Bezig..."), Disabled, IsBusy, ButtonType, Size, `EventCallback<MouseEventArgs> Click`. Disabled while busy; guard the click handler.
2. `AgtSecondaryButton.razor` — same shape, Variant.Outlined + gold accent styling.
3. `AgtPageHeader.razor` (+ `.razor.css` CSS isolation) — Title (EditorRequired), Description, `RenderFragment? Actions`; responsive flex layout using tokens.
4. `AgtCard.razor` — surface wrapper with Header/ChildContent/Footer fragments.
5. `AgtAlert.razor` — wraps RadzenAlert with our semantic colors.

`_Imports.razor` in the RCL: Microsoft.AspNetCore.Components(.Forms/.Web), Radzen, Radzen.Blazor, and the Agterhuis.Ui.Components.* namespaces.

## DI extension

`AddAgterhuisUi(this IServiceCollection, Action<AgtUiOptions>? configure = null)`: calls `services.AddRadzenComponents()`, registers options. `AgtUiOptions`: ApplicationName ("Agterhuis"), EnableAnimations (true), DefaultTheme ("light"), DefaultCulture ("nl-NL").

## Demo app

Wire up `AddAgterhuisUi`, load CSS in this order in `wwwroot/index.html` for the standalone WebAssembly host: `_content/Radzen.Blazor/css/material-base.css` → `_content/Agterhuis.Ui/css/agt-theme.css` → `_content/Agterhuis.Ui/css/agt-utilities.css` → app styles; plus `_content/Radzen.Blazor/Radzen.Blazor.min.js` before `blazor.webassembly.js` closes. Pages: `/components/buttons`, `/components/layout`, `/components/feedback` demonstrating default/disabled/busy states, plus a light/dark theme toggle that flips `data-agt-theme` on `<html>`.

## Tests

bUnit + xunit for `AgtPrimaryButton`: renders text, disabled while busy, click callback fires, no click while disabled. `Services.AddRadzenComponents()` in the TestContext ctor. Use plain `Assert.*` (no Shouldly/FluentAssertions).

## NuGet.config (repo root)

nuget.org + Azure Artifacts source placeholder (`https://pkgs.dev.azure.com/ORGANIZATION/PROJECT/_packaging/agterhuis-nuget/nuget/v3/index.json`) with `packageSourceMapping`: `Agterhuis.*` → internal feed; `Microsoft.*`, `System.*`, `Radzen.*`, `bunit`, `xunit*`, `coverlet.*`, `runtime.*` → nuget.org.

## azure-pipelines.yml

Three stages as follows. Validate: UseDotNet@2 (10.0.x), NuGetAuthenticate@1, `dotnet restore --locked-mode`, build (`--no-restore`), test with trx + coverage, PublishTestResults@2. Package: compute version — tags `v*` strip prefix; `main` → `1.0.$(Build.BuildId)`; other branches → `1.1.0-beta.$(Build.BuildId)` — then build + `dotnet pack --no-build -p:PackageVersion=...`, publish pipeline artifact `nuget-packages`. Publish: only on `main` or `v*` tags; NuGetAuthenticate@1 + DotNetCoreCLI@2 push (exclude `*.snupkg`) to `publishVstsFeed: PROJECT/agterhuis-nuget`. Trigger on main/develop + `v*` tags, PR validation on main/develop.

## Verification (must pass before you finish)

1. `dotnet restore` succeeds and lock files are generated.
2. `dotnet build -c Release` with zero warnings (warnings are errors).
3. `dotnet test -c Release` green.
4. `dotnet pack src/Agterhuis.Ui -c Release -o artifacts/packages -p:PackageVersion=0.1.0` produces nupkg + snupkg; inspect the nupkg (it is a zip) and confirm `staticwebassets` content and the `Radzen.Blazor` dependency in the nuspec.
5. `dotnet run` the demo app builds and the buttons page renders (build check is sufficient if you cannot launch a browser).
6. Initialize git, add a `.gitignore` (VisualStudio template + `artifacts/`), and make an initial commit.

Report anything you had to deviate on.
