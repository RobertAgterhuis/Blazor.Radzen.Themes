using Bunit;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPageTests
{
    [Fact]
    public void PaletteFilter_FiltersComponentList()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("designerInterop.registerKeyScope", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<List<string>>("designerInterop.getJson", _ => true).SetResult([]);
        ctx.JSInterop.SetupVoid("designerInterop.setJson", _ => true).SetVoidResult();

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-filter").Input("Accordion");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Accordion", cut.Markup, StringComparison.Ordinal);
            Assert.DoesNotContain("DataGrid", cut.Markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void TreeSelection_SyncsToBreadcrumb()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("designerInterop.registerKeyScope", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<List<string>>("designerInterop.getJson", _ => true).SetResult([]);
        ctx.JSInterop.SetupVoid("designerInterop.setJson", _ => true).SetVoidResult();

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-canvas-node__select").Click();

        cut.WaitForAssertion(() => Assert.DoesNotContain("Selecteer een node", cut.Find(".designer-breadcrumb").TextContent, StringComparison.Ordinal));
    }
}
