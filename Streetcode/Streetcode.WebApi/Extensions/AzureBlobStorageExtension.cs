using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Services.BlobStorageService;

namespace Streetcode.WebApi.Extensions;

public static class AzureBlobStorageExtension
{
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobSettings>(configuration.GetSection("AzureBlobStorage"));

        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<AzureBlobSettings>>().Value;
            return new BlobServiceClient(settings.ConnectionString);
        });

        services.AddSingleton<BlobContainerClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<AzureBlobSettings>>().Value;
            var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
            return blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        });

        return services;
    }
}