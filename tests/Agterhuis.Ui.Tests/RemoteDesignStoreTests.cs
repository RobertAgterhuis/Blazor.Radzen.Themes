using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;

namespace Agterhuis.Ui.Tests;

public sealed class RemoteDesignStoreTests
{
    [Fact]
    public async Task SaveAsync_ReturnsEnvelope_OnSuccess()
    {
        var envelope = new DesignDocumentEnvelope("Demo", 2, "etag-2", DateTimeOffset.UtcNow, CreateDocument("Demo"));
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            return Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, envelope));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var store = new RemoteDesignStore(httpClient);

        var result = await store.SaveAsync("Demo", CreateDocument("Demo"), "etag-1");

        Assert.Equal(2, result.Version);
        Assert.Equal("etag-2", result.ETag);
    }

    [Fact]
    public async Task SaveAsync_ThrowsConflict_On409()
    {
        var serverEnvelope = new DesignDocumentEnvelope("Demo", 3, "etag-3", DateTimeOffset.UtcNow, CreateDocument("Server"));
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(CreateJsonResponse(HttpStatusCode.Conflict, serverEnvelope)));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var store = new RemoteDesignStore(httpClient);

        var ex = await Assert.ThrowsAsync<DesignConflictException>(() => store.SaveAsync("Demo", CreateDocument("Local"), "etag-1"));

        Assert.NotNull(ex.ServerEnvelope);
        Assert.Equal(3, ex.ServerEnvelope!.Version);
    }

    [Fact]
    public async Task GetRecentAsync_Throws_OnNetworkFailure()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("network timeout"));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var store = new RemoteDesignStore(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => store.GetRecentAsync());
    }

    private static HttpResponseMessage CreateJsonResponse<T>(HttpStatusCode statusCode, T payload)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static DesignDocument CreateDocument(string name)
    {
        return new DesignDocument
        {
            Name = name,
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = name,
                    Nodes = []
                }
            ]
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => responder(request, cancellationToken);
    }
}
