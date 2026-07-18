using Agterhuis.Ui.Components.Forms;
using Agterhuis.Ui.Components.Layout;
using Agterhuis.Ui.Demo.Components.Layout;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class A11yContractTests
{
    [Fact]
    public void TextField_RendersLabelAssociation()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextField>(p =>
            p.Add(x => x.Label, "Naam")
             .Add(x => x.AriaLabel, "Naam")
             .Add(x => x.Name, "name-input"));

        Assert.Contains("for=\"name-input\"", cut.Markup);
        Assert.Contains("Naam", cut.Markup);
    }

    [Fact]
    public void Dropdown_UsesAriaLabel()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<AgtDropdown<int>>(p =>
            p.Add(x => x.Label, "Optie")
             .Add(x => x.AriaLabel, "Kies optie")
             .Add(x => x.Name, "option-input")
             .Add(x => x.Data, new[] { 1, 2, 3 }.Cast<object>()));

        Assert.Contains("for=\"option-input\"", cut.Markup);
        Assert.Contains("Optie", cut.Markup);
    }

    [Fact]
    public void TextField_InvalidState_SetsAriaDescribedBy()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();

        var cut = ctx.Render<AgtTextField>(p =>
            p.Add(x => x.Label, "Naam")
             .Add(x => x.ValidationMessage, "Naam is verplicht"));

        Assert.Contains("aria-describedby", cut.Markup);
        Assert.Contains("aria-invalid", cut.Markup);
    }

    [Fact]
    public void SkipLink_RendersMainTarget()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<AgtSkipLink>(p =>
            p.Add(x => x.TargetId, "main")
             .Add(x => x.Text, "Naar hoofdinhoud"));

        Assert.Contains("href=\"#main\"", cut.Markup);
        Assert.Contains("Naar hoofdinhoud", cut.Markup);
    }

    [Fact]
    public void MainLayout_ContainsSkipLinkAndMainLandmark()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.getStoredTheme", _ => true).SetResult("plum-dark");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredDensity", _ => true).SetResult("comfortable");
        ctx.JSInterop.Setup<string>("agtTheme.getStoredNavSectionState", _ => true).SetResult("expanded");
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(true);
        ctx.JSInterop.Setup<bool>("agtTheme.isViewportAtMost", _ => true).SetResult(false);

        var options = Microsoft.Extensions.Options.Options.Create(new AgtUiOptions
        {
            DefaultTheme = "plum-dark",
            DefaultDensity = "comfortable"
        });

        ctx.Services.AddSingleton<IOptions<AgtUiOptions>>(options);
        ctx.Services.AddSingleton(new AgtThemeState(options));
        ctx.Services.AddSingleton(new AgtDensityState(options));
        ctx.Services.AddSingleton<IAgtCommandRegistry, AgtCommandRegistry>();

        var cut = ctx.Render<MainLayout>(p =>
            p.Add(x => x.Body, b => b.AddMarkupContent(0, "<h1>Test</h1>")));

        Assert.Contains("Naar hoofdinhoud", cut.Markup);
        Assert.Contains("id=\"main\"", cut.Markup);
        Assert.Contains("aria-label=\"Hoofdnavigatie\"", cut.Markup);
    }
}
