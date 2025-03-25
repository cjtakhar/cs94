using Azure.Storage.Queues.Models;
using Azure;
using Microsoft.AspNetCore.Mvc;
using ManagedIdentityQueueWebApi.Settings;
using System.Text;
using System.Text.Json;

namespace ManagedIdentityQueueWebApi.Controllers;

/// <summary>
/// Demo values controller
/// Implements the <see cref="ControllerBase" />
/// </summary>
/// <seealso cref="ControllerBase" />
[Route("api/[controller]")]
[Produces("application/json")]
[ApiController]
public class JsonQueuedMessagesController : ControllerBase
{
    private readonly JsonQueueClient _jsonQueueClient;
    private readonly LimitSettings _limitSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringQueuedMessagesController"/> class.
    /// </summary>
    public JsonQueuedMessagesController(JsonQueueClient jsonQueueClient,
                                        LimitSettings limitSettings)
    {
        _jsonQueueClient = jsonQueueClient;
        _limitSettings = limitSettings;
    }

    /// <summary>
    /// Creates a queued json message
    /// </summary>
    /// <param name="timeToLiveInSeconds">The amount of for the message to live</param>
    /// <param name="message">Json message</param>
    /// <returns>
    /// The object created
    /// </returns>
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> PostJson([FromQuery] int? timeToLiveInSeconds, [FromBody] string message)
    {
        try
        {
            JsonMessage queuedJsonMessage = new JsonMessage
            {
                Message = message,
                MessageAddedDateTime = DateTime.UtcNow
            };

            // Create the queue if it doesn't already exist
            _jsonQueueClient.CreateIfNotExists();

            // Convert the plain text into bytes then Base64 encode it
            string stringifiedJson = JsonSerializer.Serialize(queuedJsonMessage);

            var messageBytes = Encoding.UTF8.GetBytes(stringifiedJson);
            var base64EncodedMessage = Convert.ToBase64String(messageBytes);


            // Enqueue a message
            Response<SendReceipt> response = await _jsonQueueClient.SendMessageAsync(base64EncodedMessage, timeToLive: new TimeSpan(hours: 0, minutes: 0, seconds: timeToLiveInSeconds ?? _limitSettings.DefaultTimeToLiveInSeconds));

            if (response.GetRawResponse().Status == StatusCodes.Status201Created)
            {
                return CreatedAtRoute(null, base64EncodedMessage);
            }

            return StatusCode(response.GetRawResponse().Status);
        }
        catch (RequestFailedException ex)
        {
            return StatusCode(ex.Status);
        }

    }
}
