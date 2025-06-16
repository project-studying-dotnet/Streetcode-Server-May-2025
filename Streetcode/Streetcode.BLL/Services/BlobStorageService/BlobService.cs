using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.BlobStorageService;

public class BlobService : IBlobService
{
    private readonly BlobEnvironmentVariables _envirovment;
    private readonly string _keyCrypt;
    private readonly string _blobPath;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public BlobService(IOptions<BlobEnvironmentVariables> environment, IRepositoryWrapper? repositoryWrapper = null)
    {
        _envirovment = environment.Value;
        _keyCrypt = _envirovment.BlobStoreKey;
        _blobPath = _envirovment.BlobStorePath;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<MemoryStream> FindFileInStorageAsMemoryStreamAsync(string name)
    {
        string[] splitedName = name.Split('.');

        byte[] decryptedBytes = await DecryptFileAsync(splitedName[0], splitedName[1]);

        return new MemoryStream(decryptedBytes);
    }

    public async Task<string> FindFileInStorageAsBase64Async(string name)
    {
        string[] splitedName = name.Split('.');

        byte[] decryptedBytes = await DecryptFileAsync(splitedName[0], splitedName[1]);

        return Convert.ToBase64String(decryptedBytes);
    }

    public async Task<string> SaveFileInStorageAsync(string base64, string name, string mimeType)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        string createdFileName = $"{DateTime.Now}{name}"
            .Replace(" ", "_")
            .Replace(".", "_")
            .Replace(":", "_");

        string hashBlobStorageName = HashFunction(createdFileName);

        Directory.CreateDirectory(_blobPath);
        await EncryptFileAsync(imageBytes, mimeType, hashBlobStorageName);

        return hashBlobStorageName;
    }

    public async Task SaveFileInStorageBase64Async(string base64, string name, string extension)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        Directory.CreateDirectory(_blobPath);
        await EncryptFileAsync(imageBytes, extension, name);
    }

    public async Task DeleteFileInStorageAsync(string name)
    {
        string path = $"{_blobPath}{name}";
        await Task.Run(() => File.Delete(path));
    }

    public async Task<string> UpdateFileInStorageAsync(
        string previousBlobName,
        string base64Format,
        string newBlobName,
        string extension)
    {
        await DeleteFileInStorageAsync(previousBlobName);

        return await SaveFileInStorageAsync(
        base64Format,
        newBlobName,
        extension);
    }

    public async Task CleanBlobStorage()
    {
        var base64Files = GetAllBlobNames();

        var existingImagesInDatabase = await _repositoryWrapper.ImageRepository.GetAllAsync();
        var existingAudiosInDatabase = await _repositoryWrapper.AudioRepository.GetAllAsync();

        List<string> existingMedia = new ();
        existingMedia.AddRange(existingImagesInDatabase.Select(img => img.BlobName));
        existingMedia.AddRange(existingAudiosInDatabase.Select(img => img.BlobName));

        var filesToRemove = base64Files.Except(existingMedia).ToList();

        foreach (var file in filesToRemove)
        {
            Console.WriteLine($"Deleting {file}...");
            await DeleteFileInStorageAsync(file);
        }
    }

    private IEnumerable<string> GetAllBlobNames()
    {
        var paths = Directory.EnumerateFiles(_blobPath);

        return paths.Select(p => Path.GetFileName(p));
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

    private async Task EncryptFileAsync(byte[] imageBytes, string type, string name)
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

        byte[] encryptedData = new byte[encryptedBytes.Length + iv.Length];
        Buffer.BlockCopy(iv, 0, encryptedData, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, encryptedData, iv.Length, encryptedBytes.Length);
        await File.WriteAllBytesAsync($"{_blobPath}{name}.{type}", encryptedData);
    }

    private async Task<byte[]> DecryptFileAsync(string fileName, string type)
    {
        byte[] encryptedData = await File.ReadAllBytesAsync($"{_blobPath}{fileName}.{type}");
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
}