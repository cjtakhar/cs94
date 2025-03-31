using Microsoft.EntityFrameworkCore;
using NoteKeeper.Data;
using NoteKeeper.Models;
using NoteKeeper.Services;

public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial notes and their corresponding attachments.
    /// </summary>
    /// <param name="context">Database context for interacting with the database.</param>
    /// <param name="generateTagsAsync">Function to generate tags based on note details using AI.</param>
    /// <param name="blobStorageService">Service for interacting with Azure Blob Storage.</param>
    /// <param name="sampleAttachmentsPath">Path to the directory containing sample attachments.</param>
public static async Task Seed(AppDbContext context, Func<string, Task<List<string>>> generateTagsAsync, BlobStorageService blobStorageService, string sampleAttachmentsPath)
{
    // Retrieve existing note summaries and details to avoid duplicate seed data.
    var existingNotes = await context.Notes
        .Select(n => new { n.Summary, n.Details })
        .ToListAsync();

    // Define a list of seed notes to be inserted if they do not already exist in the database.
    var seedNotes = new List<Note>
    {
        new Note { Summary = "Running grocery list", Details = "Milk, Eggs, Oranges" },
        new Note { Summary = "Gift supplies notes", Details = "Tape & Wrapping Paper" },
        new Note { Summary = "Valentine's Day gift ideas", Details = "Chocolate, Diamonds, New Car" },
        new Note { Summary = "Azure tips", Details = "portal.azure.com is a quick way to get to the portal" }
    };

    // Filter out notes that already exist in the database by matching both summary and details.
    var notesToAdd = seedNotes
        .Where(note => !existingNotes.Any(n => n.Summary == note.Summary && n.Details == note.Details))
        .ToList();

    // If there are no new notes to add, exit the method.
    if (!notesToAdd.Any())
    {
        return;
    }

    foreach (var note in notesToAdd)
    {
        // Assign a unique identifier and set the creation date for the note.
        note.NoteId = Guid.NewGuid();
        note.CreatedDateUtc = DateTime.UtcNow;

        // Generate tags for the note asynchronously based on its details.
        note.Tags = (await generateTagsAsync(note.Details)).Select(tag => new Tag
        {
            Id = Guid.NewGuid(),
            Name = tag,
            NoteId = note.NoteId
        }).ToList();

        // Add the note (with generated tags) to the database context.
        context.Notes.Add(note);

        // Ensure a corresponding blob storage container exists for storing attachments.
        var containerName = note.NoteId.ToString();
        if (!await blobStorageService.ContainerExistsAsync(containerName))
        {
            await blobStorageService.EnsureContainerExistsAsync(containerName);
        }

        // Retrieve the list of sample attachments for the note.
        var attachments = GetAttachmentsForNote(note.Summary);
        foreach (var attachment in attachments)
        {
            var filePath = Path.Combine(sampleAttachmentsPath, attachment);
            if (!File.Exists(filePath)) continue; // Skip if the file does not exist.

            // Check if the attachment already exists in blob storage before uploading.
            if (!await blobStorageService.AttachmentExistsAsync(containerName, attachment))
            {
                using var fileStream = File.OpenRead(filePath);
                await blobStorageService.UploadAttachmentAsync(
                    containerName,
                    attachment,
                    fileStream,
                    GetContentType(attachment)
                );
            }
        }
    }

    // Save all new notes and their generated tags to the database.
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Seeding complete without duplicates!");
}

    /// <summary>
    /// Dictionary mapping note summaries to their respective attachment file names.
    /// </summary>
    private static Dictionary<string, List<string>> noteAttachments = new()
    {
        { "Running grocery list", new List<string> { "MilkAndEggs.png", "Oranges.png" } },
        { "Gift supplies notes", new List<string> { "WrappingPaper.png", "Tape.png" } },
        { "Valentine's Day gift ideas", new List<string> { "Chocolate.png", "Diamonds.png", "NewCar.png" } },
        { "Azure tips", new List<string> { "AzureLogo.png", "AzureTipsAndTricks.pdf" } }
    };

    /// <summary>
    /// Retrieves the list of attachment file names associated with a given note summary.
    /// </summary>
    /// <param name="summary">The summary of the note.</param>
    /// <returns>A list of attachment file names.</returns>
    private static List<string> GetAttachmentsForNote(string summary)
        => noteAttachments.TryGetValue(summary, out var attachments) ? attachments : new List<string>();

    /// <summary>
    /// Determines the appropriate content type for a given file based on its extension.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <returns>The MIME type corresponding to the file extension.</returns>
    private static string GetContentType(string filename)
    {
        return Path.GetExtension(filename).ToLower() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream" // Default MIME type for unknown file types.
        };
    }
}
