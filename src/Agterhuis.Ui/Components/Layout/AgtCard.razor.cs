using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Components.Layout;

public partial class AgtCard
{
    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public string? CssClass { get; set; }
}
