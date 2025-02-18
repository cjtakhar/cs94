using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using NoteKeeper.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Read OpenAI settings from the configuration sources.
var aiSettings = builder.Configuration.GetSection("OpenAI").Get<AISettings>();

// Debugging logs to check where values are being pulled from
Console.WriteLine($"[DEBUG] OpenAI__ApiKey Loaded: {(string.IsNullOrWhiteSpace(aiSettings?.ApiKey) ? "NOT FOUND" : "LOADED")}");
Console.WriteLine($"[DEBUG] OpenAI__Endpoint Loaded: {(string.IsNullOrWhiteSpace(aiSettings?.Endpoint) ? "NOT FOUND" : "LOADED")}");

// Ensure OpenAI settings are set; throw an exception if required settings are missing.
if (aiSettings == null || string.IsNullOrWhiteSpace(aiSettings.ApiKey) || string.IsNullOrWhiteSpace(aiSettings.Endpoint))
{
    throw new InvalidOperationException("OpenAI settings are missing. Ensure they are set in Azure Application Settings, appsettings.json, or secrets.json.");
}

// Register AISettings with the dependency injection container.
builder.Services.Configure<AISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AISettings>>().Value);

// Register an HTTP client for OpenAI API communication with API key authentication.
builder.Services.AddHttpClient("OpenAI", (provider, client) =>
{
    var settings = provider.GetRequiredService<AISettings>();

    if (string.IsNullOrWhiteSpace(settings.Endpoint))
    {
        throw new InvalidOperationException("OpenAI Endpoint is missing. Ensure it is set in environment variables or appsettings.json.");
    }

    client.BaseAddress = new Uri(settings.Endpoint);
    client.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Register necessary services including controllers, Swagger, and CORS.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Note Keeper API", Version = "v1" });
});

// Configure Cross-Origin Resource Sharing (CORS) to allow requests from the React frontend.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add anything needed only during development here
}

// Enable Swagger middleware for API documentation.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Note Keeper API V1");
    c.RoutePrefix = string.Empty;
});

// Enable authorization and map controllers.
app.UseAuthorization();
app.MapControllers();

// Apply the configured CORS policy.
app.UseCors("AllowReactApp");

// Run the application.
app.Run();