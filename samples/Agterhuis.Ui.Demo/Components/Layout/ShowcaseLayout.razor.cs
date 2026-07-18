using Agterhuis.Ui.Theming;
using Microsoft.AspNetCore.Components;
using Radzen;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Agterhuis.Ui.Demo.Services;

namespace Agterhuis.Ui.Demo.Components.Layout;

public partial class ShowcaseLayout : IDisposable
{
    private const int NotificationPreviewCount = 6;
    private readonly HashSet<int> _readNotificationIds = [];
    private ElementReference _notificationsBellHostRef;
    private ElementReference _notificationsPanelRef;
    private DotNetObjectReference<ShowcaseLayout>? _dismissReference;
    private string? _dismissRegistrationId;
    private bool _focusFlyoutOnRender;
    private bool _restoreBellFocus;

    protected bool SidebarExpanded { get; set; } = true;
    protected bool NotificationsOpen { get; set; }

    protected IReadOnlyList<ShowcaseNotification> NotificationItems => DataService.Notifications.Take(NotificationPreviewCount).ToList();

    protected int UnreadNotificationCount => NotificationItems.Count(item => !_readNotificationIds.Contains(item.Id));

    protected string BellAriaLabel => UnreadNotificationCount > 0
        ? $"Meldingen, {UnreadNotificationCount} ongelezen"
        : "Meldingen, geen ongelezen berichten";

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

    protected override void OnInitialized()
    {
        ThemeState.ThemeChanged += HandleThemeChanged;
        DataService.Changed += HandleDataChanged;
    }

    protected void OnSidebarToggle()
    {
        SidebarExpanded = !SidebarExpanded;
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
            var persistedTheme = await JS.InvokeAsync<string>("agtTheme.getStoredTheme", ThemeState.Theme);
            ThemeState.SetTheme(persistedTheme);
        }

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
        ThemeState.ThemeChanged -= HandleThemeChanged;
        DataService.Changed -= HandleDataChanged;
        _dismissReference?.Dispose();
    }
}
