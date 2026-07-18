using Microsoft.AspNetCore.Components;
using Radzen;

namespace Agterhuis.Ui.Components.Feedback;

public partial class AgtAlert
{
    [Parameter]
    public AgtIntent Intent { get; set; } = AgtIntent.Info;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private static AlertStyle ResolveStyle(AgtIntent intent)
    {
        return intent switch
        {
            AgtIntent.Neutral => AlertStyle.Secondary,
            AgtIntent.Success => AlertStyle.Success,
            AgtIntent.Warning => AlertStyle.Warning,
            AgtIntent.Danger => AlertStyle.Danger,
            _ => AlertStyle.Info
        };
    }
}
