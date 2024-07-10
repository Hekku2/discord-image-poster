using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.IndexService;

public static class IndexServiceExtensions
{
    /// <summary>
    /// Register the Index service.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddIndexServices(this IServiceCollection services)
    {
        services.AddOptions<ImageIndexOptions>().BindConfiguration(nameof(ImageIndexOptions)).ValidateDataAnnotations().ValidateOnStart();
        services.AddTransient<IIndexStorageService, BlobStorageIndexStorageService>();
        services.AddKeyedTransient(KeyedServiceConstants.ImageIndexBlobContainerClient, (services, _) =>
        {
            var options = services.GetRequiredService<IOptions<ImageIndexOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BlobContainerUri))
            {
                return new BlobContainerClient(new Uri(options.BlobContainerUri), new DefaultAzureCredential());
            }
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });
        services.AddTransient<IIndexService, IndexService>();

        return services;
    }

}
