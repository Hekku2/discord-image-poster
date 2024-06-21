using DiscordImagePoster.Common.IndexService;

namespace DiscordImagePoster.Common.RandomizationService;

/// <summary>
/// Randomization service proivdes randomization logic for ImageIndex
/// </summary>
public interface IRandomizationService
{
    /// <summary>
    /// Returns random image from given image index.
    /// </summary>
    /// <param name="imageIndex">Index</param>
    /// <returns>Random image from index or null if there are no valid images available.</returns>
    ImageIndexMetadata? GetRandomImage(ImageIndex imageIndex);
}
