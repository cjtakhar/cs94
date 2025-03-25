using System.Net;
using System.Text.Json;
using System.Web;
using HelloWorldAzureFunctions2.DataTransferObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HelloWorldAzureFunctions2
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("HelloWorld")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string? name = string.Empty;

            if (req.Method == "GET")
            {
                var queryParameters = req.Url.Query;
                name = HttpUtility.ParseQueryString(queryParameters)["name"];
            }
            else if (req.Method == "POST")
            {
                string requestBody = await req.ReadAsStringAsync() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    var data = JsonSerializer.Deserialize<JsonElement>(requestBody);
                    JsonElement jsonElement;
                    if (data.TryGetProperty("name", out jsonElement))
                    {
                        name = jsonElement.GetString();
                    }
                }
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.MethodNotAllowed);
            }

            HttpResponseData response;

            if (string.Compare(name, "Exception", StringComparison.OrdinalIgnoreCase) == 0)
            {
                ErrorResponse errorResponse = new ErrorResponse
                {
                    errorDescription = "Exception occurred",
                    errorNumber = 1000,
                    parameterName = "name",
                    parameterValue = name ?? string.Empty
                };
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse));
            }
            else if (string.IsNullOrWhiteSpace(name))
            {
                ErrorResponse errorResponse = new ErrorResponse
                {
                    errorDescription = "The name was not provided, please specify a value for the name.",
                    errorNumber = 1,
                    parameterName = "name",
                    parameterValue = string.Empty
                };
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse));
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Hello, {name}");
            }

            return response;
        }
    }
}
