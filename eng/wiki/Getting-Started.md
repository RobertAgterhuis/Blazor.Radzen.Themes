# Getting Started

> Summary page. Canonical source: docs/CONSUMING.md.

## Install

```bash
dotnet add package Agterhuis.Ui
```

## Register

```csharp
builder.Services.AddAgterhuisUi();
```

## Host page order

1. Radzen base CSS
2. agt-theme.css
3. agt-utilities.css
4. app.css
5. anti-FOUC inline theme bootstrap
6. Radzen.Blazor.min.js
7. theme-interop.js

## Canonical Reference

- https://github.com/RobertAgterhuis/Blazor.Radzen.Themes/blob/main/docs/CONSUMING.md
