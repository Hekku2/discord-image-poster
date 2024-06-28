using DiscordImagePoster.Common.BlobStorageImageService;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.IndexService;

public class IndexService : IIndexService
{
    private readonly ILogger<IndexService> _logger;
    private readonly IIndexStorageService _indexService;
    private readonly IBlobStorageImageService _imageService;

    public IndexService(ILogger<IndexService> logger, IIndexStorageService indexService, IBlobStorageImageService imageService)
    {
        _logger = logger;
        _indexService = indexService;
        _imageService = imageService;
    }

    public async Task<ImageIndex?> GetIndexAsync()
    {
        return await _indexService.GetImageIndexAsync();
    }

    public async Task<ImageIndex> GetIndexOrCreateNew()
    {
        return await _indexService.GetImageIndexAsync() ?? await RefreshIndexAsync();
    }

    public async Task<ImageIndex> RefreshIndexAsync()
    {
        _logger.LogDebug("Updating image index");
        var index = await GetExistingIndexOrNewAsync();

        var allImages = await _imageService.GetAllImagesAsync();
        RemoveDeletedFromIndex(index, allImages);
        AddNewImagesToIndex(index, allImages);
        index.RefreshedAt = DateTimeOffset.UtcNow;
        await _indexService.UpdateIndexAsync(index);
        return index;
    }

    public async Task IncreasePostingCountAndUpdateMetadataAsync(ImageMetadataUpdate imageMetadataUpdate)
    {
        _logger.LogDebug("Increasing posting count for {ImagePath}", imageMetadataUpdate.Name);
        var index = await _indexService.GetImageIndexAsync();
        if (index is null)
        {
            _logger.LogWarning("No index found, generating new index.");
            index = await RefreshIndexAsync();
        }

        var image = index.Images.FirstOrDefault(x => x.Name == imageMetadataUpdate.Name);
        if (image is null)
        {
            _logger.LogWarning("Image {ImagePath} not found in index, adding.", imageMetadataUpdate.Name);
            image = CreateMetadata(imageMetadataUpdate.Name);
            index.Images.Add(image);
        }

        _logger.LogTrace("Image was posted {TimesPosted} times before addition, it it was lasted posted at {LastPostedAt}", image.TimesPosted, image.LastPostedAt);
        image.TimesPosted++;
        image.LastPostedAt = DateTimeOffset.UtcNow;
        if (imageMetadataUpdate.Caption is not null)
        {
            image.Caption = imageMetadataUpdate.Caption;
        }
        if (imageMetadataUpdate.Tags is not null)
        {
            image.Tags = imageMetadataUpdate.Tags;
        }
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
        foreach (var image in allImages)
        {
            if (index.Images.Any(x => x.Name == image))
            {
                continue;
            }

            index.Images.Add(CreateMetadata(image));
        }
    }

    private static ImageIndexMetadata CreateMetadata(string imagePath)
    {
        return new ImageIndexMetadata
        {
            Name = imagePath,
            AddedAt = DateTimeOffset.UtcNow,
            TimesPosted = 0,
            Ignore = false,
            LastPostedAt = null,
            Caption = null,
            Tags = null
        };
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
