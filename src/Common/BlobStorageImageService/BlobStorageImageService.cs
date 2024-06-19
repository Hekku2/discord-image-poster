using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.BlobStorageImageService;

public class BlobStorageImageService : IBlobStorageImageService
{
    private readonly ILogger<BlobStorageImageService> _logger;
    private readonly BlobStorageImageSourceOptions _blobConfig;
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStorageImageService
    (
        ILogger<BlobStorageImageService> logger,
        IOptions<BlobStorageImageSourceOptions> blobConfig,
        [FromKeyedServices(KeyedServiceConstants.ImageBlobContainerClient)] BlobContainerClient blobContainerClient
    )
    {
        _logger = logger;
        _blobConfig = blobConfig.Value;
        _blobContainerClient = blobContainerClient;
    }

    public async Task<string[]> GetAllImagesAsync()
    {
        var allImages = await GetAllImages(_blobContainerClient, _blobConfig.FolderPath);
        return allImages.Select(blob => blob.Blob.Name).ToArray();
    }

    public async Task<BlobDownloadStreamingResult?> GetImageStream(string name)
    {
        _logger.LogInformation("Getting image from blob: {Blob}", name);
        var blobClient = _blobContainerClient.GetBlobClient(name);
        if (await blobClient.ExistsAsync())
        {
            return await blobClient.DownloadStreamingAsync();
        }
        return null;
    }

    private async Task<List<BlobHierarchyItem>> GetAllImages(
        BlobContainerClient container,
        string prefix)
    {
        _logger.LogDebug("Traversing directory: {Prefix}", prefix);

        var images = new List<BlobHierarchyItem>();
        var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
            .AsPages(default, 20);

        await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            {
                if (blobhierarchyItem.IsPrefix)
                {
                    _logger.LogTrace("Moving to directory: {Prefix}", blobhierarchyItem.Prefix);
                    images.AddRange(await GetAllImages(container, blobhierarchyItem.Prefix));
                }
                else
                {
                    _logger.LogTrace("Adding image {Name}", blobhierarchyItem.Blob.Name);
                    images.Add(blobhierarchyItem);
                }
            }
        }
        return images;
    }
}
