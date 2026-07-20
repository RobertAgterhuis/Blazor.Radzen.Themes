using Bunit;
using Radzen;
using Microsoft.Extensions.DependencyInjection;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Demo.Services;
using Microsoft.JSInterop;
using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPageTests
{
    [Fact]
    public void PaletteFilter_FiltersComponentList()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
        var jsRuntime = new DesignerJsRuntimeStub();
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

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
        var jsRuntime = new DesignerJsRuntimeStub();
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

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
        var jsRuntime = new DesignerJsRuntimeStub();
        jsRuntime.SetResult("designerInterop.getText", "draft-json");
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        Assert.Contains("Hersteld werk uit localStorage gevonden", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"agt-command-palette-trigger\"", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Exporteren", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesignerPage_GeneratesEntityForm_FromDataPanelAction()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
        var jsRuntime = new DesignerJsRuntimeStub();
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        cut.Find(".designer-data-panel .rz-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Dossiernummer", cut.Markup, StringComparison.Ordinal);
            Assert.Contains("AgtFormActions", cut.Markup, StringComparison.Ordinal);
        });
    }
}
