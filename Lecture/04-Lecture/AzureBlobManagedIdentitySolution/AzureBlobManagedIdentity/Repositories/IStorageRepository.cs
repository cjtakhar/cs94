namespace AzureBlobManagedIdentity.Repositories
{
    public interface IStorageRepository
    {
        Task DeleteFile(string fileName);
        Task<(MemoryStream fileStream, string contentType)> GetFileAsync(string fileName);
        Task<byte[]> GetFileInByteArrayAsync(string fileName);
        Task<List<string>> GetListOfBlobs();
        Task UploadFile(string fileName, Stream fileStream, string contentType);
    }
}
