using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.IndexService;

public class BlobStorageIndexStorageService : IIndexStorageService
{
    private readonly ILogger<BlobStorageIndexStorageService> _logger;
    private readonly ImageIndexOptions _options;
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStorageIndexStorageService
    (
        ILogger<BlobStorageIndexStorageService> logger,
        IOptions<ImageIndexOptions> options,
        [FromKeyedServices(KeyedServiceConstants.ImageIndexBlobContainerClient)] BlobContainerClient blobContainerClient
    )
    {
        _logger = logger;
        _options = options.Value;
        _blobContainerClient = blobContainerClient;
    }

    public async Task<ImageIndex?> GetImageIndexAsync()
    {
        _logger.LogTrace("Getting image index from {IndexFileName}", _options.IndexFileName);
        var blobClient = _blobContainerClient.GetBlobClient(_options.IndexFileName);
        var exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            _logger.LogWarning("Index blob not found.");
            return null;
        }

        using var stream = await blobClient.OpenReadAsync();
        return await JsonSerializer.DeserializeAsync<ImageIndex>(stream);
    }

    public async Task UpdateIndexAsync(ImageIndex index)
    {
        _logger.LogTrace("Updating image index to {IndexFileName}", _options.IndexFileName);
        await _blobContainerClient.CreateIfNotExistsAsync();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(index);
        var blobClient = _blobContainerClient.GetBlobClient(_options.IndexFileName);
        await blobClient.UploadAsync(new MemoryStream(bytes), true);
    }
}
