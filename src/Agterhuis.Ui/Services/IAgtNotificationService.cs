namespace Agterhuis.Ui.Services;

public interface IAgtNotificationService
{
    void Success(string title, string? detail = null, double? duration = null);

    void Warning(string title, string? detail = null, double? duration = null);

    void Danger(string title, string? detail = null, double? duration = null);

    void Info(string title, string? detail = null, double? duration = null);
}
