namespace Agterhuis.Ui.Services;

public interface IAgtCommandRegistry
{
    event Action? Changed;

    IReadOnlyList<AgtCommandItem> Commands { get; }

    IReadOnlyList<AgtCommandItem> RecentCommands { get; }

    void SetCommands(string scope, IEnumerable<AgtCommandItem> commands);

    void RemoveScope(string scope);

    Task ExecuteAsync(AgtCommandItem command);
}
