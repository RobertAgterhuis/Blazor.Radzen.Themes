using Agterhuis.Ui.Options;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Demo.Components.Layout;

public partial class MainLayout : IDisposable
{
    private const string CommandScope = "demo-main-layout";
    private const string SidebarStateStorageKey = "main-shell-sidebar";

    private static readonly string[] CalmRoutePrefixes = ["components/data/", "catalog/data", "catalog/data-advanced", "catalog/scheduling", "app/"];

    private static readonly Dictionary<string, string> WrapperToCatalogRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["components/buttons"] = "/catalog/buttons",
        ["components/layout"] = "/catalog/layout",
        ["components/layout/sidebar-layout"] = "/catalog/layout-advanced",
        ["components/feedback"] = "/catalog/feedback",
        ["components/feedback/empty-state"] = "/catalog/feedback",
        ["components/feedback/loading-panel"] = "/catalog/feedback",
        ["components/feedback/confirm-dialog"] = "/catalog/overlays",
        ["components/feedback/notification-service"] = "/catalog/overlays",
        ["components/feedback/badge"] = "/catalog/feedback",
        ["components/forms/checkbox"] = "/catalog/selection-inputs",
        ["components/forms/switch"] = "/catalog/selection-inputs",
        ["components/forms/radio-list"] = "/catalog/selection-inputs",
        ["components/forms/text-field"] = "/catalog/text-inputs",
        ["components/forms/numeric-field"] = "/catalog/text-inputs",
        ["components/forms/text-area"] = "/catalog/text-inputs",
        ["components/forms/password"] = "/catalog/text-inputs",
        ["components/forms/auto-complete"] = "/catalog/text-inputs",
        ["components/forms/dropdown"] = "/catalog/selection-inputs",
        ["components/forms/date-picker"] = "/catalog/pickers",
        ["components/forms/file-upload"] = "/catalog/forms-advanced",
        ["components/forms/form-actions"] = "/catalog/forms",
        ["components/data/grid"] = "/catalog/data",
        ["components/layout/tabs"] = "/catalog/navigation",
        ["components/layout/breadcrumb"] = "/catalog/navigation"
    };

    private static readonly Dictionary<string, string> CatalogToWrapperRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["catalog/buttons"] = "/components/buttons",
        ["catalog/layout"] = "/components/layout",
        ["catalog/layout-advanced"] = "/components/layout/sidebar-layout",
        ["catalog/feedback"] = "/components/feedback",
        ["catalog/overlays"] = "/components/feedback/confirm-dialog",
        ["catalog/text-inputs"] = "/components/forms/text-field",
        ["catalog/selection-inputs"] = "/components/forms/dropdown",
        ["catalog/pickers"] = "/components/forms/date-picker",
        ["catalog/forms"] = "/components/forms/form-actions",
        ["catalog/forms-advanced"] = "/components/forms/file-upload",
        ["catalog/data"] = "/components/data/grid",
        ["catalog/navigation"] = "/components/layout/tabs"
    };

    [Inject]
    private AgtDensityState DensityState { get; set; } = default!;

    [Inject]
    private AgtThemeState ThemeState { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<AgtUiOptions> UiOptions { get; set; } = default!;

    [Inject]
    private IAgtCommandRegistry CommandRegistry { get; set; } = default!;

    protected bool SidebarExpanded { get; set; }

    private DotNetObjectReference<MainLayout>? _sidebarDismissReference;
    private ElementReference _sidebarSurfaceRef;
    private ElementReference _sidebarToggleRef;
    private bool _isCompactViewport;
    private bool _prefersReducedMotion;
    private bool _restoreSidebarToggleFocus;
    private string? _sidebarDismissRegistrationId;
    private string? _sidebarFocusTrapRegistrationId;

    private string CurrentRoute { get; set; } = string.Empty;

    private bool IsMobileDrawerOpen => SidebarExpanded && _isCompactViewport;

    private string SidebarCssClass => "demo-sidebar demo-sidebar--overlay";

    private string BodyCssClass => SidebarExpanded && !_isCompactViewport ? "demo-body demo-body--with-sidebar" : "demo-body";

    private string BodyContentCssClass => SidebarExpanded && !_isCompactViewport ? "demo-body-content demo-body-content--with-sidebar" : "demo-body-content";

    private string SidebarInlineStyle => SidebarExpanded
        ? "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:min(22rem,calc(100vw - 1.5rem));max-width:min(22rem,calc(100vw - 1.5rem));z-index:1100;transform:translateX(0);visibility:visible;pointer-events:auto;"
        : "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:min(22rem,calc(100vw - 1.5rem));max-width:min(22rem,calc(100vw - 1.5rem));z-index:1100;transform:translateX(-100%);visibility:hidden;pointer-events:none;";

    private bool HasContextLink => ShowWrapperToCatalogLink || IsCatalogRoute;

    protected override void OnInitialized()
    {
        DensityState.DensityChanged += HandleDensityChanged;
        ThemeState.ThemeChanged += HandleThemeChanged;
        NavigationManager.LocationChanged += HandleLocationChanged;
        RegisterCommands();
        UpdateCurrentRoute();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RestorePersistedShellStateAsync();
            StateHasChanged();
        }

        await SyncSidebarOverlayAsync();
        await SyncContextLinkPlacementAsync();

        if (_restoreSidebarToggleFocus)
        {
            _restoreSidebarToggleFocus = false;
            await JS.InvokeVoidAsync("agtTheme.focusElement", _sidebarToggleRef);
        }
    }

    private void HandleThemeChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await ApplyThemeToDocumentAsync();
            StateHasChanged();
        });
    }

    private void HandleDensityChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await ApplyDensityToDocumentAsync();
            StateHasChanged();
        });
    }

    private ValueTask ApplyThemeToDocumentAsync()
    {
        return JS.InvokeVoidAsync("agtTheme.setThemeWithTransition", ThemeState.Theme);
    }

    private ValueTask ApplyDensityToDocumentAsync()
    {
        return JS.InvokeVoidAsync("agtTheme.setDensity", DensityState.Density);
    }

    private async Task RestorePersistedShellStateAsync()
    {
        var persistedTheme = await JS.InvokeAsync<string>("agtTheme.getStoredTheme", ThemeState.Theme);
        var persistedDensity = await JS.InvokeAsync<string>("agtTheme.getStoredDensity", DensityState.Density);
        var persistedSidebarState = await JS.InvokeAsync<string>("agtTheme.getStoredNavSectionState", SidebarStateStorageKey, "expanded");
        ThemeState.SetTheme(persistedTheme);
        DensityState.SetDensity(persistedDensity);
        await ApplyThemeToDocumentAsync();
        await ApplyDensityToDocumentAsync();
        _isCompactViewport = await JS.InvokeAsync<bool>("agtTheme.isViewportAtMost", 1100);
        _prefersReducedMotion = await JS.InvokeAsync<bool>("agtTheme.prefersReducedMotion");
        SidebarExpanded = string.Equals(persistedSidebarState, "expanded", StringComparison.OrdinalIgnoreCase);
    }

    private ValueTask SyncContextLinkPlacementAsync()
    {
        if (!HasContextLink)
        {
            return ValueTask.CompletedTask;
        }

        return JS.InvokeVoidAsync("agtTheme.placeContextLink", "#main");
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            UpdateCurrentRoute();

            StateHasChanged();
            return Task.CompletedTask;
        });
    }

    private async Task OnSidebarToggleClicked()
    {
        _isCompactViewport = await JS.InvokeAsync<bool>("agtTheme.isViewportAtMost", 1100);
        SidebarExpanded = !SidebarExpanded;
        await PersistSidebarStateAsync();
    }

    public void Dispose()
    {
        DensityState.DensityChanged -= HandleDensityChanged;
        ThemeState.ThemeChanged -= HandleThemeChanged;
        NavigationManager.LocationChanged -= HandleLocationChanged;
        CommandRegistry.RemoveScope(CommandScope);
        _sidebarDismissReference?.Dispose();
    }

    private bool ShowAmbient => UiOptions.Value.EnableAmbientEffects && !_prefersReducedMotion && !IsCalmRoute;

    private string AmbientMode => !UiOptions.Value.EnableAmbientEffects
        ? "off"
        : _prefersReducedMotion
            ? "reduced"
            : IsCalmRoute
                ? "calm"
                : "on";

    private string MotionMode => _prefersReducedMotion ? "reduced" : "full";

    private bool IsCalmRoute => CalmRoutePrefixes.Any(prefix => CurrentRoute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private bool ShowWrapperToCatalogLink => WrapperToCatalogRoutes.ContainsKey(CurrentRoute);

    private string WrapperToCatalogLink => WrapperToCatalogRoutes.TryGetValue(CurrentRoute, out var route) ? route : "/catalog";

    private bool IsCatalogRoute => CurrentRoute.StartsWith("catalog", StringComparison.OrdinalIgnoreCase);

    private bool CatalogHasWrapperLink => CatalogToWrapperRoutes.ContainsKey(CurrentRoute);

    private string CatalogToWrapperLink => CatalogToWrapperRoutes.TryGetValue(CurrentRoute, out var route) ? route : "/components/theme";

    private void UpdateCurrentRoute()
    {
        CurrentRoute = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant();
    }

    private async Task OnSidebarBackdropClick()
    {
        await CloseSidebarAsync(restoreFocus: true);
    }

    [JSInvokable]
    public Task CloseSidebarFromJs()
    {
        return CloseSidebarAsync(restoreFocus: true);
    }

    private async Task CloseSidebarAsync(bool restoreFocus)
    {
        if (!SidebarExpanded)
        {
            if (restoreFocus)
            {
                _restoreSidebarToggleFocus = true;
            }

            return;
        }

        SidebarExpanded = false;
        _restoreSidebarToggleFocus = restoreFocus;
        await PersistSidebarStateAsync();
        await InvokeAsync(StateHasChanged);
    }

    private ValueTask PersistSidebarStateAsync()
    {
        var state = SidebarExpanded ? "expanded" : "collapsed";
        return JS.InvokeVoidAsync("agtTheme.setStoredNavSectionState", SidebarStateStorageKey, state);
    }

    private async Task SyncSidebarOverlayAsync()
    {
        if (!_isCompactViewport)
        {
            if (_sidebarDismissRegistrationId is not null)
            {
                await JS.InvokeVoidAsync("agtTheme.unregisterDismissHandler", _sidebarDismissRegistrationId);
                _sidebarDismissRegistrationId = null;
            }

            if (_sidebarFocusTrapRegistrationId is not null)
            {
                await JS.InvokeVoidAsync("agtTheme.unregisterFocusTrap", _sidebarFocusTrapRegistrationId);
                _sidebarFocusTrapRegistrationId = null;
            }

            return;
        }

        if (IsMobileDrawerOpen && _sidebarDismissRegistrationId is null)
        {
            _sidebarDismissReference ??= DotNetObjectReference.Create(this);
            _sidebarDismissRegistrationId = await JS.InvokeAsync<string>(
                "agtTheme.registerDismissHandler",
                _sidebarSurfaceRef,
                _sidebarToggleRef,
                _sidebarDismissReference,
                nameof(CloseSidebarFromJs));
            _sidebarFocusTrapRegistrationId = await JS.InvokeAsync<string>("agtTheme.registerFocusTrap", _sidebarSurfaceRef);
            await JS.InvokeVoidAsync("agtTheme.focusElement", _sidebarSurfaceRef);
        }

        if (!IsMobileDrawerOpen && _sidebarDismissRegistrationId is not null)
        {
            await JS.InvokeVoidAsync("agtTheme.unregisterDismissHandler", _sidebarDismissRegistrationId);
            _sidebarDismissRegistrationId = null;
        }

        if (!IsMobileDrawerOpen && _sidebarFocusTrapRegistrationId is not null)
        {
            await JS.InvokeVoidAsync("agtTheme.unregisterFocusTrap", _sidebarFocusTrapRegistrationId);
            _sidebarFocusTrapRegistrationId = null;
        }
    }

    private void RegisterCommands()
    {
        CommandRegistry.SetCommands(CommandScope,
        [
            new AgtCommandItem("demo-open-components", "Ga naar wrappers", "Navigatie", () => NavigateToAsync("/components/theme"))
            {
                Description = "Open de Agt wrappercatalogus.",
                ShortcutHint = "G W",
                Keywords = ["wrappers", "componenten", "agt"]
            },
            new AgtCommandItem("demo-open-catalog", "Ga naar Radzen catalogus", "Navigatie", () => NavigateToAsync("/catalog"))
            {
                Description = "Open de volledige Radzen QA-catalogus.",
                ShortcutHint = "G C",
                Keywords = ["catalog", "radzen", "qa"]
            },
            new AgtCommandItem("demo-open-workorders", "Ga naar Werkorders showcase", "Navigatie", () => NavigateToAsync("/app"))
            {
                Description = "Open de enterprise showcase shell.",
                ShortcutHint = "G A",
                Keywords = ["app", "showcase", "werkorders"]
            },
            new AgtCommandItem("demo-toggle-theme", "Wissel thema...", "Acties", ToggleThemeAsync)
            {
                Description = "Schakel direct tussen licht en donker binnen de actieve familie.",
                ShortcutHint = "T",
                Keywords = ["theme", "thema", "licht", "donker"]
            },
            new AgtCommandItem("demo-toggle-density", "Wissel dichtheid", "Acties", ToggleDensityAsync)
            {
                Description = "Schakel tussen comfortabele en compacte dichtheid.",
                ShortcutHint = "D",
                Keywords = ["density", "compact", "comfortable", "dichtheid"]
            }
        ]);
    }

    private Task NavigateToAsync(string href)
    {
        NavigationManager.NavigateTo(href);
        return Task.CompletedTask;
    }

    private Task ToggleThemeAsync()
    {
        ThemeState.ToggleTheme();
        return Task.CompletedTask;
    }

    private Task ToggleDensityAsync()
    {
        DensityState.ToggleDensity();
        return Task.CompletedTask;
    }
}
