# Consuming Agterhuis.Ui

[Docs Index](README.md)

This guide explains how to integrate Agterhuis.Ui into a Blazor app.

## 1. Configure NuGet

For a public clone, nuget.org-only configuration is enough for this repository.

For consumer apps, add the package from your chosen feed (nuget.org or a private feed).

Example minimal `NuGet.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

## 2. Install

```bash
dotnet add package Agterhuis.Ui
```

## 3. Register services

In `Program.cs`:

```csharp
builder.Services.AddAgterhuisUi(options =>
{
    options.DefaultTheme = "plum-dark";
    options.EnableAmbientEffects = true;
});
```

Set `EnableAmbientEffects = false` on data-dense or motion-sensitive screens.

## 4. Host page CSS and JS order

Use this order in your host page:

1. Radzen base CSS
2. `agt-theme.css`
3. `agt-utilities.css`
4. app CSS
5. anti-FOUC inline theme bootstrap script
6. Radzen JS
7. Agterhuis theme interop JS

Example (`wwwroot/index.html` for standalone WebAssembly, or the equivalent host shell in a Blazor Web App):

```html
<head>
  <link rel="stylesheet" href="_content/Radzen.Blazor/css/material-base.css" />
  <link rel="stylesheet" href="_content/Agterhuis.Ui/css/agt-theme.css" />
  <link rel="stylesheet" href="_content/Agterhuis.Ui/css/agt-utilities.css" />
  <link rel="stylesheet" href="app.css" />
  <script>
    (() => {
      const key = "agt-ui-theme";
      const raw = (localStorage.getItem(key) || "plum-dark").trim().toLowerCase();
      const normalized = raw === "dark"
        ? "plum-dark"
        : raw === "light"
          ? "plum-light"
          : raw || "plum-dark";
      document.documentElement.setAttribute("data-agt-theme", normalized);
    })();
  </script>
</head>
<body>
  <script src="_content/Radzen.Blazor/Radzen.Blazor.min.js"></script>
  <script src="_content/Agterhuis.Ui/theme-interop.js"></script>
</body>
```

For a standalone WebAssembly host, place the bootstrap shell in `wwwroot/index.html` and load `_framework/blazor.webassembly.js`. For a Blazor Web App, keep the same asset order in the host shell.

## 5. Import and use components

In `_Imports.razor`:

```razor
@using Agterhuis.Ui
```

Example usage:

```razor
<AgtPrimaryButton Text="Save" />
<AgtPageHeader Title="Dashboard" Subtitle="Team overview" />
```

## 6. Wrapper vs raw Radzen

Use an `Agt*` wrapper when:

- the component is reused in multiple apps, and
- policy enforcement is needed (accessibility contracts, defaults, intent semantics, migration insulation).

Use raw Radzen when:

- the component is niche or highly specialized, or
- a thin passthrough wrapper would add no policy value.

Intentionally raw (still fully themed): Charts, Scheduler, Gantt, PivotDataGrid, HtmlEditor, Tree, DataFilter, GoogleMap, SSRSViewer, QRCode/Barcode, Chat/AIChat.

## 7. Related docs

- [Theming](THEMING.md)
- [Theme coverage](THEME-COVERAGE.md)
- [Accessibility](ACCESSIBILITY.md)

## 8. Starter template

Use the packaged starter to create a Blazor Web App with the theme shell and core patterns already wired:

```bash
dotnet new agterhuis-app -n MijnApp --theme plum --variant dark
```

The template package is [templates/Agterhuis.Ui.Templates](../templates/Agterhuis.Ui.Templates) and includes the expected CSS/JS order, `AddAgterhuisUi()`, a themed `App.razor`, a sidebar shell, and starter pages for forms and lists.
