using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace Agterhuis.Ui.Components.Buttons;

public partial class AgtPrimaryButton
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string BusyText { get; set; } = "Bezig...";

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public ButtonType ButtonType { get; set; } = ButtonType.Button;

    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Medium;

    [Parameter]
    public EventCallback<MouseEventArgs> Click { get; set; }

    private bool IsDisabled => Disabled || IsBusy;

    private string? CombinedClass => string.IsNullOrWhiteSpace(CssClass) ? null : CssClass;

    private string DisplayText => IsBusy ? BusyText : Text;

    private async Task HandleClick(MouseEventArgs args)
    {
        if (IsDisabled)
        {
            return;
        }

        await Click.InvokeAsync(args);
    }
}
