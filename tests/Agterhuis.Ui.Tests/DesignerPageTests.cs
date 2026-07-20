using Bunit;
using Radzen;
using Microsoft.Extensions.DependencyInjection;
using Agterhuis.Ui.Services;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPageTests
{
    [Fact]
    public void PaletteFilter_FiltersComponentList()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
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
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("designerInterop.registerKeyScope", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<List<string>>("designerInterop.getJson", _ => true).SetResult([]);
        ctx.JSInterop.SetupVoid("designerInterop.setJson", _ => true).SetVoidResult();

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-canvas-node__select").Click();

        cut.WaitForAssertion(() => Assert.DoesNotContain("Selecteer een node", cut.Find(".designer-breadcrumb").TextContent, StringComparison.Ordinal));
    }

    [Fact]
    public void DesignerPage_RendersRecoveryBannerAndCommandPalette()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("designerInterop.registerKeyScope", _ => true).SetVoidResult();
        ctx.JSInterop.Setup<List<string>>("designerInterop.getJson", _ => true).SetResult([]);
        ctx.JSInterop.Setup<string>("designerInterop.getText", _ => true).SetResult("draft-json");
        ctx.JSInterop.SetupVoid("designerInterop.setJson", _ => true).SetVoidResult();
        ctx.JSInterop.SetupVoid("designerInterop.removeItem", _ => true).SetVoidResult();
        ctx.JSInterop.SetupVoid("designerInterop.saveBytesFile", _ => true).SetVoidResult();

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        Assert.Contains("Hersteld werk uit localStorage gevonden", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"agt-command-palette-trigger\"", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Exporteren", cut.Markup, StringComparison.Ordinal);
    }
}
