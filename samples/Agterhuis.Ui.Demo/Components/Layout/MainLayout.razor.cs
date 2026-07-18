using Agterhuis.Ui.Options;
using Agterhuis.Ui.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Demo.Components.Layout;

public partial class MainLayout : IDisposable
{
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
    private AgtThemeState ThemeState { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<AgtUiOptions> UiOptions { get; set; } = default!;

    protected bool SidebarExpanded { get; set; } = true;

    private bool _prefersReducedMotion;

    private string CurrentRoute { get; set; } = string.Empty;

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
            var persistedTheme = await JS.InvokeAsync<string>("agtTheme.getStoredTheme", ThemeState.Theme);
            ThemeState.SetTheme(persistedTheme);
            _prefersReducedMotion = await JS.InvokeAsync<bool>("agtTheme.prefersReducedMotion");
            StateHasChanged();
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

    private ValueTask ApplyThemeToDocumentAsync()
    {
        return JS.InvokeVoidAsync("agtTheme.setThemeWithTransition", ThemeState.Theme);
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateCurrentRoute();
        StateHasChanged();
    }

    private Task OnSidebarToggle()
    {
        SidebarExpanded = !SidebarExpanded;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        ThemeState.ThemeChanged -= HandleThemeChanged;
        NavigationManager.LocationChanged -= HandleLocationChanged;
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
}
