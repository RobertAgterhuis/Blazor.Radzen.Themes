using Agterhuis.Ui.Components.Layout;
using Agterhuis.Ui.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Agterhuis.Ui.Tests;

public sealed class AgtCommandPaletteTests
{
    [Fact]
    public void TriggerOpensPaletteAndRendersCommandList()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.registerGlobalShortcut", _ => true).SetResult("shortcut-1");
        ctx.JSInterop.Setup<string>("agtTheme.registerFocusTrap", _ => true).SetResult("trap-1");
        ctx.JSInterop.SetupVoid("agtTheme.unregisterGlobalShortcut", _ => true).SetVoidResult();
        ctx.JSInterop.SetupVoid("agtTheme.unregisterFocusTrap", _ => true).SetVoidResult();

        var registry = new AgtCommandRegistry();
        registry.SetCommands("tests", [
            new AgtCommandItem("open-planning", "Ga naar planning", "Navigatie", () => Task.CompletedTask)
            {
                Description = "Open planningsoverzicht"
            }
        ]);

        ctx.Services.AddSingleton<IAgtCommandRegistry>(registry);

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtCommandPalette>(0);
            builder.CloseComponent();
        });

        cut.Find("[data-testid='agt-command-palette-trigger']").Click();

        Assert.Contains("Ga naar planning", cut.Markup);
        Assert.Contains("Open planningsoverzicht", cut.Markup);
    }

    [Fact]
    public void KeyboardEnterExecutesActiveCommand()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.registerGlobalShortcut", _ => true).SetResult("shortcut-1");
        ctx.JSInterop.Setup<string>("agtTheme.registerFocusTrap", _ => true).SetResult("trap-1");
        ctx.JSInterop.SetupVoid("agtTheme.unregisterGlobalShortcut", _ => true).SetVoidResult();
        ctx.JSInterop.SetupVoid("agtTheme.unregisterFocusTrap", _ => true).SetVoidResult();

        var invoked = 0;
        var registry = new AgtCommandRegistry();
        registry.SetCommands("tests", [
            new AgtCommandItem("open-werkorders", "Nieuwe werkorder", "Acties", () =>
            {
                invoked++;
                return Task.CompletedTask;
            })
        ]);

        ctx.Services.AddSingleton<IAgtCommandRegistry>(registry);

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtCommandPalette>(0);
            builder.CloseComponent();
        });
        cut.Find("[data-testid='agt-command-palette-trigger']").Click();

        cut.Find("[data-testid='agt-command-palette-search']")
            .KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(1, invoked);
    }
}
