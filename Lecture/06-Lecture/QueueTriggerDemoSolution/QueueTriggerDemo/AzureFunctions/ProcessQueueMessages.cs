using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace QueueTriggerDemo.AzureFunctions
{
    public class ProcessQueueMessages
    {
        private readonly IConfiguration _configuration;
        private readonly MySettings _mySettings;
        private readonly ILogger<ProcessQueueMessages> _logger;

        public ProcessQueueMessages(IConfiguration configuration,
                                    IOptions<MySettings> mySettings,
                                    ILogger<ProcessQueueMessages> logger)
        {
            _configuration = configuration;
            _mySettings = mySettings.Value;
            _logger = logger;
        }

        [Function("ProcessQueueMessageFunction")]
        public void Run([QueueTrigger("myqueue-items-function", Connection = "StorageConnections")] string myQueueItem)
        {
            // Demo: Access connection string settings within the azure function,
            // only displaying first 60 characters so secrets are not exposed
            _logger.LogWarning("Bad Message String: [{badMessageString} ...]", _mySettings.BadMessageString);

            if (myQueueItem.Contains(_mySettings.BadMessageString))
            {
                throw new Exception("Bad Robot!");
            }

            _logger.LogWarning($"ProcessQueueMessageFunction: C# Queue trigger function processed: [{myQueueItem}]");
        }
    }
}
