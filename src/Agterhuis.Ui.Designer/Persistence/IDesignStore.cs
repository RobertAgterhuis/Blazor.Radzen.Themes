namespace Agterhuis.Ui.Designer.Persistence;

using Agterhuis.Ui.Designer.Model;

public interface IDesignStore
{
    Task<IReadOnlyList<DesignListItem>> GetRecentAsync();

    Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null);

    Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag);

    Task RemoveAsync(string name);

    Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name);

    Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version);
}
