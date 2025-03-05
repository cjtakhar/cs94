using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Azure.Storage.Queues.Models;
using Azure;

namespace AzureQueueDemo.Controllers
{
    /// <summary>
    /// Demo values controller
    /// Implements the <see cref="ControllerBase" />
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class QueuedMessagesController : ControllerBase
    {
        /// <summary>
        /// The maximum number of messages to retrieve at once
        /// </summary>
        /// <remarks>This is a fixed limit enforced by the Queue service and SDK</remarks>
        private const int MessagesToRetrieveAtOnce = 32;

        /// <summary>
        /// The queue settings
        /// </summary>
        private readonly QueueSettings _queueSettings;

        /// <summary>
        /// The customer limits settings
        /// </summary>
        private readonly LimitSettings _limitSettings;


        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedMessagesController"/> class.
        /// </summary>
        public QueuedMessagesController(LimitSettings limitSettings,
                                        QueueSettings queueSettings)
        {
            _queueSettings = queueSettings;
            _limitSettings = limitSettings;
        }


        /// <summary>
        /// The queued messages in the queuedmessages resource
        /// </summary>
        /// <returns>The queued messages in the queuedmessages resource</returns>
        [ProducesResponseType(typeof(System.Collections.IEnumerable), StatusCodes.Status200OK)]
        [Route("/api/v1/queuedmessages")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                QueueClient queueClient = new QueueClient(_queueSettings.QueueConnectionString, QueueConstants.NOTES_QUEUE);

                if (queueClient.Exists())
                {
                    // Peek does not remove items from the queue
                    Response<PeekedMessage[]>? peekedMessagesResponse = await queueClient.PeekMessagesAsync(MessagesToRetrieveAtOnce);

                    var results = (from m in peekedMessagesResponse.Value select
                         new
                         {
                             m.MessageId,
                             m.Body,
                             m.MessageText,
                             m.DequeueCount,
                             m.ExpiresOn,
                             m.InsertedOn
                         }).ToList();

                    return new ObjectResult(results);
                }

                return NotFound();
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }
        }

        /// <summary>
        /// Dequeues a single message
        /// </summary>
        /// <param name="visibilityInSeconds">The visibility in seconds.</param>
        /// <returns>
        /// The queued messages in the queuedmessages resource
        /// </returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/api/v1/queuedmessage")]
        [HttpGet]
        public async Task<IActionResult> GetQueuedMessage(int? visibilityInSeconds)
        {
            try
            {
                QueueClient queueClient = new QueueClient(_queueSettings.QueueConnectionString, QueueConstants.NOTES_QUEUE);

                if (queueClient.Exists())
                {
                    // Get does not remove items from the queue, we must delete it
                    // Get allows you to own the message for the specified time
                    Response<QueueMessage>? message = await queueClient.ReceiveMessageAsync(visibilityTimeout: new TimeSpan(hours:0,minutes:0,seconds: visibilityInSeconds ?? _limitSettings.DefaultVisibilityInSeconds));

                    if (message != null)
                    {
                        var result =
                             new
                             {
                                 message.Value?.MessageId,
                                 message.Value?.Body,
                                 message.Value?.MessageText,
                                 message.Value?.DequeueCount,
                                 message.Value?.ExpiresOn,
                                 message.Value?.InsertedOn,
                                 message.Value?.PopReceipt,
                                 message.Value?.NextVisibleOn,
                                 
                             };

                        return new ObjectResult(message);
                    }
                }
                return NoContent();
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }
        }

        /// <summary>
        /// Creates a value
        /// </summary>
        /// <param name="timeToLiveInSeconds">The amount of for the message to live</param>
        /// <param name="queuedMessageText">The queued message text.</param>
        /// <returns>
        /// The object created
        /// </returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/v1/queuedmessages")]
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] int? timeToLiveInSeconds, [FromBody] string queuedMessageText)
        {
            try
            {
                QueueClient queueClient = new QueueClient(_queueSettings.QueueConnectionString, QueueConstants.NOTES_QUEUE);

                // Create the queue if it doesn't already exist
                queueClient.CreateIfNotExists();

                // Enqueue a message
                Response<SendReceipt> response = await queueClient.SendMessageAsync(queuedMessageText,timeToLive: new TimeSpan(hours: 0, minutes: 0, seconds: timeToLiveInSeconds ?? _limitSettings.DefaultTimeToLiveInSeconds));

                if (response.GetRawResponse().Status == StatusCodes.Status201Created)
                {
                    return CreatedAtRoute(null, queuedMessageText);
                }

                return StatusCode(response.GetRawResponse().Status);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }

        }


        /// <summary>
        /// Deletes the specified value associated with the id provided.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="popReceipt">The pop receipt.</param>
        /// <returns>
        /// No content upon success, http StatusCodes.Status400BadRequest upon error
        /// </returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/v1/queuedmessages/{id}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string id, string popReceipt)
        {
            try
            {
                QueueClient queueClient = new QueueClient(_queueSettings.QueueConnectionString, QueueConstants.NOTES_QUEUE);

                if (queueClient.Exists())
                {
                    // Delete the message from the queue
                    Response response = await queueClient.DeleteMessageAsync(id, popReceipt);
                }
                return NoContent();
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }
        }        
    }
}
