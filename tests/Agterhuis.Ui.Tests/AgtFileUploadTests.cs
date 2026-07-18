using Agterhuis.Ui.Components.Forms;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtFileUploadTests
{
    [Fact]
    public void RendersDutchDefaultTexts()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtFileUpload>(p => p
            .Add(x => x.Label, "Bestand"));

        Assert.Contains("Bestand kiezen", cut.Markup);
    }

    [Fact]
    public void UsesConfiguredAcceptedExtensions()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtFileUpload>(p => p
            .Add(x => x.Label, "Bestand")
            .Add(x => x.AllowedExtensions, new[] { ".pdf", ".docx" }));

        Assert.Contains(".pdf,.docx", cut.Markup);
    }
}
