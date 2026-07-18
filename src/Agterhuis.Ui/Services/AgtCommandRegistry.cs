namespace Agterhuis.Ui.Services;

public sealed class AgtCommandRegistry : IAgtCommandRegistry
{
    private readonly Dictionary<string, List<AgtCommandItem>> _scopeCommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _recentCommandIds = [];
    private readonly Dictionary<string, AgtCommandItem> _commandMap = new(StringComparer.OrdinalIgnoreCase);
    private List<AgtCommandItem> _commands = [];

    public event Action? Changed;

    public IReadOnlyList<AgtCommandItem> Commands => _commands;

    public IReadOnlyList<AgtCommandItem> RecentCommands
    {
        get
        {
            var recent = new List<AgtCommandItem>(_recentCommandIds.Count);
            foreach (var commandId in _recentCommandIds)
            {
                if (_commandMap.TryGetValue(commandId, out var command))
                {
                    recent.Add(command);
                }
            }

            return recent;
        }
    }

    public void SetCommands(string scope, IEnumerable<AgtCommandItem> commands)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        var scoped = commands
            .Where(static command => !string.IsNullOrWhiteSpace(command.Id) && !string.IsNullOrWhiteSpace(command.Title))
            .DistinctBy(static command => command.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _scopeCommands[scope] = scoped;
        RebuildCommands();
    }

    public void RemoveScope(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        if (!_scopeCommands.Remove(scope))
        {
            return;
        }

        RebuildCommands();
    }

    public async Task ExecuteAsync(AgtCommandItem command)
    {
        ArgumentNullException.ThrowIfNull(command);

        await command.ExecuteAsync();

        _recentCommandIds.RemoveAll(id => string.Equals(id, command.Id, StringComparison.OrdinalIgnoreCase));
        _recentCommandIds.Insert(0, command.Id);

        const int maxRecentCount = 6;
        if (_recentCommandIds.Count > maxRecentCount)
        {
            _recentCommandIds.RemoveRange(maxRecentCount, _recentCommandIds.Count - maxRecentCount);
        }

        Changed?.Invoke();
    }

    private void RebuildCommands()
    {
        _commands = _scopeCommands.Values.SelectMany(static commandList => commandList)
            .DistinctBy(static command => command.Id, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static command => command.Section, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static command => command.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _commandMap.Clear();
        foreach (var command in _commands)
        {
            _commandMap[command.Id] = command;
        }

        _recentCommandIds.RemoveAll(commandId => !_commandMap.ContainsKey(commandId));

        Changed?.Invoke();
    }
}
