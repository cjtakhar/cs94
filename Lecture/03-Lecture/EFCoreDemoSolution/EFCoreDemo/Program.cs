using AppInsightsDemo.CustomSettings;
using EFCoreDemo;
using EFCoreDemo.CustomSettings;
using EFCoreDemo.Data;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    // Add nice title
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EF Core Demo 1a", Version = "v1" });

    // Add documentation via C# XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// DEMO: Entity Framework: Register the context with dependency injection
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
builder.Services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(connectionString));

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

// Get the dependency injection for creating services
using (var scope = app.Services.CreateScope())
{
    // Get the service provider so services can be called
    var services = scope.ServiceProvider;
    try
    {
        // Get the database context service
        var context = services.GetRequiredService<MyDatabaseContext>();

        // Initialize the data
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
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

app.Run();
