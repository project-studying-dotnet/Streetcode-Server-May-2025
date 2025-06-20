using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.BLL.Services.BlobStorageService;

public class AzureBlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobEnvironmentVariables _environmentVariables;
    private readonly string _keyCrypt;

    public AzureBlobService(BlobServiceClient blobServiceClient,
        IOptions<BlobEnvironmentVariables> environmentVariables,
        IOptions<AzureBlobSettings> options)
    {
        var settings = options.Value;
        _blobServiceClient = blobServiceClient;
        _containerClient = _blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        _environmentVariables = environmentVariables.Value;
        _keyCrypt = _environmentVariables.BlobStoreKey;
    }

    public async Task<string> SaveFileInStorageAsync(string base64, string name, string mimeType)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        string createdFileName = $"{DateTime.Now}{name}"
            .Replace(" ", "_")
            .Replace(".", "_")
            .Replace(":", "_");
        string hashedBlobName = HashFunction(createdFileName);
        
        await EncryptFileAsync(bytes, hashedBlobName, mimeType);

        return hashedBlobName;
    }

    public async Task<MemoryStream> FindFileInStorageAsMemoryStreamAsync(string name)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(name);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Blob '{name}' not found in Azure Blob Storage.");
        }

        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream);

        byte[] encryptedBytes = memoryStream.ToArray();
        byte[] decryptedBytes = await DecryptFileAsync(encryptedBytes);

        return new MemoryStream(decryptedBytes);
    }
    
    public async Task<string> FindFileInStorageAsBase64Async(string name)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(name);
        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"File '{name}' not found");
        }

        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream);

        byte[] encryptedBytes = memoryStream.ToArray();
        byte[] decryptedBytes = await DecryptFileAsync(encryptedBytes);

        return Convert.ToBase64String(decryptedBytes);
    }
    
    public async Task DeleteFileInStorageAsync(string name)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(name);
        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"File '{name}' not found");
        }
        
        await blobClient.DeleteAsync();
    }

    public async Task<string> UpdateFileInStorageAsync(string previousBlobName, string base64Format, string newBlobName,
        string extension)
    {
        await DeleteFileInStorageAsync(previousBlobName);
        return await SaveFileInStorageAsync(base64Format, newBlobName, extension);
    }

    private static string HashFunction(string createdFileName)
    {
        using (var hash = SHA256.Create())
        {
            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(createdFileName));
            return Convert.ToBase64String(result).Replace('/', '_');
        }
    }

    private async Task<byte[]> DecryptFileAsync(byte[] encryptedData)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_keyCrypt);
        byte[] iv = new byte[16];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);

        byte[] decryptedBytes;
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            decryptedBytes = decryptor.TransformFinalBlock(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        }

        return decryptedBytes;
    }
    
    private async Task EncryptFileAsync(byte[] imageBytes, string name, string mimeType)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_keyCrypt);

        byte[] iv = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(iv);
        }

        byte[] encryptedBytes;
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            encryptedBytes = encryptor.TransformFinalBlock(imageBytes, 0, imageBytes.Length);
        }

        byte[] encryptedData = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, encryptedData, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, encryptedData, iv.Length, encryptedBytes.Length);
        
        var fileName = $"{name}.{mimeType}";
        
        BlobClient blobClient = _containerClient.GetBlobClient(fileName);
        using var stream = new MemoryStream(encryptedBytes);
        stream.Position = 0;
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}