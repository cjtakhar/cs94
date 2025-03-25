using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionTimerDemo
{
    public class TimedProcessor
    {
        private readonly ILogger _logger;

        public TimedProcessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimedProcessor>();
        }

        /// <summary>
        /// Executes once every 5 seconds
        /// </summary>
        /// <param name="myTimer">My timer.</param>
        /// <param name="log">The log.</param>
        /// <remarks>
        /// In order to run this from visual studio you will need to setup a local.settings.json file
        /// See ReadMe.txt
        /// 
        /// See: https://en.wikipedia.org/wiki/Cron#CRON_expression 
        /// The /2 means every 2 seconds
        /// The format of the CRON Expression is:
        /// {second} {minute} {hour} {day} {month} {day-of-week}
        /// The */2 in the first position means every 2 seconds
        /// </remarks>
        [Function("TimmerFunction")]
        public void Run([TimerTrigger("*/2 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
