namespace Agterhuis.Ui.Services;

public sealed record AgtCommandItem(string Id, string Title, string Section, Func<Task> ExecuteAsync)
{
    public string Description { get; init; } = string.Empty;

    public string ShortcutHint { get; init; } = string.Empty;

    public string[] Keywords { get; init; } = [];
}
