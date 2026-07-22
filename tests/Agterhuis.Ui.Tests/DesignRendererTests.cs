using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Services;
using Agterhuis.Ui.Demo.Components.Pages;
using Bunit;
using Radzen;
using Radzen.Blazor;

namespace Agterhuis.Ui.Tests;

public sealed class DesignRendererTests
{
    [Fact]
    public void DesignRenderer_RendersFormActionsComponentStructureInOrder()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var realPage = ctx.Render<FormActionsDemo>();
        var pageHeaderIndex = realPage.Markup.IndexOf("agt-page-header", StringComparison.Ordinal);
        var cardIndex = realPage.Markup.IndexOf("agt-card", StringComparison.Ordinal);
        var actionsIndex = realPage.Markup.IndexOf("agt-form-actions", StringComparison.Ordinal);

        Assert.True(pageHeaderIndex >= 0 && pageHeaderIndex < cardIndex && cardIndex < actionsIndex);

        var model = new DesignPage
        {
            Route = "/components/forms/form-actions",
            Title = "Form actions",
            Nodes =
            [
                new DesignNode
                {
                    ComponentType = "AgtPageHeader",
                    Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                    {
                        ["Title"] = DesignParameterValue.FromValue("AgtFormActions"),
                        ["Description"] = DesignParameterValue.FromValue("Rechts uitgelijnde save/cancel rij.")
                    }
                },
                new DesignNode
                {
                    ComponentType = "AgtCard",
                    Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                    {
                        ["ChildContent"] =
                        [
                            new DesignNode
                            {
                                ComponentType = "AgtFormActions",
                                Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                                {
                                    ["SaveText"] = DesignParameterValue.FromValue("Opslaan"),
                                    ["CancelText"] = DesignParameterValue.FromValue("Annuleren"),
                                    ["IsBusy"] = DesignParameterValue.FromValue(false)
                                }
                            }
                        ]
                    }
                }
            ]
        };

        var cut = ctx.Render<DesignRenderer>(parameters => parameters.Add(static component => component.Page, model));
        var renderedOrder = cut.FindAll("[data-agt-design-component]")
            .Select(static element => element.GetAttribute("data-agt-design-component") ?? string.Empty)
            .ToArray();

        Assert.Equal(["AgtPageHeader", "AgtCard", "AgtFormActions"], renderedOrder);
    }

    [Fact]
    public void DesignPreviewRenderer_ResolvesScalarBindingToSampleValue()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var model = DesignDataModelSeeder.CreateDefault();
        var dataContext = new DesignDataContext(model);
        var page = new DesignPage
        {
            Route = "/preview",
            Title = "Preview",
            Nodes =
            [
                new DesignNode
                {
                    ComponentType = "AgtTextField",
                    Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                    {
                        ["AriaLabel"] = DesignParameterValue.FromValue("Klantnaam"),
                        ["Label"] = DesignParameterValue.FromValue("Klantnaam"),
                        ["Value"] = new DesignParameterValue { Expression = "@entities.Klant.Select(item => item.Klantnaam)" }
                    }
                }
            ]
        };

        var cut = ctx.Render<DesignPreviewRenderer>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.DataContext, dataContext));

        Assert.Contains("Klant 1", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesignPreviewRenderer_BindsSeedRowsToDataGrid()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var model = DesignDataModelSeeder.CreateDefault();
        var dataContext = new DesignDataContext(model);
        var page = new DesignPage
        {
            Route = "/preview-grid",
            Title = "Preview grid",
            Nodes =
            [
                new DesignNode
                {
                    ComponentType = "RadzenDataGrid",
                    Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                    {
                        ["Data"] = new DesignParameterValue { Expression = "@entities.Schadedossier" }
                    },
                    Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                    {
                        ["Columns"] =
                        [
                            new DesignNode
                            {
                                ComponentType = "RadzenDataGridColumn",
                                Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                                {
                                    ["Property"] = DesignParameterValue.FromValue("Dossiernummer"),
                                    ["Title"] = DesignParameterValue.FromValue("Dossier")
                                }
                            }
                        ]
                    }
                }
            ]
        };

        var cut = ctx.Render<DesignPreviewRenderer>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.DataContext, dataContext));

        var grid = cut.FindComponent<RadzenDataGrid<object>>();
        var data = grid.Instance.Data?.Cast<object>().ToArray() ?? [];

        Assert.NotEmpty(data);
        var first = Assert.IsAssignableFrom<IDictionary<string, object?>>(data[0]);
        Assert.Equal("ATG-2024-00001", first["Dossiernummer"]?.ToString());
    }
}