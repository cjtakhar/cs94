using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebJobNetCoreDemo.CustomSettings;

// Create the host builder to run the webjob
var builder = new HostBuilder();

// Configure logging
builder.ConfigureLogging((context, b) =>
{   
    // Force log level to warning SDK is generating a lot of
    // noise and doesn't appear to honor config even with the
    // following commented out line
    //b.AddConfiguration(context.Configuration);
    b.SetMinimumLevel(LogLevel.Warning);
    b.AddConsole();
});

// Configure the webjob to support storage
// Configure to support queue triggering
builder.ConfigureWebJobs(b =>
{
    b.AddAzureStorageBlobs();
    b.AddAzureStorageQueues();
});

// Get the environment variable that defines if we are using Development, Production etc...
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null)
{
    builder.UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
}

// Configure settings locations
builder.ConfigureAppConfiguration((context, configurationBuilder) =>
{    
    configurationBuilder
        .AddJsonFile($"appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    if (context.HostingEnvironment.IsDevelopment())
    {
        configurationBuilder
            .AddUserSecrets<Program>();
    }
});

// Configure dependency injection
builder.ConfigureServices((context, s) =>
{
    ConfigureServices(context, s);
    s.BuildServiceProvider();
});

// Run the webjob
var host = builder.Build();
using (host)
{
    await host.RunAsync();
}

/// <summary>
/// Register items to be injected
/// </summary>
void ConfigureServices(HostBuilderContext context, IServiceCollection s)
{
    // Configure storage settings to use the same settings as the SDK
    StorageSettings storageSettings = new StorageSettings();
    storageSettings.ConnectionString = context.Configuration.GetValue<string>("AzureWebJobsStorage");
    s.AddSingleton<IStorageSettings>(storageSettings);    
}