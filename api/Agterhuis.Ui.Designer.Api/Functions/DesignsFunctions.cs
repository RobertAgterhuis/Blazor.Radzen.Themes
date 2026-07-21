using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Agterhuis.Ui.Designer.Api.Extensions;
using Agterhuis.Ui.Designer.Api.Storage;
using Agterhuis.Ui.Designer.Persistence;

namespace Agterhuis.Ui.Designer.Api.Functions;

public sealed class DesignsFunctions(BlobDesignStore designStore)
{
    [Function("ListDesigns")]
    public async Task<HttpResponseData> ListDesigns(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designs")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var items = await designStore.ListAsync(cancellationToken);
        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, items, cancellationToken);
    }

    [Function("GetDesign")]
    public async Task<HttpResponseData> GetDesign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designs/{name}")] HttpRequestData request,
        string name,
        CancellationToken cancellationToken)
    {
        var query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
        var versionRaw = query["version"];
        var hasVersion = int.TryParse(versionRaw, out var version) && version > 0;

        var envelope = await designStore.LoadAsync(name, hasVersion ? version : null, cancellationToken);
        if (envelope is null)
        {
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, envelope, cancellationToken);
    }

    [Function("PutDesign")]
    public async Task<HttpResponseData> PutDesign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "designs/{name}")] HttpRequestData request,
        string name,
        CancellationToken cancellationToken)
    {
        var body = await request.ReadFromJsonAsync<DesignDocumentEnvelope>(cancellationToken: cancellationToken);
        if (body?.Document is null)
        {
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var expectedEtag = request.Headers.TryGetValues("If-Match", out var values)
            ? values.FirstOrDefault()
            : null;

        var forceSave = string.Equals(request.Headers.TryGetValues("X-Force-Save", out var forceValues) ? forceValues.FirstOrDefault() : null, "true", StringComparison.OrdinalIgnoreCase);
        var result = await designStore.SaveAsync(name, body.Document, expectedEtag, forceSave, cancellationToken);

        if (result.IsConflict)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Conflict, result.ServerEnvelope, cancellationToken);
        }

        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, result.Envelope, cancellationToken);
    }

    [Function("DeleteDesign")]
    public async Task<HttpResponseData> DeleteDesign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "designs/{name}")] HttpRequestData request,
        string name,
        CancellationToken cancellationToken)
    {
        var deleted = await designStore.SoftDeleteAsync(name, cancellationToken);
        return deleted
            ? request.CreateResponse(HttpStatusCode.NoContent)
            : request.CreateResponse(HttpStatusCode.NotFound);
    }

    [Function("GetDesignVersions")]
    public async Task<HttpResponseData> GetDesignVersions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "designs/{name}/versions")] HttpRequestData request,
        string name,
        CancellationToken cancellationToken)
    {
        var versions = await designStore.GetVersionsAsync(name, cancellationToken);
        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, versions, cancellationToken);
    }

    [Function("RestoreDesignVersion")]
    public async Task<HttpResponseData> RestoreDesignVersion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "designs/{name}/restore/{version:int}")] HttpRequestData request,
        string name,
        int version,
        CancellationToken cancellationToken)
    {
        var envelope = await designStore.RestoreAsync(name, version, cancellationToken);
        if (envelope is null)
        {
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, envelope, cancellationToken);
    }
}
