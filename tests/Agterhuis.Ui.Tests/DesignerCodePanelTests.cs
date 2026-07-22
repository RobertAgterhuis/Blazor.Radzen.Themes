using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Bunit;
using Microsoft.AspNetCore.Components;
using Bunit.JSInterop;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerCodePanelTests
{
    [Fact]
    public async Task ValidateAndApplyModel_RejectsInvalidDocumentAndPreservesLastValidState()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        var received = (DesignDocument?)null;

        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.InitialTab, "model")
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.InitialModelJson, "{\"SchemaVersion\":2,\"Pages\":[{\"Route\":\"/\",\"Title\":\"\",\"Nodes\":[]}]}" )
            .Add(component => component.OnDocumentChanged, EventCallback.Factory.Create<DesignDocument>(ctx, value => received = value)));

        await cut.InvokeAsync(() => cut.Instance.ApplyModelAsync());

        Assert.Null(received);
        Assert.Contains("Page title is verplicht", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("<textarea", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyModelAsync_ShowsJsonError_ForInvalidJson()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        var received = (DesignDocument?)null;

        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.InitialTab, "model")
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.InitialModelJson, "{ invalid json }")
            .Add(component => component.OnDocumentChanged, EventCallback.Factory.Create<DesignDocument>(ctx, value => received = value)));

        await cut.InvokeAsync(() => cut.Instance.ApplyModelAsync());

        Assert.Null(received);
        Assert.Contains("JSON-fout:", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void CodeTab_HighlightsSelectedNodeBlock()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        var selectedNodeId = document.Pages[0].Nodes[0].Id;

        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.InitialTab, "code")
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.SelectedNodeId, selectedNodeId));

        Assert.Contains("agt-code-line--highlight", cut.Markup, StringComparison.Ordinal);
        Assert.Contains($"agt-node:{selectedNodeId}", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeEditor_ReportsUnsupportedRazorSyntax_WithLineDiagnostic()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance));

        await cut.InvokeAsync(() => cut.Instance.OnCodeEditorChanged("@if(true){<div/>}"));

        Assert.Contains("niet roundtrippable", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CodeEditor_ReportsNestedSlotMismatch_WithLineDiagnostic()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Demo");
        var cut = ctx.Render<DesignerCodePanel>(parameters => parameters
            .Add(component => component.CurrentPage, document.Pages[0])
            .Add(component => component.Document, document)
            .Add(component => component.EnableMonaco, false)
            .Add(component => component.Registry, Agterhuis.Ui.Designer.Registry.DesignerComponentRegistry.Instance));

        await cut.InvokeAsync(() => cut.Instance.OnCodeEditorChanged("<ChildContent>\n<Columns>\n</ChildContent>"));

        Assert.Contains("matcht niet", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}