using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using AzureBlobManagedIdentity.Repositories;
using AzureBlobManagedIdentity.Settings;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    // Add nice title
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"Azure Blob Storage Managed Identity Demo API {GlobalSettings.ServerStartDateTime.ToLongTimeString()}", Version = "v1" });

    // Add documentation via C# XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Note: Don't forget to add this to the csproj file (removing /* and */)
    /*
      	<!-- DEMO: Enable documentation file so API has user defined documentation via XML comments -->
	    <PropertyGroup>
		    <GenerateDocumentationFile>true</GenerateDocumentationFile>

		<!-- Disable warning: Missing XML comment for publicly visible type or member 'Type_or_Member'-->
		<NoWarn>$(NoWarn);1591</NoWarn>
	    </PropertyGroup>
    */
});

// Get the settings needed for the BlobContainerClient
StorageAccountSettings storageAccountSettings = builder.Configuration.GetSection(nameof(StorageAccountSettings)).Get<StorageAccountSettings>();
PictureSettings pictureSettings = builder.Configuration.GetSection(nameof(PictureSettings)).Get<PictureSettings>();

// Create the container endpoint using to point to the container specified in the
// Picture Settings configuration
string containerEndPoint = string.Format(storageAccountSettings.ContainerEndpoint, pictureSettings.PictureContainerName);

// There are two demos below
// DEMO 1 shows how to use MANAGED IDENTITIES
// DEMO 2 shows how to use the ACCOUNT NAME and ACCOUNT KEY
bool isDemo1 = true;

// DEMO 1: Use MANAGED IDENTITIES
// Setup the azure credentials using the TenantId that the storage account resides in
if (isDemo1)
{
    var managedIdentityCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        SharedTokenCacheTenantId = storageAccountSettings.TenantId,
        VisualStudioCodeTenantId = storageAccountSettings.TenantId,
        VisualStudioTenantId = storageAccountSettings.TenantId
    });


    // DEMO 1: Use MANAGED IDENTITIES
    // Create the container if its not present using MANAGED IDENTITIES
    var blobServiceClient = new BlobServiceClient(new Uri(storageAccountSettings.Url), managedIdentityCredential);
    var blobContainerClient = new BlobContainerClient(new Uri(containerEndPoint), managedIdentityCredential);
    if (!blobContainerClient.Exists())
    {
        var _ = blobServiceClient.CreateBlobContainer(pictureSettings.PictureContainerName);
    }


    // DEMO 1: Use MANAGED IDENTITIES
    // Register the BlobContainerClient with dependency injection using MANAGED IDENTITIES
    builder.Services.AddSingleton<BlobContainerClient>(new BlobContainerClient(new Uri(containerEndPoint), managedIdentityCredential));
}

// DEMO 2: Use ACCOUNT KEY and ACCOUNT NAME
if (!isDemo1)
{
    Azure.Storage.StorageSharedKeyCredential sharedKeyCredential =
     new StorageSharedKeyCredential(storageAccountSettings.AccountName, storageAccountSettings.AccountKey);

    // DEMO 2: Use ACCOUNT KEY and ACCOUNT NAME
    // Create the container if its not present using ACCOUNT KEY and ACCOUNT NAME
    var blobServiceClient = new BlobServiceClient(new Uri(storageAccountSettings.Url), sharedKeyCredential);
    var blobContainerClient = new BlobContainerClient(new Uri(containerEndPoint), sharedKeyCredential);
    if (!blobContainerClient.Exists())
    {
        var _ = blobServiceClient.CreateBlobContainer(pictureSettings.PictureContainerName);
    }

    // DEMO 2: Use ACCOUNT KEY and ACCOUNT NAME
    // Register the BlobContainerClient with dependency injection using ACCOUNT KEY and ACCOUNT NAME
    builder.Services.AddSingleton<BlobContainerClient>(new BlobContainerClient(new Uri(containerEndPoint), sharedKeyCredential));
}

// Configure Repositories
// Scoped indicates an the instance is the same instance for the request but different across requests
builder.Services.AddScoped(typeof(IStorageRepository), typeof(StorageRepository));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add anything needed only during development here
}

// Code Note: Moved outside of env.IsDevelopment() so both 
// Debug and Release are supported
app.UseSwagger();

// Customize the UseSwaggerUI() 
app.UseSwaggerUI(c =>
{
    // 1. Display a friendly title
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");

    // Code Note: 
    // Launch the Swagger UI by default
    // Serving the Swagger UI at the app's root 
    // (http://localhost:<port>)
    c.RoutePrefix = string.Empty;
});

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public static class GlobalSettings
{
    public static DateTime ServerStartDateTime = DateTime.UtcNow;
}