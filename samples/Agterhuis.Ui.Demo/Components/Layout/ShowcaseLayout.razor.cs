using Agterhuis.Ui.Theming;
using Agterhuis.Ui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Radzen;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Agterhuis.Ui.Demo.Services;

namespace Agterhuis.Ui.Demo.Components.Layout;

public partial class ShowcaseLayout : IDisposable
{
    private sealed record NavEntry(string Text, string Icon, string Path, NavLinkMatch Match = NavLinkMatch.Prefix);

    private const string CommandScope = "demo-showcase-layout";
    private const int NotificationPreviewCount = 6;
    private const string SidebarStateStorageKey = "showcase-shell-sidebar";
    private static readonly IReadOnlyList<NavEntry> ShowcaseNavItems =
    [
        new("Dashboard", "space_dashboard", "/app", NavLinkMatch.All),
        new("Werkorders", "assignment", "/app/werkorders"),
        new("Planning", "event", "/app/planning"),
        new("Klanten", "groups", "/app/klanten"),
        new("Projecten", "account_tree", "/app/projecten"),
        new("Assets", "hub", "/app/assets"),
        new("Servicedesk", "support_agent", "/app/servicedesk"),
        new("Rapportage", "query_stats", "/app/rapportage"),
        new("Instellingen", "settings", "/app/instellingen"),
        new("Help", "help", "/app/help")
    ];

    private readonly HashSet<int> _readNotificationIds = [];
    private ElementReference _notificationsBellHostRef;
    private ElementReference _notificationsPanelRef;
    private ElementReference _navFilterRef;
    private ElementReference _showcaseNavRootRef;
    private ElementReference _sidebarSurfaceRef;
    private ElementReference _sidebarToggleRef;
    private DotNetObjectReference<ShowcaseLayout>? _dismissReference;
    private DotNetObjectReference<ShowcaseLayout>? _sidebarDismissReference;
    private string? _dismissRegistrationId;
    private string? _sidebarDismissRegistrationId;
    private string? _sidebarFocusTrapRegistrationId;
    private bool _isCompactViewport;
    private bool _focusFlyoutOnRender;
    private bool _restoreBellFocus;
    private bool _restoreSidebarToggleFocus;

    protected bool SidebarExpanded { get; set; }
    protected bool NotificationsOpen { get; set; }
    protected string NavFilterText { get; set; } = string.Empty;

    private bool IsMobileDrawerOpen => SidebarExpanded && _isCompactViewport;

    private bool IsDesktopRail => !_isCompactViewport && !SidebarExpanded;

    private string SidebarCssClass => IsDesktopRail
        ? "showcase-sidebar showcase-sidebar--overlay showcase-sidebar--desktop-collapsed"
        : "showcase-sidebar showcase-sidebar--overlay";

    private string BodyCssClass => !_isCompactViewport
        ? (SidebarExpanded ? "showcase-body showcase-body--with-sidebar" : "showcase-body showcase-body--with-rail")
        : "showcase-body";

    private string SidebarInlineStyle => _isCompactViewport
        ? (SidebarExpanded
            ? "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:min(22rem,calc(100vw - 1.5rem));max-width:min(22rem,calc(100vw - 1.5rem));z-index:1100;transform:translateX(0);visibility:visible;pointer-events:auto;"
            : "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:min(22rem,calc(100vw - 1.5rem));max-width:min(22rem,calc(100vw - 1.5rem));z-index:1100;transform:translateX(-100%);visibility:hidden;pointer-events:none;")
        : (SidebarExpanded
            ? "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:min(22rem,calc(100vw - 1.5rem));max-width:min(22rem,calc(100vw - 1.5rem));z-index:1100;transform:translateX(0);visibility:visible;pointer-events:auto;"
            : "position:fixed;left:0;top:3.75rem;height:calc(100vh - 3.75rem);width:4.5rem;max-width:4.5rem;z-index:1100;transform:translateX(0);visibility:visible;pointer-events:auto;");

