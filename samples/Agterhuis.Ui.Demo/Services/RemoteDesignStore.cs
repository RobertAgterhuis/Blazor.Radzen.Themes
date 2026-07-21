using System.Net;
using System.Net.Http.Json;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;

namespace Agterhuis.Ui.Demo.Services;

public sealed class RemoteDesignStore(HttpClient httpClient) : IDesignStore
{
    public async Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
    {
        var items = await httpClient.GetFromJsonAsync<IReadOnlyList<DesignListItem>>("api/designs");
        return items ?? [];
    }

    public async Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var query = version is int requestedVersion && requestedVersion > 0
            ? $"?version={requestedVersion}"
            : string.Empty;

        using var response = await httpClient.GetAsync($"api/designs/{Uri.EscapeDataString(name)}{query}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DesignDocumentEnvelope>();
    }

    public async Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(document);

        var payload = new DesignDocumentEnvelope(
            name,
            0,
            expectedETag ?? string.Empty,
            DateTimeOffset.UtcNow,
            document);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/designs/{Uri.EscapeDataString(name)}")
        {
            Content = JsonContent.Create(payload)
        };

        if (!string.IsNullOrWhiteSpace(expectedETag))
        {
            request.Headers.TryAddWithoutValidation("If-Match", expectedETag);
        }
        else
        {
            request.Headers.TryAddWithoutValidation("X-Force-Save", "true");
        }

        using var response = await httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            DesignDocumentEnvelope? serverEnvelope = null;
            try
            {
                serverEnvelope = await response.Content.ReadFromJsonAsync<DesignDocumentEnvelope>();
            }
            catch
            {
                // No envelope returned by server; keep null.
            }

            throw new DesignConflictException("Het ontwerp is op de server gewijzigd sinds het laden.", serverEnvelope);
        }

        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<DesignDocumentEnvelope>();
        return envelope ?? throw new InvalidOperationException("Opslaan gaf geen geldig ontwerpresultaat terug.");
    }

    public async Task RemoveAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        using var response = await httpClient.DeleteAsync($"api/designs/{Uri.EscapeDataString(name)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var items = await httpClient.GetFromJsonAsync<IReadOnlyList<DesignVersionInfo>>($"api/designs/{Uri.EscapeDataString(name)}/versions");
        return items ?? [];
    }

    public async Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
    {
        if (string.IsNullOrWhiteSpace(name) || version <= 0)
        {
            return null;
        }

        using var response = await httpClient.PostAsync($"api/designs/{Uri.EscapeDataString(name)}/restore/{version}", content: null);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DesignDocumentEnvelope>();
    }
}
