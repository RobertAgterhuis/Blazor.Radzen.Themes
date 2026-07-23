using Agterhuis.Ui.Components.Feedback;
using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Services;
using Agterhuis.Ui.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerFunctionalIntegrityTests
{
    [Fact]
    public async Task DesignerShell_FindingDfi001_SelectedEntityCallbackUpdatesShellState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, new InMemoryDesignStore()));

        var field = typeof(DesignerShell).GetField("_selectedEntityName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        field!.SetValue(cut.Instance, "Schadedossier");

        var method = typeof(DesignerShell).GetMethod("OnSelectedEntityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        var task = method!.Invoke(cut.Instance, ["Klant"]) as Task;
        Assert.NotNull(task);
        await task!;

        var current = field.GetValue(cut.Instance) as string;
        Assert.Equal("Klant", current);
    }

    [Fact]
    public async Task DesignerCanvasNode_FindingDfi002_ColumnResizeKeepsNumericSizeFlow()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        (string ParameterName, string Value)? captured = null;

        var node = new DesignNode
        {
            ComponentType = "RadzenColumn",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Size"] = DesignParameterValue.FromValue("6")
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] = []
            }
        };

        var cut = ctx.Render<DesignerCanvasNode>(parameters => parameters
            .Add(component => component.Node, node)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance)
            .Add(component => component.NodeQuickParameterChanged, EventCallback.Factory.Create<(string ParameterName, string Value)>(this, value => captured = value)));

        var wrapper = cut.Find(".designer-canvas-node");
        var style = wrapper.GetAttribute("style") ?? string.Empty;
        Assert.Contains("50%", style, StringComparison.Ordinal);

        var resizeMethod = cut.Instance.GetType().GetMethod("OnResizeColumnUp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(resizeMethod);

        var resizeTask = resizeMethod!.Invoke(cut.Instance, null) as Task;
        Assert.NotNull(resizeTask);
        await resizeTask!;

        Assert.NotNull(captured);
        Assert.Equal("Size", captured!.Value.ParameterName);
        Assert.Equal("7", captured.Value.Value);
    }

    [Fact]
    public void DesignerCanvasNode_FindingDfi002b_UsesFriendlyPlaceholderFromDesignDataContext()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var dataModel = DesignDataModelSeeder.CreateDefault();
        var context = new DesignDataContext(dataModel);

        var node = new DesignNode
        {
            ComponentType = "AgtTextField",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Label"] = DesignParameterValue.FromValue("Klantnaam"),
                ["AriaLabel"] = DesignParameterValue.FromValue("Klantnaam")
            }
        };

        var cut = ctx.Render<DesignerCanvasNode>(parameters => parameters
            .Add(component => component.Node, node)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance)
            .Add(component => component.DesignDataContext, context));

        Assert.Contains("Bijv. klantnaam...", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesignerCanvasNode_Func002_DoesNotInjectUnsupportedPlaceholderForAgtSwitch()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var node = new DesignNode
        {
            ComponentType = "AgtSwitch",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Label"] = DesignParameterValue.FromValue("Voorbeeld schakelaar"),
                ["AriaLabel"] = DesignParameterValue.FromValue("Voorbeeld schakelaar")
            }
        };

        var cut = ctx.Render<DesignerCanvasNode>(parameters => parameters
            .Add(component => component.Node, node)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance)
            .Add(component => component.DesignDataContext, new DesignDataContext(DesignDataModelSeeder.CreateDefault())));

        var errorNode = cut.FindAll(".designer-canvas-node__error").FirstOrDefault();
        Assert.Null(errorNode);
    }

    [Fact]
    public async Task DesignerCodePanel_FindingDfi003_RazorEditorCodeChangesUpdateDocument()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        DesignDocument? updated = null;

        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance)
            .Add(component => component.OnDocumentChanged, EventCallback.Factory.Create<DesignDocument>(this, value => updated = value)));

        Assert.Contains("Razor editor is bewerkbaar", cut.Markup, StringComparison.Ordinal);

        var targetNode = document.Pages[0].Nodes[0];
        var generated = new Agterhuis.Ui.Designer.CodeGen.RazorCodeGenerator().GeneratePageCode(document.Pages[0], document);
        var currentTitle = targetNode.Parameters["Title"].Literal?.ToString() ?? string.Empty;
        var from = $"@Title=\"{currentTitle}\"";
        var updatedCode = generated.Replace(from, "@Title=\"Gewijzigde titel\"", StringComparison.Ordinal);

        await cut.InvokeAsync(() => cut.Instance.OnCodeEditorChanged(updatedCode));

        Assert.NotNull(updated);
        Assert.Equal("Gewijzigde titel", updated!.Pages[0].Nodes[0].Parameters["Title"].Literal?.ToString());
    }

    [Fact]
    public async Task DesignerShell_FindingDfi004_PageDragOverSetsExplicitIntentState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, new InMemoryDesignStore()));

        var dragStartMethod = typeof(DesignerShell).GetMethod("OnPageDragStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dragOverMethod = typeof(DesignerShell).GetMethod("OnPageDragOver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dragOverField = typeof(DesignerShell).GetField("_pageDragOverIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(dragStartMethod);
        Assert.NotNull(dragOverMethod);
        Assert.NotNull(dragOverField);

        await cut.InvokeAsync(() =>
        {
            dragStartMethod!.Invoke(cut.Instance, [0]);
            dragOverMethod!.Invoke(cut.Instance, [0, new DragEventArgs()]);
        });

        var current = (int?)dragOverField!.GetValue(cut.Instance);
        Assert.Equal(0, current);
    }

    [Fact]
    public async Task DesignerShell_Phase6_PreviewNavigateSelectsMatchingPage()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, new InMemoryDesignStore()));

        var navigateMethod = typeof(DesignerShell).GetMethod("OnPreviewNavigate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var startTemplateMethod = typeof(DesignerShell).GetMethod("OnTemplateStartSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var selectedPageField = typeof(DesignerShell).GetField("_selectedPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandField = typeof(DesignerShell).GetField("_commands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(navigateMethod);
        Assert.NotNull(startTemplateMethod);
        Assert.NotNull(selectedPageField);
        Assert.NotNull(commandField);

        var commandStack = commandField!.GetValue(cut.Instance) as DesignDocumentCommandStack;
        Assert.NotNull(commandStack);

        var startTask = startTemplateMethod!.Invoke(cut.Instance, [DesignDocumentTemplateKind.SidebarApp]) as Task;
        Assert.NotNull(startTask);
        await startTask!;

        var targetRoute = commandStack!.Document.Pages.FirstOrDefault(page => !string.Equals(page.Route, "/", StringComparison.Ordinal))?.Route;
        Assert.False(string.IsNullOrWhiteSpace(targetRoute));

        var expectedIndex = commandStack.Document.Pages
            .Select((page, index) => new { page, index })
            .First(item => string.Equals(item.page.Route, targetRoute, StringComparison.OrdinalIgnoreCase))
            .index;

        var task = navigateMethod!.Invoke(cut.Instance, [targetRoute!]) as Task;
        Assert.NotNull(task);
        await task!;

        var selectedPageIndex = (int?)selectedPageField!.GetValue(cut.Instance);
        Assert.Equal(expectedIndex, selectedPageIndex);
    }

    [Fact]
    public async Task DesignerShell_Phase6_AddPageFromTemplateCreatesAndSelectsNewPage()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, new InMemoryDesignStore()));

        var addMethod = typeof(DesignerShell).GetMethod("OnAddPageFromTemplateAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var selectedPageField = typeof(DesignerShell).GetField("_selectedPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandField = typeof(DesignerShell).GetField("_commands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(addMethod);
        Assert.NotNull(selectedPageField);
        Assert.NotNull(commandField);

        var commandStack = commandField!.GetValue(cut.Instance) as DesignDocumentCommandStack;
        Assert.NotNull(commandStack);
        var beforeCount = commandStack!.Document.Pages.Count;

        var addTask = addMethod!.Invoke(cut.Instance, [DesignDocumentTemplateKind.Dashboard]) as Task;
        Assert.NotNull(addTask);
        await addTask!;

        var afterCount = commandStack.Document.Pages.Count;
        Assert.Equal(beforeCount + 1, afterCount);

        var selectedPageIndex = (int?)selectedPageField!.GetValue(cut.Instance);
        Assert.Equal(afterCount - 1, selectedPageIndex);

        var addedPage = commandStack.Document.Pages[^1];
        Assert.False(string.IsNullOrWhiteSpace(addedPage.Route));
        Assert.True(addedPage.Nodes.Count > 0);
    }

    [Fact]
    public void DesignerShell_Phase6_AddPageTemplateOptionsExcludeBlankAndSidebarApp()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, new InMemoryDesignStore()));

        var property = typeof(DesignerShell).GetProperty("AddPageTemplateOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(property);

        var options = property!.GetValue(cut.Instance) as IReadOnlyList<DesignDocumentTemplates.TemplateDefinition>;
        Assert.NotNull(options);
        Assert.NotEmpty(options!);

        Assert.DoesNotContain(options, option => option.Kind == DesignDocumentTemplateKind.Blank);
        Assert.DoesNotContain(options, option => option.Kind == DesignDocumentTemplateKind.SidebarApp);
        Assert.Contains(options, option => option.Kind == DesignDocumentTemplateKind.Dashboard);
    }

    private sealed class InMemoryDesignStore : IDesignStore
    {
        private readonly DesignDocumentEnvelope _envelope = new(
            "Demo",
            1,
            "etag-1",
            DateTimeOffset.UtcNow,
            DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo"));

        public Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
            => Task.FromResult<IReadOnlyList<DesignListItem>>([new DesignListItem("Demo", DateTimeOffset.UtcNow, 1)]);

        public Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
            => Task.FromResult<DesignDocumentEnvelope?>(_envelope);

        public Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
            => Task.FromResult(_envelope);

        public Task RemoveAsync(string name)
            => Task.CompletedTask;

        public Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
            => Task.FromResult<IReadOnlyList<DesignVersionInfo>>([new DesignVersionInfo(1, DateTimeOffset.UtcNow, 1024)]);

        public Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
            => Task.FromResult<DesignDocumentEnvelope?>(_envelope);
    }

    private sealed class ConfirmDialogStub : IAgtConfirmDialog
    {
        public Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null)
            => Task.FromResult(true);

        public Task<bool> ConfirmDeleteAsync(string itemName)
            => Task.FromResult(true);
    }
}