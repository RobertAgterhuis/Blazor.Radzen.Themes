using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Demo.Components.Pages;
using Bunit;
using Radzen;

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
}