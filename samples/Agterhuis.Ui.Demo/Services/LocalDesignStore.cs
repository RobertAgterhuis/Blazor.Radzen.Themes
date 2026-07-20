using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Serialization;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Demo.Services;

public sealed class LocalDesignStore(IJSRuntime jsRuntime) : IDesignStore
{
    private const string IndexKey = "agt-designer-documents";
    private const string DocumentPrefix = "agt-designer-document-";

    public async Task<IReadOnlyList<string>> GetRecentNamesAsync()
    {
        var names = await jsRuntime.InvokeAsync<List<string>>("designerInterop.getJson", IndexKey) ?? [];
        return names
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<string?> LoadAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return await jsRuntime.InvokeAsync<string?>("designerInterop.getText", DocumentPrefix + name);
    }

    public async Task SaveAsync(string name, DesignDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(document);

        var json = DesignDocumentSerializer.Serialize(document);
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", DocumentPrefix + name, json);

        var recentNames = (await GetRecentNamesAsync()).Where(existing => !string.Equals(existing, name, StringComparison.Ordinal)).ToList();
        recentNames.Insert(0, name);
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", IndexKey, recentNames.Take(10).ToArray());
    }

    public async Task RemoveAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await jsRuntime.InvokeVoidAsync("designerInterop.removeItem", DocumentPrefix + name);

        var recentNames = (await GetRecentNamesAsync()).Where(existing => !string.Equals(existing, name, StringComparison.Ordinal)).ToArray();
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", IndexKey, recentNames);
    }

    public async Task<IReadOnlyList<(string Name, string Json)>> GetDocumentsAsync()
    {
        var documents = new List<(string Name, string Json)>();
        foreach (var name in await GetRecentNamesAsync())
        {
            var json = await LoadAsync(name);
            if (!string.IsNullOrWhiteSpace(json))
            {
                documents.Add((name, json));
            }
        }

        return documents;
    }
}
