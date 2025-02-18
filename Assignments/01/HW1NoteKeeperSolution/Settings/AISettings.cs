namespace NoteKeeper.Settings
{
    public class AISettings
    {
        public string? Endpoint { get; set; }
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public string DeploymentModelName => Model ?? "gpt-4o-mini";
    }
}