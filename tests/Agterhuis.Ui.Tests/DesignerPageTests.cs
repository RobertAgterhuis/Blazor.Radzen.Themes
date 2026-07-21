using Bunit;
using Radzen;
using Microsoft.Extensions.DependencyInjection;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Designer.Persistence;
using Microsoft.JSInterop;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPageTests
{
    private static void RegisterDesignerServices(BunitContext ctx, DesignerJsRuntimeStub jsRuntime)
    {
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<IAgtCommandRegistry>(_ => new AgtCommandRegistry());
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        var localStore = new LocalDesignStore(jsRuntime);
        ctx.Services.AddSingleton(localStore);
        ctx.Services.AddSingleton<IDesignStore>(localStore);
        ctx.Services.AddSingleton(DesignerComponentRegistry.Instance);
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new AlwaysConfirmDialog());
    }

    [Fact]
    public void PaletteFilter_FiltersComponentList()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

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
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-canvas-node__select").Click();

        cut.WaitForAssertion(() => Assert.DoesNotContain("Selecteer een node", cut.Find(".designer-breadcrumb").TextContent, StringComparison.Ordinal));
    }

    [Fact]
    public void DesignerPage_RendersRecoveryBannerAndCommandPalette()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        jsRuntime.SetResult("designerInterop.getText", "draft-json");
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        Assert.Contains("Hersteld werk uit localStorage gevonden", cut.Markup, StringComparison.Ordinal);
        cut.FindAll(".designer-menu-toggle")
            .First(button => button.TextContent.Contains("Instellingen", StringComparison.Ordinal))
            .Click();
        Assert.Contains("data-testid=\"agt-command-palette-trigger\"", cut.Markup, StringComparison.Ordinal);

        cut.FindAll(".designer-menu-toggle")
            .First(button => button.TextContent.Contains("Bestand", StringComparison.Ordinal))
            .Click();
        Assert.Contains("Exporteren", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesignerPage_GeneratesEntityForm_FromDataPanelAction()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        cut.Find(".designer-panel--data .designer-panel__toggle").Click();
        cut.Find(".designer-data-panel .rz-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Dossiernummer", cut.Markup, StringComparison.Ordinal);
            Assert.Contains("AgtFormActions", cut.Markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void DesignerPage_RendersCodeWorkbench()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();

        cut.Find(".designer-panel--code .designer-panel__toggle").Click();

        Assert.Contains("Razor (preview)", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Model (JSON)", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesignerPage_EscapeClearsSelection()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-canvas-node__select").Click();

        cut.Find(".designer-page").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cut.WaitForAssertion(() => Assert.Contains("Selecteer een node", cut.Find(".designer-breadcrumb").TextContent, StringComparison.Ordinal));
    }

    [Fact]
    public void DesignerPage_DeleteRemovesSelectedNode()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.Find(".designer-canvas-node__select").Click();

        var nodesBefore = cut.FindAll(".designer-canvas-node").Count;
        cut.Find(".designer-page").KeyDown(new KeyboardEventArgs { Key = "Delete" });

        cut.WaitForAssertion(() => Assert.True(cut.FindAll(".designer-canvas-node").Count < nodesBefore));
    }

    [Fact]
    public void DesignerPage_ArrowDownSelectsNextSibling()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        document.Pages[0].Nodes.Add(new DesignNode
        {
            ComponentType = "AgtEmptyState",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Title"] = DesignParameterValue.FromValue("Tweede root"),
                ["Description"] = DesignParameterValue.FromValue("Tweede sibling")
            }
        });
        var json = System.Text.Json.JsonSerializer.Serialize(document, Agterhuis.Ui.Designer.Serialization.DesignJsonOptions.Default);
        jsRuntime.SetResult("designerInterop.getText", json);

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("name", document.Name));

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.FindAll(".designer-canvas-node__select")[0].Click();
        var firstBreadcrumb = cut.Find(".designer-breadcrumb").TextContent;

        cut.Find(".designer-page").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        cut.WaitForAssertion(() => Assert.NotEqual(firstBreadcrumb, cut.Find(".designer-breadcrumb").TextContent));
    }

    [Fact]
    public void DesignerPage_PageTabsSwitchActivePageCanvasAndProperties()
    {
        using var ctx = new BunitContext();
        var jsRuntime = new DesignerJsRuntimeStub();
        RegisterDesignerServices(ctx, jsRuntime);

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Multi");
        document.Pages.Add(new DesignPage
        {
            Route = "/tweede",
            Title = "Tweede",
            Nodes =
            [
                new DesignNode
                {
                    ComponentType = "AgtEmptyState",
                    Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                    {
                        ["Title"] = DesignParameterValue.FromValue("Tweede pagina"),
                        ["Description"] = DesignParameterValue.FromValue("Content")
                    }
                }
            ]
        });

        var json = System.Text.Json.JsonSerializer.Serialize(document, Agterhuis.Ui.Designer.Serialization.DesignJsonOptions.Default);
        jsRuntime.SetResult("designerInterop.getText", json);

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("name", document.Name));

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.Designer>();
        cut.FindAll(".designer-page-tab__button")[1].Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Tweede pagina", cut.Markup, StringComparison.Ordinal);
            Assert.Contains("/tweede", cut.Markup, StringComparison.Ordinal);
        });
    }

    private sealed class AlwaysConfirmDialog : IAgtConfirmDialog
    {
        public Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null)
            => Task.FromResult(true);

        public Task<bool> ConfirmDeleteAsync(string itemName)
            => Task.FromResult(true);
    }
}
