using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJobNetCoreDemo.WebJobFunctions
{
    /// <summary>
    /// Illustrates reading messages from a queue and logging them
    /// </summary>
    public class SimpleQueueDemoFunctions
    {
        private ILogger _logger;

        /// <summary>
        /// Demo of receiving messages from the message queue
        /// </summary>
        /// <param name="logger">The logger</param>
        public SimpleQueueDemoFunctions(ILogger<SimpleQueueDemoFunctions> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a message from the queue and logs it as a warning
        /// </summary>
        /// <param name="message">The message</param>        
        public void ProcessQueueMessage([QueueTrigger("%webjob-queueddemo-name%")] string message)
        {
            _logger.LogWarning("Queued Message: [{message}]", message);
        }

    }
}
