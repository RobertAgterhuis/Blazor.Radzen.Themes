using Agterhuis.Ui.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Demo.Components.Layout;

public partial class BlogLayout : IDisposable
{
    private const string BlogDefaultAppliedKey = "blog-volt-default-applied";
    private const string BlogReadModeKey = "blog-read-mode";

    [Inject]
    private AgtThemeState ThemeState { get; set; } = default!;

    [Inject]
    private AgtDensityState DensityState { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private bool _initialized;
    private bool _mobileMenuOpen;

    protected bool IsReadMode { get; private set; }

    private string CurrentRoute { get; set; } = "blog";

    private bool IsArticleRoute => CurrentRoute.StartsWith("blog/artikel", StringComparison.OrdinalIgnoreCase);

    protected override void OnInitialized()
    {
        ThemeState.ThemeChanged += HandleThemeChanged;
        NavigationManager.LocationChanged += HandleLocationChanged;
        UpdateCurrentRoute();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RestoreBlogPreferencesAsync();
            await EnsureVoltDefaultAsync();
            await ApplyReadModeThemeAsync();
            await ApplyThemeToDocumentAsync();
            _initialized = true;
            StateHasChanged();
        }

        if (_initialized)
        {
            await JS.InvokeVoidAsync("blogShowcase.disposeMotion", "#blog-shell");
            await JS.InvokeVoidAsync("blogShowcase.initMotion", "#blog-shell");
        }
    }

    private async Task RestoreBlogPreferencesAsync()
    {
        var storedReadMode = await JS.InvokeAsync<string>("agtTheme.getStoredValue", BlogReadModeKey, string.Empty);
        if (string.Equals(storedReadMode, "on", StringComparison.OrdinalIgnoreCase))
        {
            IsReadMode = true;
        }
        else if (string.Equals(storedReadMode, "off", StringComparison.OrdinalIgnoreCase))
        {
            IsReadMode = false;
        }
        else
        {
            IsReadMode = IsArticleRoute;
        }

        await PersistReadModeAsync();
    }

    private async Task EnsureVoltDefaultAsync()
    {
        var defaultApplied = await JS.InvokeAsync<string>("agtTheme.getStoredValue", BlogDefaultAppliedKey, string.Empty);
        if (string.Equals(defaultApplied, "1", StringComparison.Ordinal))
        {
            return;
        }

        if (!string.Equals(ThemeState.ActiveTheme.Name, "volt", StringComparison.OrdinalIgnoreCase))
        {
            ThemeState.SetTheme("volt-dark");
        }

        await JS.InvokeVoidAsync("agtTheme.setStoredValue", BlogDefaultAppliedKey, "1");
    }

    private async Task OnReadModeToggle(MouseEventArgs _)
    {
        IsReadMode = !IsReadMode;
        await PersistReadModeAsync();
        await ApplyReadModeThemeAsync();
    }

    private Task ToggleMobileMenu()
    {
        _mobileMenuOpen = !_mobileMenuOpen;
        return Task.CompletedTask;
    }

    private async Task PersistReadModeAsync()
    {
        await JS.InvokeVoidAsync("agtTheme.setStoredValue", BlogReadModeKey, IsReadMode ? "on" : "off");
    }

    private async Task ApplyReadModeThemeAsync()
    {
        if (!string.Equals(ThemeState.ActiveTheme.Name, "volt", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ThemeState.SetTheme(IsReadMode ? "volt-light" : "volt-dark");
        await ApplyThemeToDocumentAsync();
    }

    private ValueTask ApplyThemeToDocumentAsync()
    {
        return JS.InvokeVoidAsync("agtTheme.setThemeWithTransition", ThemeState.Theme);
    }

    private void HandleThemeChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await ApplyThemeToDocumentAsync();
            StateHasChanged();
        });
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = InvokeAsync(async () =>
        {
            _mobileMenuOpen = false;
            UpdateCurrentRoute();

            if (IsArticleRoute && !IsReadMode)
            {
                IsReadMode = true;
                await PersistReadModeAsync();
                await ApplyReadModeThemeAsync();
            }

            StateHasChanged();
        });
    }

    private string DesktopLinkClass(string path)
    {
        return IsActive(path) ? "blog-nav__link is-active" : "blog-nav__link";
    }

    private string MobileLinkClass(string path, bool exact = false)
    {
        return IsActive(path, exact) ? "blog-mobile-menu__link is-active" : "blog-mobile-menu__link";
    }

    private string TabLinkClass(string path, bool exact = false)
    {
        return IsActive(path, exact) ? "blog-tabbar__link is-active" : "blog-tabbar__link";
    }

    private string? AriaCurrent(string path, bool exact = false)
    {
        return IsActive(path, exact) ? "page" : null;
    }

    private bool IsActive(string path, bool exact = false)
    {
        var normalizedPath = path.Trim('/');
        if (exact)
        {
            return string.Equals(CurrentRoute, normalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        return CurrentRoute.StartsWith(normalizedPath, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateCurrentRoute()
    {
        CurrentRoute = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).TrimEnd('/');
        if (string.IsNullOrWhiteSpace(CurrentRoute))
        {
            CurrentRoute = "blog";
        }
    }

    public void Dispose()
    {
        ThemeState.ThemeChanged -= HandleThemeChanged;
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
