using Agterhuis.Ui.Components.Feedback;

namespace Agterhuis.Ui.Services;

public sealed class AgtConfirmOptions
{
    public string? OkText { get; set; }

    public string? CancelText { get; set; }

    public AgtIntent Intent { get; set; } = AgtIntent.Primary;
}
