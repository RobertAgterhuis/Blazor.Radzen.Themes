using Agterhuis.Ui.Components.Layout;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Tests;

public sealed class AgtDrawerTests
{
    [Fact]
    public void OpenDrawerRegistersFocusTrap()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.registerFocusTrap", _ => true).SetResult("trap-1");
        ctx.JSInterop.SetupVoid("agtTheme.unregisterFocusTrap", _ => true).SetVoidResult();

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtDrawer>(0);
            builder.AddAttribute(1, nameof(AgtDrawer.IsOpen), true);
            builder.AddAttribute(2, nameof(AgtDrawer.Title), "Details");
            builder.AddAttribute(3, nameof(AgtDrawer.ChildContent), (RenderFragment)(_ => { }));
            builder.CloseComponent();
        });

        Assert.Contains("agt-drawer", cut.Markup);
        Assert.Contains("Details", cut.Markup);
    }

    [Fact]
    public void EscapeClosesDrawerViaBinding()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("agtTheme.registerFocusTrap", _ => true).SetResult("trap-1");
        ctx.JSInterop.SetupVoid("agtTheme.unregisterFocusTrap", _ => true).SetVoidResult();

        var open = true;

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<AgtDrawer>(0);
            builder.AddAttribute(1, nameof(AgtDrawer.IsOpen), open);
            builder.AddAttribute(2, nameof(AgtDrawer.IsOpenChanged), EventCallback.Factory.Create<bool>(this, value => open = value));
            builder.AddAttribute(3, nameof(AgtDrawer.ChildContent), (RenderFragment)(_ => { }));
            builder.CloseComponent();
        });

        cut.Find(".agt-drawer")
            .KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        Assert.False(open);
    }
}
