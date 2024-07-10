using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.BlobStorageImageService;

public static class BlobStorageImageServiceExtensions
{
    /// <summary>
    /// Register the Blob Storage Image service.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddBlobStorageImageService(this IServiceCollection services)
    {
        services.AddOptions<BlobStorageImageSourceOptions>().BindConfiguration(nameof(BlobStorageImageSourceOptions)).ValidateDataAnnotations().ValidateOnStart();
        services.AddTransient<IBlobStorageImageService, BlobStorageImageService>();
        services.AddKeyedTransient(KeyedServiceConstants.ImageBlobContainerClient, (services, _) =>
        {
            var options = services.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BlobContainerUri))
            {
                return new BlobContainerClient(new Uri(options.BlobContainerUri), new DefaultAzureCredential());
            }

            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });
        return services;
    }
}
