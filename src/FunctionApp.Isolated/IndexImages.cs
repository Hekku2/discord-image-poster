using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.IndexService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class IndexImages
{
    private readonly ILogger _logger;
    private readonly IIndexService _indexService;
    private readonly IBlobStorageImageService _imageService;

    public IndexImages(ILogger<SendImage> logger, IIndexService indexService, IBlobStorageImageService imageService)
    {
        _logger = logger;
        _indexService = indexService;
        _imageService = imageService;
    }

    [Function("GetImageIndex")]
    public async Task<ImageIndex?> GetImageIndex([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Getting index.");

        return await _indexService.GetImageIndexAsync();
    }

    [Function("UpdateImageIndex")]
    public async Task UpdateImageIndex([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Updating image index");
        var index = await GetExistingIndexOrNewAsync();

        var allImages = await _imageService.GetAllImagesAsync();
        RemoveDeletedFromIndex(index, allImages);
        AddNewImagesToIndex(index, allImages);
        index.RefreshedAt = DateTimeOffset.UtcNow;
        await _indexService.UpdateIndexAsync(index);
    }

    private async Task<ImageIndex> GetExistingIndexOrNewAsync()
    {
        var index = await _indexService.GetImageIndexAsync();
        if (index is null)
        {
            _logger.LogWarning("No index found, creating new index.");
            return new ImageIndex
            {
                Images = new List<ImageIndexMetadata>(),
                RefreshedAt = DateTimeOffset.UtcNow,
            };
        }

        return index;
    }

    private static void AddNewImagesToIndex(ImageIndex index, string[] allImages)
    {
        foreach (string image in allImages)
        {
            if (index.Images.Any(x => x.Name == image))
            {
                continue;
            }

            index.Images.Add(new ImageIndexMetadata
            {
                Name = image,
                Description = null,
                AddedAt = DateTimeOffset.UtcNow,
                TimesPosted = 0,
                Ignore = false,
                LastPostedAt = null,
            });
        }
    }

    private void RemoveDeletedFromIndex(ImageIndex index, string[] allImages)
    {
        foreach (ImageIndexMetadata image in index.Images)
        {
            if (allImages.Contains(image.Name))
            {
                continue;
            }

            _logger.LogWarning("Image {Name} not found in storage, removing from index.", image.Name);
            index.Images.Remove(image);
        }
    }
}
