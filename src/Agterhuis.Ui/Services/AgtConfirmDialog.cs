using Agterhuis.Ui.Components.Feedback;
using Radzen;

namespace Agterhuis.Ui.Services;

public sealed class AgtConfirmDialog(DialogService dialogService) : IAgtConfirmDialog
{
    public async Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null)
    {
        options ??= new AgtConfirmOptions();

        var settings = new ConfirmOptions
        {
            OkButtonText = options.OkText ?? "Ja",
            CancelButtonText = options.CancelText ?? "Nee",
            CssClass = ResolveCssClass(options.Intent)
        };

        var result = await dialogService.Confirm(message, title, settings);
        return result ?? false;
    }

    public Task<bool> ConfirmDeleteAsync(string itemName)
    {
        var safeItemName = string.IsNullOrWhiteSpace(itemName) ? "dit item" : itemName.Trim();

        return ConfirmAsync(
            $"Weet u zeker dat u '{safeItemName}' wilt verwijderen? Deze actie kan niet ongedaan worden gemaakt.",
            "Verwijderen bevestigen",
            new AgtConfirmOptions
            {
                Intent = AgtIntent.Danger,
                OkText = "Verwijderen",
                CancelText = "Annuleren"
            });
    }

    private static string ResolveCssClass(AgtIntent intent)
    {
        return intent switch
        {
            AgtIntent.Danger => "agt-confirm-dialog agt-confirm-dialog--danger",
            AgtIntent.Secondary => "agt-confirm-dialog agt-confirm-dialog--secondary",
            _ => "agt-confirm-dialog agt-confirm-dialog--primary"
        };
    }
}
