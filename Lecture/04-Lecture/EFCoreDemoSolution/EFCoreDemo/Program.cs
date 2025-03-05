using AppInsightsDemo.CustomSettings;
using Azure.Identity;
using EFCoreDemo;
using EFCoreDemo.CustomSettings;
using EFCoreDemo.Data;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;

Stopwatch startupTime = Stopwatch.StartNew();

var builder = WebApplication.CreateBuilder(args);

// Access settings
// Using custom settings so additional settings can be provided
ApplicationInsights applicationInsightsSettings = builder.Configuration.GetSection(key: nameof(ApplicationInsights)).Get<ApplicationInsights>() ?? new ApplicationInsights();

ApplicationInsightsServiceOptions aiOptions = new ApplicationInsightsServiceOptions();

// Configures adaptive sampling, disabling adaptive sampling allows all events to be recorded
aiOptions.EnableAdaptiveSampling = applicationInsightsSettings.EnableAdaptiveSampling;

// Configure the connection string which provides the key necessary to connect to the application insights instance
aiOptions.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

// Setup app insights
builder.Services.AddApplicationInsightsTelemetry(aiOptions);

// Register custom TelemetryInitializer to provide role name when running locally
builder.Services.AddSingleton<ITelemetryInitializer, DevelopmentRoleNameTelemetryInitializer>();

// Setup live monitoring key so authentication is enabled allowing filtering of events
builder.Services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) =>
{
    module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey;
});

// Setup live monitoring key so authentication is enabled allowing filtering of events
builder.Services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) => module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey);

// DEMO: Setup snapshot debugging
builder.Services.AddSnapshotCollector((configuration)
    => builder.Configuration
    .Bind(nameof(SnapshotCollectorConfiguration), configuration));

if (builder.Environment.IsDevelopment())
{
    // Add console logging
    builder.Logging.AddConsole();
}

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This enables enumerations to appear as strings in the swagger UI
        // Also adds support for string to enum and vice versa
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Using the EndpointsApiExplorer to generate the OpenAPI document
// because there are some harmless 1st chance exceptions that occur
// this removes them.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Add nice title
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"EF Core Demo - Managed Identity {GlobalSettings.ServerStartDateTime.ToLongTimeString()}", Version = "v1" });

    // Add documentation via C# XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

// DEMO 1: Simple approach all variants of Azure credentials are tried
// Entity Framework: Register the context with dependency injection
// Note: Connection string must include: Authentication=Active Directory Default;
//builder.Services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(connectionString));

// DEMO 2: Improved startup approach
// Only the specific Azure credentials needed for development vs production are used.
// Note: Connection string must NOT include: Authentication=Active Directory Default;
ImprovedStartupPerformanceApproach(builder, connectionString);

// DEMO: Custom Settings
// Add the class that represents the settings for the CustomerLimits section 
// in the JSON settings

// 3. Instantiate the class that models the JSON Settings
CustomerLimits customerLimits = new CustomerLimits();

// 4. Bind to the settings
builder.Configuration.GetSection(nameof(CustomerLimits)).Bind(customerLimits);

// 5. Use dependency injection to inject a singleton
builder.Services.AddSingleton(implementationInstance: customerLimits);

var app = builder.Build();

ILogger logger;

// Get the dependency injection for creating services
using (var scope = app.Services.CreateScope())
{
    // Get the service provider so services can be called
    var services = scope.ServiceProvider;

    logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Get the database context service
        var context = services.GetRequiredService<MyDatabaseContext>();

        // Initialize the data
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "*** ERROR *** An error occurred while seeding the database. *** ERROR ***");
    }
}

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

startupTime.Stop();
logger.LogWarning("*** Application startup time: {StartupTime} ms ***", startupTime.ElapsedMilliseconds);

app.Run();

/// <summary>
/// This method improves startup performance by 
/// customizing the Azure credential chain
/// </summary>
void ImprovedStartupPerformanceApproach(WebApplicationBuilder builder, string connectionString)
{
    /*
        Explanation:
            Transient Lifetime: Services registered with the Transient lifetime are 
            created each time they are requested. This means that a new instance of the 
            TokenRefreshingInterceptor will be created every time it is injected into a class
            or requested from the service provider.

        Why Transient is Used:
        1.	Statelessness: TokenRefreshingInterceptor is likely stateless or has minimal state.
            Transient services are ideal for lightweight, stateless services that do not hold 
            any state between requests.
        2.	Fresh Instances: Each request for TokenRefreshingInterceptor gets a new instance, 
            ensuring that any state or configuration specific to a single request does not affect other requests.
        3.	Interceptors: In the context of Entity Framework Core, interceptors like 
            TokenRefreshingInterceptor are often used to modify or inspect database operations.
            Using a transient lifetime ensures that each database operation gets a fresh interceptor instance,
            which can be important for maintaining clean and predictable behavior.

        Example Scenario:
            If TokenRefreshingInterceptor needs to refresh a token for each database connection, 
            having it as a transient service ensures that each database connection gets a new interceptor 
            instance, which can handle token refreshing independently.

        Summary:
            Using AddTransient for TokenRefreshingInterceptor ensures that a new instance is 
            created each time it is needed, which is suitable for stateless services or services 
            that need to be isolated per request or operation.
    */
    builder.Services.AddTransient<TokenRefreshingInterceptor>();

    // Registering the DefaultAzureCredential as a singleton service
    // because it contains no modifiable state and is thread-safe.
    builder.Services.AddSingleton<DefaultAzureCredential>(sp =>
    {
        DefaultAzureCredentialOptions azureCredentialOptions;

        // Customize the credential chain to only use what you need
        // This reduces startup time by about 3 times
        // On my machine this reduced startup time from 18 seconds to 6 seconds
        if (builder.Environment.IsDevelopment())
        {
            // Customize the credential chain to only use what you need
            azureCredentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeInteractiveBrowserCredential = true,

                // Enable as this is what is being used locally
                ExcludeAzureCliCredential = false,

                ExcludeAzureDeveloperCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeWorkloadIdentityCredential = true,
                ExcludeVisualStudioCredential = true

            };
        }
        else
        {
            // Customize the credential chain to only use what you need
            azureCredentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,

                // Enable: Managed identity is used in production
                ExcludeManagedIdentityCredential = false,

                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeAzureCliCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeWorkloadIdentityCredential = true,
                ExcludeVisualStudioCredential = true
            };
        }
        return new DefaultAzureCredential(azureCredentialOptions);
    });

    // Register DB context with dependency injection
    builder.Services.AddDbContext<MyDatabaseContext>((sp, options) =>
    {
        // Use the connection string provided
        options.UseSqlServer(connectionString);

        // Resolve the interceptor from DI and add it.
        var interceptor = sp.GetRequiredService<TokenRefreshingInterceptor>();
        options.AddInterceptors(interceptor);
    });
}

public static class GlobalSettings
{
    public static DateTime ServerStartDateTime = DateTime.UtcNow;
}