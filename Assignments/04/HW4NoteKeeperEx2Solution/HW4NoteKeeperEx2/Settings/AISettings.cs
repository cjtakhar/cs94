namespace NoteKeeper.Settings
{
    /// <summary>
    /// Contains configuration settings for AI-related operations
    /// </summary>
    public class AISettings
    {
        public string? Endpoint { get; set; } // The endpoint for the AI model

        public string? ApiKey { get; set; } // The API key for the AI model

        public string? Model { get; set; } // The name of the AI model

        public string DeploymentModelName => Model ?? "gpt-4o-mini"; // The name of the AI model to use
    }
}
