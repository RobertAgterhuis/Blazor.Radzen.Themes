# Designer consuming guide

Use the Designer RCL as a package-level UI surface in any Blazor app that needs the designer.

## Package reference

Reference the `Agterhuis.Ui.Designer` project or NuGet package from the consuming app. The package brings the designer shell, start screen, and static web assets.

## Service registration

Register the shared Agterhuis UI services and the designer services in `Program.cs`:

```csharp
using Agterhuis.Ui.Designer.Extensions;

builder.Services.AddDesigner();
```

`AddDesigner()` wires the shared Agterhuis UI services that the designer shell expects.

## Store implementation

Provide an `IDesignStore` implementation in the consuming app. The shell and start screen use it for recent documents, open/save, and template-backed persistence.

Required members:

- `GetRecentNamesAsync()`
- `LoadAsync(string name)`
- `SaveAsync(string name, DesignDocument document)`
- `RemoveAsync(string name)`

## Rendering the designer

Place `<DesignerShell>` on a page and pass the store plus the registry:

```razor
@using Agterhuis.Ui.Designer.Components
@using Agterhuis.Ui.Designer.Persistence
@using Agterhuis.Ui.Designer.Registry

<DesignerShell Store="@DesignStore"
               Registry="@Registry"
               DefaultCanvasTheme="plum-dark" />
```

The shell owns the full designer experience: toolbar, palette, canvas, properties, data, tree, and code panels.

## Static assets

The designer JavaScript is served from the RCL static web asset path:

- `_content/Agterhuis.Ui.Designer/designer-interop.js`

The interop script loads the resize helper from the same package path, so consumers do not need to copy either file into their app.

## Notes

- Register the theme and Agterhuis UI shared services before rendering the designer page.
- If your app hosts the designer inside another layout, make sure the page can load the package static assets before the shell renders.
