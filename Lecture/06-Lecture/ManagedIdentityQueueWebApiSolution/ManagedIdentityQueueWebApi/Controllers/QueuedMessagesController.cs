using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using Azure;
using Microsoft.AspNetCore.Mvc;
using ManagedIdentityQueueWebApi.Settings;
using System.Text;

namespace ManagedIdentityQueueWebApi.Controllers;

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
    private readonly QueueSettings _queueSettings;
    private readonly QueueClient _queueClient;
    private readonly LimitSettings _limitSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedMessagesController"/> class.
    /// </summary>
    public QueuedMessagesController(QueueSettings queueSettings,
                                    QueueClient queueClient,
                                    LimitSettings limitSettings)
    {
        _queueSettings = queueSettings;
        _queueClient = queueClient;
        _limitSettings = limitSettings;
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
            // Create the queue if it doesn't already exist
            _queueClient.CreateIfNotExists();

            // Convert the plain text into bytes then Base64 encode it
            var messageBytes = Encoding.UTF8.GetBytes(queuedMessageText);
            var base64EncodedMessage = Convert.ToBase64String(messageBytes);

            // Enqueue a message
            Response<SendReceipt> response = await _queueClient.SendMessageAsync(base64EncodedMessage, timeToLive: new TimeSpan(hours: 0, minutes: 0, seconds: timeToLiveInSeconds ?? _limitSettings.DefaultTimeToLiveInSeconds));

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
}
