using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;

namespace Agterhuis.Ui.Designer.Api.Extensions;

internal static class HttpResponseDataExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<HttpResponseData> CreateJsonResponseAsync<T>(this HttpRequestData request, HttpStatusCode statusCode, T payload, CancellationToken cancellationToken)
    {
        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        await response.WriteStringAsync(json, cancellationToken, Encoding.UTF8);
        return response;
    }
}
