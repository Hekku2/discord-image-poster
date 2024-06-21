using DiscordImagePoster.Common.IndexService;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.RandomizationService;

public class RandomizationService : IRandomizationService
{
    private readonly ILogger<RandomizationService> _logger;

    public RandomizationService(ILogger<RandomizationService> logger)
    {
        _logger = logger;
    }

    public ImageIndexMetadata? GetRandomImage(ImageIndex imageIndex)
    {
        _logger.LogTrace("Getting random image from image index.");

        var allowedImages = imageIndex.Images.Where(x => !x.Ignore);
        var minimunPosts = allowedImages.Min(x => x.TimesPosted);
        var imagesInPostingRange = allowedImages.Where(x => x.TimesPosted == minimunPosts).ToList();

        return imagesInPostingRange.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
    }
}
