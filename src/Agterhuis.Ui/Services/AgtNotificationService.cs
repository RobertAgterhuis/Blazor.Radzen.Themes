using Agterhuis.Ui.Components.Feedback;
using Radzen;

namespace Agterhuis.Ui.Services;

public sealed class AgtNotificationService(NotificationService notificationService) : IAgtNotificationService
{
    private const double DefaultDurationMs = 4000;

    public void Success(string title, string? detail = null, double? duration = null)
    {
        Notify(AgtIntent.Success, title, detail, duration);
    }

    public void Warning(string title, string? detail = null, double? duration = null)
    {
        Notify(AgtIntent.Warning, title, detail, duration);
    }

    public void Danger(string title, string? detail = null, double? duration = null)
    {
        Notify(AgtIntent.Danger, title, detail, duration);
    }

    public void Info(string title, string? detail = null, double? duration = null)
    {
        Notify(AgtIntent.Info, title, detail, duration);
    }

    private void Notify(AgtIntent intent, string title, string? detail, double? duration)
    {
        notificationService.Notify(new NotificationMessage
        {
            Severity = ResolveSeverity(intent),
            Summary = $"{ResolveIcon(intent)} {title}",
            Detail = detail ?? string.Empty,
            Duration = duration ?? DefaultDurationMs,
            ShowProgress = true,
            Style = ResolveStyle(intent)
        });
    }

    private static NotificationSeverity ResolveSeverity(AgtIntent intent)
    {
        return intent switch
        {
            AgtIntent.Success => NotificationSeverity.Success,
            AgtIntent.Warning => NotificationSeverity.Warning,
            AgtIntent.Danger => NotificationSeverity.Error,
            _ => NotificationSeverity.Info
        };
    }

    private static string ResolveIcon(AgtIntent intent)
    {
        return intent switch
        {
            AgtIntent.Success => "check_circle",
            AgtIntent.Warning => "warning",
            AgtIntent.Danger => "error",
            _ => "info"
        };
    }

    private static string ResolveStyle(AgtIntent intent)
    {
        var intentToken = intent.ToString().ToLowerInvariant();
        return $"--agt-notification-intent:{intentToken}";
    }
}
