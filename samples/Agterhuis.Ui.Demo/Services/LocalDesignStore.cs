using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Serialization;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Demo.Services;

public sealed class LocalDesignStore(IJSRuntime jsRuntime) : IDesignStore
{
    private const string IndexKey = "agt-designer-documents";
    private const string LegacyDocumentPrefix = "agt-designer-document-";
    private const string EnvelopePrefix = "agt-designer-envelope-";
    private const string VersionsPrefix = "agt-designer-versions-";
    private const int MaxRecent = 10;
    private const int MaxVersions = 5;

    public async Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
    {
        var names = await jsRuntime.InvokeAsync<List<string>>("designerInterop.getJson", IndexKey) ?? [];
        var distinct = names
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var items = new List<DesignListItem>(distinct.Length);
        foreach (var name in distinct)
        {
            var envelope = await LoadAsync(name);
            if (envelope is not null)
            {
                items.Add(new DesignListItem(envelope.Name, envelope.LastModified, envelope.Version));
            }
        }

        return items
            .OrderByDescending(static item => item.LastModified)
            .ThenBy(static item => item.Name, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (version is int targetVersion && targetVersion > 0)
        {
            var versions = await GetLocalVersionsAsync(name);
            return versions.FirstOrDefault(item => item.Version == targetVersion);
        }

        var envelopeJson = await jsRuntime.InvokeAsync<string?>("designerInterop.getText", EnvelopePrefix + name);
        if (!string.IsNullOrWhiteSpace(envelopeJson))
        {
            var envelope = DeserializeEnvelope(envelopeJson);
            if (envelope is not null)
            {
                return envelope;
            }
        }

        // Backward compatibility for previously stored plain document JSON.
        var legacyJson = await jsRuntime.InvokeAsync<string?>("designerInterop.getText", LegacyDocumentPrefix + name);
        if (string.IsNullOrWhiteSpace(legacyJson))
        {
            return null;
        }

        try
        {
            var legacyDocument = DesignDocumentSerializer.Deserialize(legacyJson);
            return new DesignDocumentEnvelope(
                name,
                1,
                "local-v1",
                DateTimeOffset.UtcNow,
                legacyDocument);
        }
        catch
        {
            return null;
        }
    }

    public async Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(document);

        var now = DateTimeOffset.UtcNow;
        var existing = await LoadAsync(name);
        var nextVersion = (existing?.Version ?? 0) + 1;
        var envelope = new DesignDocumentEnvelope(name, nextVersion, $"local-v{nextVersion}", now, document);

        var envelopes = await GetLocalVersionsAsync(name);
        envelopes.Add(envelope);
        if (envelopes.Count > MaxVersions)
        {
            envelopes = envelopes.Skip(envelopes.Count - MaxVersions).ToList();
        }

        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", EnvelopePrefix + name, SerializeEnvelope(envelope));
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", VersionsPrefix + name, envelopes.Select(SerializeEnvelope).ToArray());
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", LegacyDocumentPrefix + name, DesignDocumentSerializer.Serialize(document));

        var recentNames = ((await jsRuntime.InvokeAsync<List<string>>("designerInterop.getJson", IndexKey)) ?? [])
            .Where(existingName => !string.Equals(existingName, name, StringComparison.Ordinal))
            .ToList();
        recentNames.Insert(0, name);
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", IndexKey, recentNames.Take(MaxRecent).ToArray());

        return envelope;
    }

    public async Task RemoveAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await jsRuntime.InvokeVoidAsync("designerInterop.removeItem", LegacyDocumentPrefix + name);
        await jsRuntime.InvokeVoidAsync("designerInterop.removeItem", EnvelopePrefix + name);
        await jsRuntime.InvokeVoidAsync("designerInterop.removeItem", VersionsPrefix + name);

        var recentNames = ((await jsRuntime.InvokeAsync<List<string>>("designerInterop.getJson", IndexKey)) ?? [])
            .Where(existing => !string.Equals(existing, name, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        await jsRuntime.InvokeVoidAsync("designerInterop.setJson", IndexKey, recentNames);
    }

    public async Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var envelopes = await GetLocalVersionsAsync(name);
        return envelopes
            .OrderByDescending(static envelope => envelope.Version)
            .Select(static envelope => new DesignVersionInfo(
                envelope.Version,
                envelope.LastModified,
                DesignDocumentSerializer.Serialize(envelope.Document).Length))
            .ToArray();
    }

    public async Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
    {
        if (string.IsNullOrWhiteSpace(name) || version <= 0)
        {
            return null;
        }

        var envelopes = await GetLocalVersionsAsync(name);
        var target = envelopes.FirstOrDefault(item => item.Version == version);
        if (target is null)
        {
            return null;
        }

        return await SaveAsync(name, target.Document, expectedETag: null);
    }

    public async Task<IReadOnlyList<(string Name, string Json)>> GetDocumentsAsync()
    {
        var documents = new List<(string Name, string Json)>();
        var names = await GetRecentAsync();
        foreach (var item in names)
        {
            var envelope = await LoadAsync(item.Name);
            if (envelope is not null)
            {
                documents.Add((envelope.Name, DesignDocumentSerializer.Serialize(envelope.Document)));
            }
        }

        return documents;
    }

    private async Task<List<DesignDocumentEnvelope>> GetLocalVersionsAsync(string name)
    {
        var rawVersions = await jsRuntime.InvokeAsync<List<string>>("designerInterop.getJson", VersionsPrefix + name) ?? [];
        var envelopes = rawVersions
            .Select(DeserializeEnvelope)
            .Where(static envelope => envelope is not null)
            .Cast<DesignDocumentEnvelope>()
            .OrderBy(static envelope => envelope.Version)
            .ToList();

        if (envelopes.Count > 0)
        {
            return envelopes;
        }

        var current = await LoadAsync(name, version: null);
        if (current is null)
        {
            return [];
        }

        return [current];
    }

    private static string SerializeEnvelope(DesignDocumentEnvelope envelope)
    {
        var payload = new LocalEnvelopePayload
        {
            Name = envelope.Name,
            Version = envelope.Version,
            ETag = envelope.ETag,
            LastModified = envelope.LastModified,
            Document = DesignDocumentSerializer.Serialize(envelope.Document)
        };

        return System.Text.Json.JsonSerializer.Serialize(payload);
    }

    private static DesignDocumentEnvelope? DeserializeEnvelope(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<LocalEnvelopePayload>(json);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Name) || string.IsNullOrWhiteSpace(payload.Document))
            {
                return null;
            }

            return new DesignDocumentEnvelope(
                payload.Name,
                payload.Version <= 0 ? 1 : payload.Version,
                string.IsNullOrWhiteSpace(payload.ETag) ? $"local-v{Math.Max(1, payload.Version)}" : payload.ETag,
                payload.LastModified == default ? DateTimeOffset.UtcNow : payload.LastModified,
                DesignDocumentSerializer.Deserialize(payload.Document));
        }
        catch
        {
            return null;
        }
    }

    private sealed class LocalEnvelopePayload
    {
        public string Name { get; set; } = string.Empty;

        public int Version { get; set; }

        public string ETag { get; set; } = string.Empty;

        public DateTimeOffset LastModified { get; set; }

        public string Document { get; set; } = string.Empty;
    }
}
