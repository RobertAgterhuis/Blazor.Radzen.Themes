using Agterhuis.Ui.Demo.Components.Layout;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Theming;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class BlogLayoutReadModeTests
{
    [Fact]
    public void ReadModeToggle_PersistsPreference_ButDoesNotSwitchTheme_ForNonVoltFamily()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddAgterhuisUi();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", invocation =>
                invocation.Arguments.Count > 0
                && string.Equals(invocation.Arguments[0]?.ToString(), "blog-read-mode", StringComparison.Ordinal))
            .SetResult("off");

        ctx.JSInterop.Setup<string>("agtTheme.getStoredValue", invocation =>
                invocation.Arguments.Count > 0
                && string.Equals(invocation.Arguments[0]?.ToString(), "blog-volt-default-applied", StringComparison.Ordinal))
            .SetResult("1");

        var persistInvocation = ctx.JSInterop.SetupVoid("agtTheme.setStoredValue", _ => true);
        persistInvocation.SetVoidResult();

        var themeTransitionInvocation = ctx.JSInterop.SetupVoid("agtTheme.setThemeWithTransition", _ => true);
        themeTransitionInvocation.SetVoidResult();

        ctx.JSInterop.SetupVoid("blogShowcase.disposeMotion", _ => true).SetVoidResult();
        ctx.JSInterop.SetupVoid("blogShowcase.initMotion", _ => true).SetVoidResult();

        var themeState = ctx.Services.GetRequiredService<AgtThemeState>();
        themeState.SetTheme("ocean-dark");

        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<BlogLayout>(0);
            builder.AddAttribute(1, "Body", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddMarkupContent(0, "<section><h1>Body</h1></section>");
            }));
            builder.CloseComponent();
        });

        cut.WaitForAssertion(() => Assert.Equal("ocean-dark", themeState.Theme));

        var transitionCountBeforeToggle = themeTransitionInvocation.Invocations.Count;

        cut.Find(".blog-read-toggle").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("ocean-dark", themeState.Theme);
            Assert.Contains(persistInvocation.Invocations, invocation =>
                invocation.Arguments.Count >= 2
                && string.Equals(invocation.Arguments[0]?.ToString(), "blog-read-mode", StringComparison.Ordinal)
                && string.Equals(invocation.Arguments[1]?.ToString(), "on", StringComparison.Ordinal));
            Assert.Equal(transitionCountBeforeToggle, themeTransitionInvocation.Invocations.Count);
        });
    }
}
