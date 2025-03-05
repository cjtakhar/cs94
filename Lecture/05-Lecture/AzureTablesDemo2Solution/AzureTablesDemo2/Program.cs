using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Azure;
using AzureTables2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add nice title
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure Tables Demo API", Version = "v1" });

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

// Register the table service client
builder.Services.AddAzureClients(c =>
    {
        c.AddTableServiceClient(
         builder.Configuration.GetConnectionString(TableConstants.TABLE_CONNECTION_STRING_NAME));        
    }
);

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