    private static string PackageVersion => typeof(AgtThemeState).Assembly.GetName().Version?.ToString(3) ?? "dev";

    private bool HasNavFilter => !string.IsNullOrWhiteSpace(NavFilterText);

    private List<NavEntry> FilteredShowcaseNavItems =>
        HasNavFilter
            ? [.. ShowcaseNavItems.Where(item => item.Text.Contains(NavFilterText, StringComparison.OrdinalIgnoreCase))]
            : [.. ShowcaseNavItems];

    private bool HasVisibleShowcaseNavItems => FilteredShowcaseNavItems.Count > 0;

    protected IReadOnlyList<ShowcaseNotification> NotificationItems => DataService.Notifications.Take(NotificationPreviewCount).ToList();

    protected int UnreadNotificationCount => NotificationItems.Count(item => !_readNotificationIds.Contains(item.Id));

    protected string BellAriaLabel => UnreadNotificationCount > 0
        ? $"Meldingen, {UnreadNotificationCount} ongelezen"
        : "Meldingen, geen ongelezen berichten";

    [Inject]
    private AgtDensityState DensityState { get; set; } = default!;

    [Inject]
    private AgtThemeState ThemeState { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ShowcaseDataService DataService { get; set; } = default!;

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAgtCommandRegistry CommandRegistry { get; set; } = default!;

    protected override void OnInitialized()
    {
        DensityState.DensityChanged += HandleDensityChanged;
        ThemeState.ThemeChanged += HandleThemeChanged;
        DataService.Changed += HandleDataChanged;
        NavigationManager.LocationChanged += HandleLocationChanged;
        RegisterCommands();
    }

    private void HandleDensityChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await JS.InvokeVoidAsync("agtTheme.setDensity", DensityState.Density);
            StateHasChanged();
        });
    }

    private async Task OnSidebarToggleClicked()
    {
        _isCompactViewport = await JS.InvokeAsync<bool>("agtTheme.isViewportAtMost", 1100);
        SidebarExpanded = !SidebarExpanded;
        await PersistSidebarStateAsync();
    }

    protected Task OnNotificationsToggle(MouseEventArgs _)
    {
        NotificationsOpen = !NotificationsOpen;

        if (NotificationsOpen)
        {
            MarkVisibleNotificationsAsRead();
            _focusFlyoutOnRender = true;
        }
        else
        {
            _restoreBellFocus = true;
        }

        return Task.CompletedTask;
    }

    protected Task OnMarkAllRead(MouseEventArgs _)
    {
        foreach (var item in DataService.Notifications)
        {
            _readNotificationIds.Add(item.Id);
        }

        return Task.CompletedTask;
    }

    protected bool IsUnread(ShowcaseNotification item)
    {
        return !_readNotificationIds.Contains(item.Id);
    }

    protected static string NotificationIntentClass(ShowcaseIntent intent) => intent switch
    {
        ShowcaseIntent.Success => "showcase-notifications__icon--success",
        ShowcaseIntent.Warning => "showcase-notifications__icon--warning",
        ShowcaseIntent.Danger => "showcase-notifications__icon--danger",
        _ => "showcase-notifications__icon--info"
    };

    protected static string NotificationIcon(ShowcaseNotification item)
    {
        if (item.Title.Contains("onderweg", StringComparison.OrdinalIgnoreCase))
        {
            return "directions_car";
        }

        if (item.Title.Contains("afgerond", StringComparison.OrdinalIgnoreCase))
        {
            return "task_alt";
        }

        if (item.Title.Contains("klant", StringComparison.OrdinalIgnoreCase))
        {
            return "help_center";
        }

        return "notifications";
    }

    protected static string RelativeTime(DateTime when)
    {
        var elapsed = DateTime.Now - when;

        if (elapsed.TotalMinutes < 1)
        {
            return "zojuist";
        }

        if (elapsed.TotalHours < 1)
        {
            return $"{Math.Max(1, (int)elapsed.TotalMinutes)} min geleden";
        }

        if (elapsed.TotalDays < 1)
        {
            return $"{Math.Max(1, (int)elapsed.TotalHours)} u geleden";
        }

        if (elapsed.TotalDays < 2)
        {
            return "gisteren";
        }

        return $"{Math.Max(1, (int)elapsed.TotalDays)} d geleden";
    }

    protected void OnActionTooltip(ElementReference element)
    {
        TooltipService.Open(element, "Snelactie", new TooltipOptions { Duration = 1500 });
    }

    protected async Task OnQuickPreview(MouseEventArgs _)
    {
        var latest = DataService.WorkOrders.FirstOrDefault();
        if (latest is null)
        {
            return;
        }

        await DialogService.OpenAsync<Agterhuis.Ui.Demo.Components.Pages.App.ShowcaseWorkOrderDetailsDialog>(
            $"Preview {latest.Number}",
            new Dictionary<string, object?>
            {
                ["WorkOrder"] = latest
            },
            new DialogOptions
            {
                Width = "520px",
                CloseDialogOnOverlayClick = true
            });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RestorePersistedShellStateAsync();
            StateHasChanged();
        }

        await SyncSidebarOverlayAsync();
        await JS.InvokeVoidAsync("agtTheme.applyNavItemTitles", _showcaseNavRootRef);

        if (NotificationsOpen && _dismissRegistrationId is null)
        {
            _dismissReference ??= DotNetObjectReference.Create(this);
            _dismissRegistrationId = await JS.InvokeAsync<string>(
                "agtTheme.registerDismissHandler",
                _notificationsPanelRef,
                _notificationsBellHostRef,
                _dismissReference,
                nameof(CloseNotificationsFromJs));
        }

        if (!NotificationsOpen && _dismissRegistrationId is not null)
        {
            await JS.InvokeVoidAsync("agtTheme.unregisterDismissHandler", _dismissRegistrationId);
            _dismissRegistrationId = null;
        }

        if (_focusFlyoutOnRender && NotificationsOpen)
        {
            _focusFlyoutOnRender = false;
            await JS.InvokeVoidAsync("agtTheme.focusElement", _notificationsPanelRef);
        }

        if (_restoreBellFocus)
        {
            _restoreBellFocus = false;
            await JS.InvokeVoidAsync("agtTheme.focusElement", _notificationsBellHostRef);
        }

        if (_restoreSidebarToggleFocus)
        {
            _restoreSidebarToggleFocus = false;
            await JS.InvokeVoidAsync("agtTheme.focusElement", _sidebarToggleRef);
        }
    }

    private async Task RestorePersistedShellStateAsync()
    {
        var persistedTheme = await JS.InvokeAsync<string>("agtTheme.getStoredTheme", ThemeState.Theme);
        var persistedDensity = await JS.InvokeAsync<string>("agtTheme.getStoredDensity", DensityState.Density);
        var persistedSidebarState = await JS.InvokeAsync<string>("agtTheme.getStoredNavSectionState", SidebarStateStorageKey, "expanded");
        ThemeState.SetTheme(persistedTheme);
        DensityState.SetDensity(persistedDensity);
        await JS.InvokeVoidAsync("agtTheme.setThemeWithTransition", ThemeState.Theme);
        await JS.InvokeVoidAsync("agtTheme.setDensity", DensityState.Density);
        _isCompactViewport = await JS.InvokeAsync<bool>("agtTheme.isViewportAtMost", 1100);
        SidebarExpanded = string.Equals(persistedSidebarState, "expanded", StringComparison.OrdinalIgnoreCase);
    }

    private void OnNavFilterInput(ChangeEventArgs args)
    {
        NavFilterText = args.Value?.ToString() ?? string.Empty;
    }

    private async Task OnNavFilterKeyDown(KeyboardEventArgs args)
    {
        if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            if (!HasNavFilter)
            {
                return;
            }

            NavFilterText = string.Empty;
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (string.Equals(args.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.focusFirstNavItem", _showcaseNavRootRef);
        }
    }

    private async Task OnShowcaseMenuKeyDown(KeyboardEventArgs args)
    {
        if (string.Equals(args.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.focusNavItem", _showcaseNavRootRef, "next");
            return;
        }

        if (string.Equals(args.Key, "ArrowUp", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.focusNavItem", _showcaseNavRootRef, "prev");
            return;
        }

        if (string.Equals(args.Key, "Home", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.focusNavItem", _showcaseNavRootRef, "first");
            return;
        }

        if (string.Equals(args.Key, "End", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.focusNavItem", _showcaseNavRootRef, "last");
            return;
        }

        if (string.Equals(args.Key, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("agtTheme.activateFocusedNavItem", _showcaseNavRootRef);
        }
    }

    private async Task ClearNavFilter()
    {
        if (!HasNavFilter)
        {
            return;
        }

        NavFilterText = string.Empty;
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("agtTheme.focusElement", _navFilterRef);
    }

    [JSInvokable]
    public Task CloseNotificationsFromJs()
    {
        if (!NotificationsOpen)
        {
            return Task.CompletedTask;
        }

        NotificationsOpen = false;
        _restoreBellFocus = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void HandleThemeChanged()
    {
        _ = InvokeAsync(async () =>
        {
            await JS.InvokeVoidAsync("agtTheme.setThemeWithTransition", ThemeState.Theme);
            StateHasChanged();
        });
    }

    private void HandleDataChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void MarkVisibleNotificationsAsRead()
    {
        foreach (var item in NotificationItems)
        {
            _readNotificationIds.Add(item.Id);
        }
    }

    public void Dispose()
    {
        DensityState.DensityChanged -= HandleDensityChanged;
        ThemeState.ThemeChanged -= HandleThemeChanged;
        DataService.Changed -= HandleDataChanged;
        NavigationManager.LocationChanged -= HandleLocationChanged;
        CommandRegistry.RemoveScope(CommandScope);
        _dismissReference?.Dispose();
        _sidebarDismissReference?.Dispose();
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            StateHasChanged();
            return Task.CompletedTask;
        });
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
            new AgtCommandItem("showcase-dashboard", "Ga naar dashboard", "Navigatie", () => NavigateToAsync("/app"))
            {
                Description = "Open het werkorders dashboard.",
                ShortcutHint = "G D",
                Keywords = ["dashboard", "home", "werkorders"]
            },
            new AgtCommandItem("showcase-planning", "Ga naar planning", "Navigatie", () => NavigateToAsync("/app/planning"))
            {
                Description = "Open de planningsweergave.",
                ShortcutHint = "G P",
                Keywords = ["planning", "kalender", "rooster"]
            },
            new AgtCommandItem("showcase-werkorders", "Ga naar werkorders", "Navigatie", () => NavigateToAsync("/app/werkorders"))
            {
                Description = "Open de werkorderlijst.",
                ShortcutHint = "G W",
                Keywords = ["werkorders", "grid", "tickets"]
            },
            new AgtCommandItem("showcase-nieuwe-werkorder", "Nieuwe werkorder", "Acties", () => NavigateToAsync("/app/werkorders"))
            {
                Description = "Ga naar werkorders en start een nieuw item.",
                ShortcutHint = "N",
                Keywords = ["nieuw", "aanmaken", "werkorder"]
            },
            new AgtCommandItem("showcase-toggle-theme", "Wissel thema...", "Acties", ToggleThemeAsync)
            {
                Description = "Schakel direct tussen licht en donker.",
                ShortcutHint = "T",
                Keywords = ["theme", "thema", "licht", "donker"]
            },
            new AgtCommandItem("showcase-toggle-density", "Wissel dichtheid", "Acties", ToggleDensityAsync)
            {
                Description = "Schakel tussen compacte en comfortabele dichtheid.",
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
