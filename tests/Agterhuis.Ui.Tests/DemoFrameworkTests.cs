using Agterhuis.Ui.Components.Forms;
using Agterhuis.Ui.Demo.Components.Demo;
using Agterhuis.Ui.Demo.Components.Pages.Catalog.Examples.TextBox;
using Agterhuis.Ui.Demo.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class DemoFrameworkTests
{
    [Fact]
    public void DemoExample_RendersPreviewAndCodeTabs()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<DemoSourceProvider>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<DemoExample>(parameters => parameters
            .Add(x => x.Title, "Basis")
            .Add(x => x.Description, "Beschrijving")
            .Add(x => x.ExampleComponentType, typeof(TextBoxBasicExample))
            .Add(x => x.SourcePath, "Components/Pages/Catalog/Examples/TextBox/TextBoxBasicExample.razor"));

        Assert.Contains("Voorbeeld", cut.Markup);
        Assert.Contains("Beschrijving", cut.Markup);

        cut.FindAll("button")
            .Single(button => button.TextContent.Contains("Code", StringComparison.Ordinal))
            .Click();

        Assert.Contains("Kopieer code", cut.Markup);
        Assert.Contains("RadzenTextBox", cut.Markup);
    }

    [Fact]
    public void ComponentParameterTable_RendersRowsForKnownWrapper()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<ComponentParameterTable>(parameters =>
            parameters.Add(x => x.ComponentType, typeof(AgtTextField)));

        Assert.Contains("Naam", cut.Markup);
        Assert.Contains("Value", cut.Markup);
        Assert.Contains("Label", cut.Markup);
    }

    [Fact]
    public void DemoExample_ThrowsWhenNestedExampleCrashes()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<DemoSourceProvider>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ctx.Render<DemoExample>(parameters => parameters
                .Add(x => x.Title, "Crash")
                .Add(x => x.ExampleComponentType, typeof(ThrowingExampleComponent))
                .Add(x => x.SourcePath, "Components/Pages/Catalog/Examples/TextBox/TextBoxBasicExample.razor")));

        Assert.Contains("Voorbeeld-crash", ex.Message);
    }

    private sealed class ThrowingExampleComponent : ComponentBase
    {
        protected override void OnInitialized()
        {
            throw new InvalidOperationException("Voorbeeld-crash");
        }
    }
}
