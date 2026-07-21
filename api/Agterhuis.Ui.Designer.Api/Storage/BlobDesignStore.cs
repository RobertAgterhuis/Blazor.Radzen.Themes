using System.Net;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Api.Storage;

public sealed class BlobDesignStore(BlobServiceClient blobServiceClient)
{
    private const string ContainerName = "designs";
    private const int MaxVersions = 20;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<DesignListItem>> ListAsync(CancellationToken cancellationToken)
    {
        var container = await EnsureContainerAsync(cancellationToken);
        var items = new List<DesignListItem>();
        await foreach (var blob in container.GetBlobsAsync(prefix: string.Empty, cancellationToken: cancellationToken))
        {
            if (!blob.Name.EndsWith("/_meta.json", StringComparison.Ordinal))
            {
                continue;
            }

            var metaBlob = container.GetBlobClient(blob.Name);
            var meta = await ReadMetaAsync(metaBlob, cancellationToken);
            if (meta is null || meta.IsDeleted)
            {
                continue;
            }

            items.Add(new DesignListItem(meta.Name, meta.LastModified, meta.CurrentVersion));
        }

        return items
            .OrderByDescending(static item => item.LastModified)
            .ThenBy(static item => item.Name, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version, CancellationToken cancellationToken)
    {
        var container = await EnsureContainerAsync(cancellationToken);
        var metaBlob = container.GetBlobClient(GetMetaPath(name));
        var meta = await ReadMetaAsync(metaBlob, cancellationToken);
        if (meta is null || meta.IsDeleted)
        {
            return null;
        }

        var resolvedVersion = version ?? meta.CurrentVersion;
        if (resolvedVersion <= 0)
        {
            return null;
        }

        var versionBlob = container.GetBlobClient(GetVersionPath(name, resolvedVersion));
        if (!await versionBlob.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var json = await DownloadTextAsync(versionBlob, cancellationToken);
        var document = DesignDocumentSerializer.Deserialize(json);
        return new DesignDocumentEnvelope(name, resolvedVersion, meta.ETag, meta.LastModified, document);
    }

    public async Task<SaveResult> SaveAsync(string name, DesignDocument document, string? expectedEtag, bool forceSave, CancellationToken cancellationToken)
    {
        var container = await EnsureContainerAsync(cancellationToken);
        var metaBlob = container.GetBlobClient(GetMetaPath(name));
        var currentMeta = await ReadMetaAsync(metaBlob, cancellationToken);

        if (!forceSave
            && !string.IsNullOrWhiteSpace(expectedEtag)
            && currentMeta is not null
            && !string.Equals(expectedEtag, currentMeta.ETag, StringComparison.Ordinal))
        {
            var serverEnvelope = await LoadAsync(name, currentMeta.CurrentVersion, cancellationToken);
            return SaveResult.Conflict(serverEnvelope);
        }

        var nextVersion = Math.Max(0, currentMeta?.CurrentVersion ?? 0) + 1;
        var now = DateTimeOffset.UtcNow;
        var payload = DesignDocumentSerializer.Serialize(document);
        var versionBlob = container.GetBlobClient(GetVersionPath(name, nextVersion));
        await UploadTextAsync(versionBlob, payload, overwrite: true, cancellationToken);

        var nextMeta = new DesignMeta
        {
            Name = name,
            CurrentVersion = nextVersion,
            LastModified = now,
            IsDeleted = false,
            ETag = string.Empty
        };

        BlobRequestConditions? conditions = null;
        if (currentMeta is not null)
        {
            conditions = new BlobRequestConditions
            {
                IfMatch = new ETag(currentMeta.ETag)
            };
        }

        try
        {
            var uploadedMeta = await UploadMetaAsync(metaBlob, nextMeta, conditions, cancellationToken);
            nextMeta.ETag = uploadedMeta.Value.ETag.ToString();
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.PreconditionFailed)
        {
            var serverEnvelope = await LoadAsync(name, version: null, cancellationToken);
            return SaveResult.Conflict(serverEnvelope);
        }

        await EnforceRetentionAsync(container, name, nextVersion, cancellationToken);

        var envelope = new DesignDocumentEnvelope(name, nextVersion, nextMeta.ETag, nextMeta.LastModified, document);
        return SaveResult.Success(envelope);
    }

    public async Task<bool> SoftDeleteAsync(string name, CancellationToken cancellationToken)
    {
        var container = await EnsureContainerAsync(cancellationToken);
        var metaBlob = container.GetBlobClient(GetMetaPath(name));
        var currentMeta = await ReadMetaAsync(metaBlob, cancellationToken);
        if (currentMeta is null)
        {
            return false;
        }

        currentMeta.IsDeleted = true;
        currentMeta.LastModified = DateTimeOffset.UtcNow;

        try
        {
            var result = await UploadMetaAsync(metaBlob, currentMeta, new BlobRequestConditions
            {
                IfMatch = new ETag(currentMeta.ETag)
            }, cancellationToken);
            currentMeta.ETag = result.Value.ETag.ToString();
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name, CancellationToken cancellationToken)
    {
        var container = await EnsureContainerAsync(cancellationToken);
        var prefix = GetDesignPrefix(name);
        var versions = new List<DesignVersionInfo>();

        await foreach (var blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            var version = TryParseVersion(name, blob.Name);
            if (version is null)
            {
                continue;
            }

            versions.Add(new DesignVersionInfo(version.Value, blob.Properties.LastModified ?? DateTimeOffset.MinValue, blob.Properties.ContentLength ?? 0));
        }

        return versions
            .OrderByDescending(static item => item.Version)
            .ToArray();
    }

    public async Task<DesignDocumentEnvelope?> RestoreAsync(string name, int version, CancellationToken cancellationToken)
    {
        var existing = await LoadAsync(name, version, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var result = await SaveAsync(name, existing.Document, expectedEtag: null, forceSave: true, cancellationToken);
        return result.Envelope;
    }

    private async Task<BlobContainerClient> EnsureContainerAsync(CancellationToken cancellationToken)
    {
        var container = blobServiceClient.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return container;
    }

    private static string GetDesignPrefix(string name) => $"{name.Trim()}/";

    private static string GetMetaPath(string name) => $"{GetDesignPrefix(name)}_meta.json";

    private static string GetVersionPath(string name, int version) => $"{GetDesignPrefix(name)}v{version}.json";

    private static int? TryParseVersion(string name, string blobName)
    {
        var prefix = GetDesignPrefix(name);
        if (!blobName.StartsWith(prefix, StringComparison.Ordinal) || !blobName.EndsWith(".json", StringComparison.Ordinal))
        {
            return null;
        }

        var fileName = blobName[prefix.Length..];
        if (!fileName.StartsWith("v", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("_meta", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var numberPart = fileName[1..^5];
        return int.TryParse(numberPart, out var parsed) ? parsed : null;
    }

    private static async Task<string> DownloadTextAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        var response = await blobClient.DownloadContentAsync(cancellationToken);
        return response.Value.Content.ToString();
    }

    private static async Task UploadTextAsync(BlobClient blobClient, string content, bool overwrite, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: overwrite, cancellationToken);
    }

    private static async Task<Response<BlobContentInfo>> UploadMetaAsync(BlobClient blobClient, DesignMeta meta, BlobRequestConditions? conditions, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(meta, JsonOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        var options = new BlobUploadOptions
        {
            Conditions = conditions
        };

        return await blobClient.UploadAsync(stream, options, cancellationToken);
    }

    private static async Task<DesignMeta?> ReadMetaAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        var meta = JsonSerializer.Deserialize<DesignMeta>(response.Value.Content, JsonOptions);
        if (meta is null)
        {
            return null;
        }

        meta.ETag = response.Value.Details.ETag.ToString();
        return meta;
    }

    private static async Task EnforceRetentionAsync(BlobContainerClient container, string name, int currentVersion, CancellationToken cancellationToken)
    {
        var minAllowed = currentVersion - MaxVersions + 1;
        if (minAllowed <= 1)
        {
            return;
        }

        for (var candidate = 1; candidate < minAllowed; candidate++)
        {
            var blob = container.GetBlobClient(GetVersionPath(name, candidate));
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private sealed class DesignMeta
    {
        public string Name { get; set; } = string.Empty;

        public int CurrentVersion { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public string ETag { get; set; } = string.Empty;
    }

    public sealed class SaveResult
    {
        private SaveResult(bool isConflict, DesignDocumentEnvelope? envelope, DesignDocumentEnvelope? serverEnvelope)
        {
            IsConflict = isConflict;
            Envelope = envelope;
            ServerEnvelope = serverEnvelope;
        }

        public bool IsConflict { get; }

        public DesignDocumentEnvelope? Envelope { get; }

        public DesignDocumentEnvelope? ServerEnvelope { get; }

        public static SaveResult Success(DesignDocumentEnvelope envelope) => new(false, envelope, null);

        public static SaveResult Conflict(DesignDocumentEnvelope? serverEnvelope) => new(true, null, serverEnvelope);
    }
}
