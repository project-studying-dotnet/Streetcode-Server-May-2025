namespace Streetcode.BLL.Interfaces.BlobStorage;

public interface IBlobService
{
    public Task<string> SaveFileInStorageAsync(string base64, string name, string mimeType);
    public Task<MemoryStream> FindFileInStorageAsMemoryStreamAsync(string name);
    public Task<string> UpdateFileInStorageAsync(
        string previousBlobName,
        string base64Format,
        string newBlobName,
        string extension);
    public Task<string> FindFileInStorageAsBase64Async(string name);
    public Task DeleteFileInStorageAsync(string name);
}
