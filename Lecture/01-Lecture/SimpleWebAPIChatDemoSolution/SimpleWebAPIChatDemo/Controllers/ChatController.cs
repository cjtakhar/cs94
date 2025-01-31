using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using SimpleWebAPIChatDemo.Settings;

namespace SimpleWebAPIChatDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatClient _chatClient;
        private readonly AISettings _aISettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class.
        /// </summary>
        /// <param name="chatClient">The chat client to interact with the AI model.</param>
        /// <param name="aISettings">The settings for the AI model.</param>
        /// <param name="logger">The logger instance for logging.</param>
        public ChatController(IChatClient chatClient,
                              AISettings aISettings,
                              ILogger<ChatController> logger)
        {
            _logger = logger;
            _chatClient = chatClient;
            _aISettings = aISettings;
        }

        /// <summary>
        /// Post a chat message to the AI model and get a response.
        /// </summary>
        /// <param name="prompt">The input prompt to send to the AI model.</param>
        /// <returns>The response from the AI model as a string.</returns>
        [HttpPost(Name = "PostChat")]
        public async Task<string> Post([FromBody] string prompt)
        {
            // Create chat options using settings from AISettings
            ChatOptions chatOptions = new ChatOptions()
            {
                Temperature = _aISettings.Temperature, // Controls the randomness of the response
                TopP = _aISettings.TopP, // Controls the diversity of the response
                MaxOutputTokens = _aISettings.MaxOutputTokens // Maximum number of tokens in the response
            };

            // Send the prompt to the AI model and get the response
            ChatCompletion responseCompletion = await _chatClient.CompleteAsync(chatMessage: prompt, options: chatOptions);

            // Return the response text or a default message if the response is null
            return responseCompletion.Message?.Text ?? "<Unable to respond>";
        }
    }
}
