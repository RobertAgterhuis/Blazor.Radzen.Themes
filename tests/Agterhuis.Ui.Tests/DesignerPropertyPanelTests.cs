using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;
using Agterhuis.Ui.Demo.Components.Designer;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPropertyPanelTests
{
    public static TheoryData<string, string, string> ParameterToEditorMapping
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            data.Add("Title", "string", "String");
            data.Add("Size", "int", "Numeric");
            data.Add("Visible", "bool", "Boolean");
            data.Add("Variant", "Variant", "Enum");
            data.Add("Date", "DateTime", "DateTime");
            data.Add("BackgroundColor", "string", "ColorToken");
            data.Add("Click", "EventCallback", "EventCallback");
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ParameterToEditorMapping))]
    public void PropertyPanel_MapsKnownParameterTypes_ToExpectedEditors(string parameterName, string typeName, string expectedEditor)
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor(new[]
        {
            CreateParameter(parameterName, typeName)
        });

        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                [parameterName] = DesignParameterValue.FromValue("seed")
            }
        };

        var page = new DesignPage { Route = "/", Title = "Page" };

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark", "plum-light" })
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        var editor = cut.Find($"[data-agt-designer-param='{parameterName}']");
        Assert.Equal(expectedEditor, editor.GetAttribute("data-agt-designer-editor-kind"));
    }

    [Fact]
    public void PropertyPanel_ShowsA11yWarning_WhenLabelAndAriaLabelAreMissing()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor(
        [
            CreateParameter("Label", "string"),
            CreateParameter("AriaLabel", "string")
        ]);

        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
        };

        var page = new DesignPage { Route = "/", Title = "Page" };

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        Assert.Contains("Toegankelijkheidswaarschuwing", cut.Markup, StringComparison.Ordinal);

        node.Parameters["Label"] = DesignParameterValue.FromValue("Naam");
        node.Parameters["AriaLabel"] = DesignParameterValue.FromValue("Naamveld");

        cut.Render(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        Assert.DoesNotContain("Toegankelijkheidswaarschuwing", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyPanel_ShowsResetAndModifiedBadge_WhenParameterHasCustomValue()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor([CreateParameter("Title", "string", defaultValue: "Default title")]);

        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Title"] = DesignParameterValue.FromValue("Custom title")
            }
        };

        var page = new DesignPage { Route = "/", Title = "Page" };

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        var titleField = cut.Find("[data-agt-designer-param='Title']");
        Assert.Contains("Gewijzigd", titleField.TextContent, StringComparison.Ordinal);

        var resetButton = titleField.QuerySelector("button");
        Assert.NotNull(resetButton);
        Assert.False(resetButton!.HasAttribute("disabled"));
    }

    [Fact]
    public void PropertyPanel_ShowsBindableBadge_WhenParameterIsBindable()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor([CreateParameter("Data", "string", isBindable: true)]);

        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Data"] = DesignParameterValue.FromValue("Schadedossiers")
            }
        };

        var page = new DesignPage { Route = "/", Title = "Page" };

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        var dataField = cut.Find("[data-agt-designer-param='Data']");
        Assert.Contains("Bindbaar", dataField.TextContent, StringComparison.Ordinal);
    }

    private static DesignerComponentDescriptor CreateDescriptor(IReadOnlyList<ComponentParameterDescriptor> parameters)
        => new(
            "AgtTest",
            typeof(Agterhuis.Ui.Components.Layout.AgtCard),
            "Test",
            "Forms & Inputs",
            "extension",
            true,
            true,
            [],
            parameters);

    private static ComponentParameterDescriptor CreateParameter(string name, string typeName, string defaultValue = "null", bool isBindable = false)
    {
        var parameterType = typeName switch
        {
            "string" => typeof(string),
            "int" => typeof(int),
            "bool" => typeof(bool),
            "DateTime" => typeof(DateTime),
            "Variant" => typeof(Radzen.Variant),
            "EventCallback" => typeof(Microsoft.AspNetCore.Components.EventCallback),
            _ => typeof(object)
        };

        var isEventCallback = parameterType == typeof(Microsoft.AspNetCore.Components.EventCallback);

        return new ComponentParameterDescriptor(
            name,
            typeName,
            parameterType,
            defaultValue,
            "-",
            isBindable,
            false,
            isEventCallback,
            false,
            false);
    }
}
