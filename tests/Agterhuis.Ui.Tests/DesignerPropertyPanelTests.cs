using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Registry;
using Agterhuis.Ui.Designer.Components;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerPropertyPanelTests
{
    public static TheoryData<string, string, string> ParameterToEditorMapping
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            data.Add("Title", "string", "String");
            data.Add("Value", "int", "Numeric");
            data.Add("Value", "bool", "Boolean");
            data.Add("ButtonStyle", "Variant", "Enum");
            data.Add("Value", "DateTime", "DateTime");
            data.Add("Placeholder", "string", "String");
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
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        var editor = cut.Find($"[data-agt-designer-param='{parameterName}']");
        Assert.Equal(expectedEditor, editor.GetAttribute("data-agt-designer-editor-kind"));
    }

    [Fact]
    public async Task PropertyPanel_ShowsEventCallbackParameters_AndAddsDefaultHandler()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor([CreateParameter("Click", "EventCallback")]);
        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
        };

        (ComponentParameterDescriptor Parameter, DesignParameterValue? Value)? captured = null;

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, new DesignPage { Route = "/", Title = "Page" })
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor)
            .Add(component => component.SetNodeParameter, EventCallback.Factory.Create<(ComponentParameterDescriptor Parameter, DesignParameterValue? Value)>(this, value => captured = value)));

        cut.Find("button[role='tab']:nth-of-type(2)").Click();

        var eventField = cut.Find("[data-agt-designer-param='Click']");
        Assert.Equal("EventCallback", eventField.GetAttribute("data-agt-designer-editor-kind"));

        var addButton = cut.FindAll("button").First(button => button.TextContent.Contains("Handler toevoegen", StringComparison.Ordinal));
        addButton.Click();
        await cut.InvokeAsync(() => Task.CompletedTask);

        Assert.NotNull(captured);
        Assert.NotNull(captured!.Value.Value);
        Assert.Equal("OnTestClick", captured.Value.Value!.EventHandlerName);
    }

    [Fact]
    public async Task PropertyPanel_EventHandlerValueChanged_AndRemove_InvokeSetNodeParameter()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = CreateDescriptor([CreateParameter("Click", "EventCallback")]);
        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "AgtTest",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Click"] = new DesignParameterValue
                {
                    EventHandlerName = "OnTestClick"
                }
            }
        };

        var invocations = new List<(ComponentParameterDescriptor Parameter, DesignParameterValue? Value)>();
        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, new DesignPage { Route = "/", Title = "Page" })
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor)
            .Add(component => component.SetNodeParameter, EventCallback.Factory.Create<(ComponentParameterDescriptor Parameter, DesignParameterValue? Value)>(this, value => invocations.Add(value))));

        cut.Find("button[role='tab']:nth-of-type(2)").Click();

        var method = cut.Instance.GetType()
            .GetMethod("SetEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        var updateTask = method!.Invoke(cut.Instance, [descriptor.Parameters[0], "OnSaveClicked"]) as Task;
        Assert.NotNull(updateTask);
        await updateTask!;

        Assert.NotEmpty(invocations);
        Assert.Equal("OnSaveClicked", invocations[^1].Value?.EventHandlerName);

        var removeMethod = cut.Instance.GetType()
            .GetMethod("RemoveEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(removeMethod);
        var removeTask = removeMethod!.Invoke(cut.Instance, [descriptor.Parameters[0]]) as Task;
        Assert.NotNull(removeTask);
        await removeTask!;

        Assert.Null(invocations[^1].Value);
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
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        Assert.Contains("Toegankelijkheidswaarschuwing", cut.Markup, StringComparison.Ordinal);

        node.Parameters["Label"] = DesignParameterValue.FromValue("Naam");
        node.Parameters["AriaLabel"] = DesignParameterValue.FromValue("Naamveld");

        cut.Render(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
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
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
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
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        var dataField = cut.Find("[data-agt-designer-param='Data']");
        Assert.Contains("Bindbaar", dataField.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyPanel_ShowsColumnsEditor_ForGridNodeWithColumnsSlot()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var descriptor = new DesignerComponentDescriptor(
            "AgtDataGrid",
            typeof(Agterhuis.Ui.Components.Data.AgtDataGrid<object>),
            "Data Grid",
            "Data & Scheduling",
            "extension",
            true,
            true,
            false,
            ["Columns"],
            []);

        var node = new DesignNode
        {
            Id = "grid-1",
            ComponentType = "AgtDataGrid",
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["Columns"] =
                [
                    new DesignNode
                    {
                        Id = "column-1",
                        ComponentType = "RadzenDataGridColumn",
                        Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        {
                            ["Title"] = DesignParameterValue.FromValue("Dossiernummer")
                        }
                    }
                ]
            }
        };

        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, new DesignPage { Route = "/", Title = "Page" })
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, DesignDataModelSeeder.CreateDefault())
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor));

        cut.Find("button[role='tab']:nth-of-type(3)").Click();

        Assert.NotNull(cut.Find("[data-agt-designer-columns-editor='true']"));
        Assert.Contains("Dossiernummer", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropertyPanel_BindingPicker_SavesExpression()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var model = DesignDataModelSeeder.CreateDefault();
        var descriptor = CreateDescriptor([CreateParameter("Data", "IEnumerable", isBindable: true)]);
        var node = new DesignNode
        {
            Id = "node-1",
            ComponentType = "RadzenDataGrid",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
        };

        (ComponentParameterDescriptor Parameter, DesignParameterValue? Value)? captured = null;
        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, new DesignPage { Route = "/", Title = "Page" })
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, model)
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor)
            .Add(component => component.SetNodeParameter, EventCallback.Factory.Create<(ComponentParameterDescriptor Parameter, DesignParameterValue? Value)>(this, value => captured = value)));

        var method = cut.Instance.GetType()
            .GetMethod("OnBindingEntityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        var task = method!.Invoke(cut.Instance, [descriptor.Parameters[0], model.Entities[0].Name]) as Task;
        Assert.NotNull(task);
        await task!;

        Assert.NotNull(captured);
        Assert.NotNull(captured!.Value.Value);
        Assert.StartsWith("@entities.", captured.Value.Value!.Expression, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyPanel_DataGridColumns_ApplyInvokesCallback()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var model = DesignDataModelSeeder.CreateDefault();
        var entity = model.Entities[0];
        var descriptor = new DesignerComponentDescriptor(
            "RadzenDataGrid",
            typeof(Radzen.Blazor.RadzenDataGrid<object>),
            "Data Grid",
            "Data",
            "table_rows",
            true,
            false,
            false,
            ["Columns"],
            [CreateParameter("Data", "IEnumerable", isBindable: true)]);

        var node = new DesignNode
        {
            Id = "grid-1",
            ComponentType = "RadzenDataGrid",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Data"] = new DesignParameterValue { Expression = $"@entities.{entity.Name}" }
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["Columns"] = []
            }
        };

        IReadOnlyList<DataGridColumnConfig>? capturedColumns = null;
        var cut = ctx.Render<PropertyPanel>(parameters => parameters
            .Add(component => component.Page, new DesignPage { Route = "/", Title = "Page" })
            .Add(component => component.CanvasTheme, "plum-dark")
            .Add(component => component.ThemeOptions, new[] { "plum-dark" })
            .Add(component => component.DataModel, model)
            .Add(component => component.SelectedNode, node)
            .Add(component => component.SelectedDescriptor, descriptor)
            .Add(component => component.UpsertColumnNodes, EventCallback.Factory.Create<IReadOnlyList<DataGridColumnConfig>>(this, value => capturedColumns = value))
            .Add(component => component.SetDataGridPaging, EventCallback.Factory.Create<DataGridPagingConfig>(this, static _ => { })));

        cut.FindAll("button").First(button => button.TextContent.Contains("Alle selecteren", StringComparison.Ordinal)).Click();
        cut.FindAll("button").First(button => button.TextContent.Contains("Kolommen toepassen", StringComparison.Ordinal)).Click();

        Assert.NotNull(capturedColumns);
        Assert.True(capturedColumns!.Count > 0);
        Assert.All(capturedColumns, static column => Assert.True(!string.IsNullOrWhiteSpace(column.FieldName)));
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
            false,
            [],
            parameters);

    private static ComponentParameterDescriptor CreateParameter(string name, string typeName, string defaultValue = "null", bool isBindable = false)
    {
        var parameterType = typeName switch
        {
            "string" => typeof(string),
            "int" => typeof(int),
            "IEnumerable" => typeof(IEnumerable<object>),
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
