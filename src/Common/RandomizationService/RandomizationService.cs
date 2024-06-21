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

        var allowedImages = imageIndex.Images.Where(image => !image.Ignore);
        if (!allowedImages.Any())
        {
            _logger.LogTrace("No allowed images found in index.");
            return null;
        }
        var minimunPosts = allowedImages.Min(image => image.TimesPosted);
        _logger.LogTrace("Minimum posts for allowed images: {MinimunPosts}", minimunPosts);
        return allowedImages
            .Where(image => image.TimesPosted == minimunPosts)
            .OrderBy(x => Guid.NewGuid())
            .FirstOrDefault();
    }
}
