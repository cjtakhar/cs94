using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ServerlessAPIAzureFunction
{
    public class Function1
    {
        const string MyRoute = "v1/person/";
        private ILogger<Function1> _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("GetFunction1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = MyRoute)] HttpRequestData req, string name)
        {
            _logger.LogWarning("GetFunction1: C# HTTP GET trigger function processed a request.");

            HttpResponseData response;

            // Not likely to happen because the route is defined with a parameter
            if (name == null)
            {
                _logger.LogError("GetFunction1: Name was null");

                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Please pass a name in the url");
                return response;
            }

            // Fetching the name from the path parameter in the request URL
            _logger.LogWarning("GetFunction1: Name was provided {name}", name);

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"HTTP GET: Hello {name}");

            return response;
        }

    }
}
