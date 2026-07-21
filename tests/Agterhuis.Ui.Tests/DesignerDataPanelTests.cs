using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerDataPanelTests
{
    [Fact]
    public void ImportPreview_ShowsDerivedFieldsBeforeApply()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var model = DesignDataModelSeeder.CreateDefault();

        var cut = ctx.Render<DesignerDataPanel>(parameters => parameters
            .Add(component => component.DataModel, model)
            .Add(component => component.SelectedEntityName, model.Entities[0].Name));

        cut.FindAll("button").First(button => button.TextContent.Contains("Importeer schema", StringComparison.Ordinal)).Click();
        var textarea = cut.Find("textarea");
        textarea.Change("""
        {
          "title": "ImportEntity",
          "type": "object",
          "properties": {
            "Naam": { "type": "string" },
            "Aantal": { "type": "integer" }
          }
        }
        """);

        cut.FindAll("button").First(button => button.TextContent.Contains("Voorbeeld laden", StringComparison.Ordinal)).Click();

        Assert.Contains("ImportEntity", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Naam", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Aantal", cut.Markup, StringComparison.Ordinal);
    }
}
