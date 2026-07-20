using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Bunit;
using Microsoft.AspNetCore.Components;

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
            .Add(component => component.InitialModelJson, "{\"SchemaVersion\":2,\"Pages\":[{\"Route\":\"/\",\"Title\":\"\",\"Nodes\":[]}]}" )
            .Add(component => component.OnDocumentChanged, EventCallback.Factory.Create<DesignDocument>(ctx, value => received = value)));

        await cut.InvokeAsync(() => cut.Instance.ApplyModelAsync());

        Assert.Null(received);
        Assert.Contains("Page title is required", cut.Markup, StringComparison.Ordinal);
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
            .Add(component => component.InitialModelJson, "{ invalid json }")
            .Add(component => component.OnDocumentChanged, EventCallback.Factory.Create<DesignDocument>(ctx, value => received = value)));

        await cut.InvokeAsync(() => cut.Instance.ApplyModelAsync());

        Assert.Null(received);
        Assert.Contains("JSON-fout:", cut.Markup, StringComparison.Ordinal);
    }
}