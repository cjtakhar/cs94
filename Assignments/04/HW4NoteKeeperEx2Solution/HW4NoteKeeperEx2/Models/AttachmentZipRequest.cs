namespace NoteKeeper.Models
{
    /// <summary>
    /// DTO for zip request messages.
    /// </summary>
    public class AttachmentZipRequest
    {
        public string? noteId { get; set; }
        public string? zipFileId { get; set; }
    }
}
