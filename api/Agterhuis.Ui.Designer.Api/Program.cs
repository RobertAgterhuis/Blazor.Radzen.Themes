using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using Agterhuis.Ui.Designer.Api.Storage;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
    var connectionString = configuration["AzureWebJobsStorage"];
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("AzureWebJobsStorage ontbreekt in de Functions-configuratie.");
    }

    return new BlobServiceClient(connectionString);
});
builder.Services.AddSingleton<BlobDesignStore>();

builder.Build().Run();
