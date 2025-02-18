using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NoteKeeper.Settings;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace NoteKeeper.Controllers
{
    /// <summary>
    /// API Controller for managing notes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        /// <summary>
        /// In-memory storage for notes.
        /// </summary>
        private static List<Note> notes = new List<Note>();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AISettings _aiSettings;
        private readonly ILogger<NotesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesController"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        /// <param name="aiSettings">Configuration settings for OpenAI.</param>
        /// <param name="logger">Logger instance for logging information.</param>
        public NotesController(IHttpClientFactory httpClientFactory, IOptions<AISettings> aiSettings, ILogger<NotesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            if (aiSettings == null)
            {
                throw new ArgumentNullException(nameof(aiSettings), "AISettings is null. Ensure it is registered in Program.cs.");
            }

            _aiSettings = aiSettings.Value;

            _logger.LogInformation("OpenAI Endpoint: {Endpoint}", _aiSettings.Endpoint);
            _logger.LogInformation("OpenAI API Key Loaded: {ApiKeyStatus}", string.IsNullOrWhiteSpace(_aiSettings.ApiKey) ? "NOT FOUND" : "LOADED");
        }

        /// <summary>
        /// Retrieves all notes.
        /// </summary>
        /// <returns>A list of notes.</returns>
        [HttpGet]
        public IActionResult GetNotes()
        {
            return Ok(notes);
        }

        /// <summary>
        /// Retrieves a note by its ID.
        /// </summary>
        /// <param name="noteId">The unique identifier of the note.</param>
        /// <returns>The note if found; otherwise, a NotFound response.</returns>
        [HttpGet("{noteId}")]
        public IActionResult GetNoteById(Guid noteId)
        {
            var note = notes.FirstOrDefault(n => n.NoteId == noteId);
            return note == null ? NotFound() : Ok(note);
        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="newNote">The note to create.</param>
        /// <returns>The created note.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note newNote)
        {
            if (newNote == null || string.IsNullOrWhiteSpace(newNote.Summary) || string.IsNullOrWhiteSpace(newNote.Details))
            {
                return BadRequest("Summary and details are required.");
            }

            newNote.NoteId = Guid.NewGuid();
            newNote.CreatedDateUtc = DateTime.UtcNow;

            var generatedTags = await GenerateTagsAsync(newNote.Details);
            newNote.Tags = generatedTags ?? new List<string>();

            notes.Add(newNote);
            return CreatedAtAction(nameof(GetNoteById), new { noteId = newNote.NoteId }, newNote);
        }

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        /// <param name="noteId">The unique identifier of the note.</param>
        /// <param name="updatedNote">The updated note details.</param>
        /// <returns>No content if successful; otherwise, NotFound.</returns>
        [HttpPatch("{noteId}")]
        public async Task<IActionResult> UpdateNote(Guid noteId, [FromBody] Note updatedNote)
        {
            var note = notes.FirstOrDefault(n => n.NoteId == noteId);
            if (note == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(updatedNote.Summary))
            {
                note.Summary = updatedNote.Summary;
                note.ModifiedDateUtc = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(updatedNote.Details))
            {
                note.Details = updatedNote.Details;
                note.ModifiedDateUtc = DateTime.UtcNow;

                var updatedTags = await GenerateTagsAsync(updatedNote.Details);
                note.Tags = updatedTags ?? new List<string>();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        /// <param name="noteId">The unique identifier of the note.</param>
        /// <returns>No content if successful; otherwise, NotFound.</returns>
        [HttpDelete("{noteId}")]
        public IActionResult DeleteNote(Guid noteId)
        {
            var note = notes.FirstOrDefault(n => n.NoteId == noteId);
            if (note == null) return NotFound();

            notes.Remove(note);
            return NoContent();
        }

        /// <summary>
        /// Calls OpenAI API to generate AI-powered tags for a note's details.
        /// </summary>
        /// <param name="details">The details of the note.</param>
        /// <returns>A list of generated tags.</returns>
        private async Task<List<string>> GenerateTagsAsync(string details)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Generate 3-5 relevant one-word tags for the given note details. Always return a valid JSON array." },
                    new { role = "user", content = details }
                },
                temperature = 0.5,
                max_tokens = 50
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _aiSettings.Endpoint)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("api-key", _aiSettings.ApiKey);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API request failed with status code: {StatusCode}", response.StatusCode);
                return new List<string> { "ErrorFetchingTags" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("OpenAI response received successfully.");

            try
            {
                var jsonResponse = JsonDocument.Parse(responseContent);
                var messageContent = jsonResponse.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrEmpty(messageContent))
                {
                    messageContent = messageContent.Replace("```json", "").Replace("```", "").Trim();

                    if (messageContent.StartsWith("[") && messageContent.EndsWith("]"))
                    {
                        return JsonSerializer.Deserialize<List<string>>(messageContent) ?? new List<string> { "NoTagsGenerated" };
                    }
                }
                return new List<string> { "InvalidTagsFormat" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing OpenAI response.");
                return new List<string> { "ErrorParsingTags" };
            }
        }
    }

    public class Note
    {
        public Guid NoteId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedDateUtc { get; set; }
        public DateTime? ModifiedDateUtc { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}