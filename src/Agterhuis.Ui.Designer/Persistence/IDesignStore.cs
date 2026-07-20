namespace Agterhuis.Ui.Designer.Persistence;

using Agterhuis.Ui.Designer.Model;

public interface IDesignStore
{
    Task<IReadOnlyList<string>> GetRecentNamesAsync();

    Task<string?> LoadAsync(string name);

    Task SaveAsync(string name, DesignDocument document);

    Task RemoveAsync(string name);
}
