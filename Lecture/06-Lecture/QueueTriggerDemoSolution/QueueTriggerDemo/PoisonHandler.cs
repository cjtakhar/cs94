using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace QueueTriggerDemo
{
    public class PoisonHandler
    {
        private readonly ILogger<PoisonHandler> _logger;

        public PoisonHandler(ILogger<PoisonHandler> logger)
        {
            _logger = logger;
        }

        [Function("PoisonHandler")]
        public void Run([QueueTrigger("myqueue-items-poison", Connection = "AzureWebJobsStorage")]string myQueueItem)
        {
            _logger.LogCritical($"PoisonHandler: C# Queue trigger function processed message: {myQueueItem}");
        }
    }
}
