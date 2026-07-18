using Microsoft.AspNetCore.Components;

namespace Agterhuis.Ui.Components.Layout;

public partial class AgtPageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string Kicker { get; set; } = "Component catalog";

    [Parameter]
    public RenderFragment? Actions { get; set; }
}
