using AppInsightsDemo;
using AppInsightsDemo.CustomSettings;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Access settings
// Using custom settings so additional settings can be provided
ApplicationInsights applicationInsightsSettings = builder.Configuration.GetSection(key: nameof(ApplicationInsights)).Get<ApplicationInsights>() ?? new ApplicationInsights();
ApplicationInsightsServiceOptions aiOptions = new ApplicationInsightsServiceOptions();

// Configures adaptive sampling
// Disabling adaptive sampling allows all events to be recorded
aiOptions.EnableAdaptiveSampling = applicationInsightsSettings.EnableAdaptiveSampling;

// Configure the connection string which provides the key necessary to connect to the application insights instance
aiOptions.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

// The following line enables Application Insights telemetry collection.
builder.Services.AddApplicationInsightsTelemetry(aiOptions);

// Register custom TelemetryInitializer to provide role name when running locally
builder.Services.AddSingleton<ITelemetryInitializer, DevelopmentRoleNameTelemetryInitializer>();

// Setup live monitoring key so authentication is enabled allowing filtering of events
builder.Services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) =>
{
    module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey;
});

// DEMO: Setup snapshot debugging
if (applicationInsightsSettings.EnableSnapshotCollectorInSdk)
{
    builder.Services.AddSnapshotCollector((configuration)
        => builder.Configuration
        .Bind(nameof(SnapshotCollectorConfiguration), configuration));
}

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddOpenApi();

// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "App Insights Demo API",
        Version = "v1.0",
        Description = "App Insights Demo example"
    });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add the class that represents the settings for the CustomerLimits section 
// in the JSON settings
CustomerLimits customerLimits = new CustomerLimits();
builder.Configuration.GetSection(nameof(CustomerLimits)).Bind(customerLimits);
builder.Services.AddSingleton(implementationInstance: customerLimits);

var app = builder.Build();

app.Logger.LogTrace("EnableSnapshotCollectorInSdk [{EnableSnapshotCollectorInSdk}]", applicationInsightsSettings.EnableSnapshotCollectorInSdk);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
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