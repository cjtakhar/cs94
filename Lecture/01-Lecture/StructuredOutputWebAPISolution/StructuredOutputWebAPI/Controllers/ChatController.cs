using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using StructuredOutputWebAPI.Settings;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using System.Net.Mime;

namespace StructuredOutputWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {

        public class KeyPhrasesResponse
        {
            public List<string> Phrases { get; set; } = [];
        }

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<KeyPhrasesResponse>> Post([FromBody] string prompt)
        {
            JsonSchema schema = JsonSchema.FromType<KeyPhrasesResponse>();
            string jsonSchemaString = schema.ToJson();

            JsonElement jsonSchemaElement = JsonDocument.Parse(jsonSchemaString).RootElement;

            ChatResponseFormatJson chatResponseFormatJson = ChatResponseFormat.ForJsonSchema(jsonSchemaElement, "ChatResponse", "Chat response schema");

            // Create chat options using settings from AISettings
            ChatOptions chatOptions = new ChatOptions()
            {
                Temperature = _aISettings.Temperature, // Controls the randomness of the response
                TopP = _aISettings.TopP, // Controls the diversity of the response
                MaxOutputTokens = _aISettings.MaxOutputTokens, // Maximum number of tokens in the response
                ResponseFormat = chatResponseFormatJson // Format the response as JSON
            };

            ChatCompletion responseCompletion;

            // Demo 01 Using a prompt to provide context to the AI model
            responseCompletion = await Demo01PromptEnhancement(prompt, chatOptions);

            // Demo 02 Using a system message to provide context to the AI model
            //responseCompletion = await Demo02SystemPromptToProvideContext(prompt, chatOptions);

            KeyPhrasesResponse response;

            try
            {
                response = JsonSerializer.Deserialize<KeyPhrasesResponse>(responseCompletion.Message.Text!)!;
                // Return the response text or a default message if the response is null
                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from AI model");
                return this.StatusCode(500, "Failed to deserialize response from AI model");
            }
        }

        /// <summary>
        /// Enhances the prompt to provide context to the AI model.
        /// </summary>
        /// <param name="prompt">The input prompt to send to the AI model.</param>
        /// <param name="chatOptions">The chat options to configure the AI model.</param>
        /// <returns>The response from the AI model.</returns>
        private async Task<ChatCompletion> Demo01PromptEnhancement(string prompt, ChatOptions chatOptions)
        {
            string enhancedPrompt = $"Identify and return a JSON list of the most important 3 key phrases from the following text: {prompt}";
            ChatCompletion responseCompletion = await _chatClient.CompleteAsync(enhancedPrompt, options: chatOptions);
            return responseCompletion;
        }

        /// <summary>
        /// Uses a system message to provide context to the AI model.
        /// </summary>
        /// <param name="prompt">The input prompt to send to the AI model.</param>
        /// <param name="chatOptions">The chat options to configure the AI model.</param>
        /// <returns>The response from the AI model.</returns>
        private async Task<ChatCompletion> Demo02SystemPromptToProvideContext(string prompt, ChatOptions chatOptions)
        {
            List<ChatMessage> messages = new List<ChatMessage>();
            messages.Add(new ChatMessage(ChatRole.System, "You will identify and return a JSON list of the most important 3 key phrases the users input"));
            messages.Add(new ChatMessage(ChatRole.User, prompt));
            // Send the prompt to the AI model and get the response
            ChatCompletion responseCompletion = await _chatClient.CompleteAsync(messages, options: chatOptions);
            return responseCompletion;
        }
    }
}
