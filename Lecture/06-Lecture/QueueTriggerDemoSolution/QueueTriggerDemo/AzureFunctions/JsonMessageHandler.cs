using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using QueueTriggerDemo.Models;

namespace QueueTriggerDemo.AzureFunctions
{
    public class JsonMessageHandler
    {
        private readonly IConfiguration _configuration;
        private readonly MySettings _mySettings;
        private readonly ILogger<ProcessQueueMessages> _logger;

        public JsonMessageHandler(IConfiguration configuration,
                                    IOptions<MySettings> mySettings,
                                    ILogger<ProcessQueueMessages> logger)
        {
            _configuration = configuration;
            _mySettings = mySettings.Value;
            _logger = logger;
        }

        [Function("ProcessJsonQueueMessageFunction")]
        public void Run([QueueTrigger("myjsonqueue-items-function", Connection = "StorageConnections")] JsonMessage myJsonMessage)
        {
            _logger.LogWarning("Bad Message String: [{badMessageString} ...]", _mySettings.BadMessageString);

            if (myJsonMessage.Message.Contains(_mySettings.BadMessageString))
            {
                throw new Exception("Bad Robot!");
            }

            _logger.LogWarning($"ProcessQueueMessageFunction: C# Queue trigger function processed: Message: [{myJsonMessage.Message}] DateAdded: [{myJsonMessage.MessageAddedDateTime}]");
        }
    }
}
