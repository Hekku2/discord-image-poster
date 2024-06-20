using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.IndexService;

public class BlobStorageIndexStorageService : IIndexStorageService
{
    private ILogger<BlobStorageIndexStorageService> _logger;
    private readonly BlobContainerClient _blobContainerClient;

    private const string IndexBlobName = "index.json";

    public BlobStorageIndexStorageService
    (
        ILogger<BlobStorageIndexStorageService> logger,
        [FromKeyedServices(KeyedServiceConstants.ImageIndexBlobContainerClient)] BlobContainerClient blobContainerClient
    )
    {
        _logger = logger;
        _blobContainerClient = blobContainerClient;
    }

    public async Task<ImageIndex?> GetImageIndexAsync()
    {
        var blobClient = _blobContainerClient.GetBlobClient(IndexBlobName);
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
        await _blobContainerClient.CreateIfNotExistsAsync();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(index);
        var blobClient = _blobContainerClient.GetBlobClient(IndexBlobName);
        await blobClient.UploadAsync(new MemoryStream(bytes), true);
    }
}
