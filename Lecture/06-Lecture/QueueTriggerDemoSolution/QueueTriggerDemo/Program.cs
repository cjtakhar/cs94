using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueueTriggerDemo;
using Azure.Storage.Blobs;
using Azure.Identity;

var builder = FunctionsApplication.CreateBuilder(args);
// Load general app config first
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Optional: load environment-specific settings (if you have them)
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

// Local Azure Functions settings (values block, or flattened keys)
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

// Load user secrets last (to override local/dev settings securely)
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.ConfigureFunctionsWebApplication();

// Add MySettings to dependency injection
builder.Services.Configure<MySettings>(builder.Configuration.GetSection("MySettings"));

var blobServiceUri = builder.Configuration["StorageConnections:blobServiceUri"];

// Check if blobServiceUri is provided in the configuration
if (string.IsNullOrWhiteSpace(blobServiceUri))
{
    var azureWebJobsStorage = builder.Configuration["AzureWebJobsStorage"];

    // If blobServiceUri is not provided, check for AzureWebJobsStorage
    if (!string.IsNullOrWhiteSpace(azureWebJobsStorage))
    {
        // Use the AzureWebJobsStorage connection string if blobServiceUri is not provided
        builder.Services.AddSingleton(x => new BlobServiceClient(azureWebJobsStorage));
    }
    else
    {
        // If neither is provided, throw an exception
        throw new InvalidOperationException("Either blobServiceUri or AzureWebJobsStorage must be configured.");
    }
}
else
{
    // If blobServiceUri is provided, use it to create the BlobServiceClient
    builder.Services.AddSingleton(x => new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential()));
}

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
