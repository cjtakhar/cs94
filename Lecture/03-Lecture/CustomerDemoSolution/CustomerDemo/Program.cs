using CustomerDemo.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// DEMO: Enable to allow manual handling of model binding errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// DEMO: Enable multi-stream read
builder.Services.AddTransient<EnableMultipleStreamReadMiddleware>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    // Add nice title
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer Custom Validation Demo API", Version = "v1" });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add anything needed only during development here
}

// DEMO: Enable multi-stream read
app.UseMultipleStreamReadMiddleware();

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
