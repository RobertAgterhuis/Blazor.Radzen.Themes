namespace Agterhuis.Ui.Services;

public interface IAgtConfirmDialog
{
    Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null);

    Task<bool> ConfirmDeleteAsync(string itemName);
}
