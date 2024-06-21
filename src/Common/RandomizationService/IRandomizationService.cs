using DiscordImagePoster.Common.IndexService;

namespace DiscordImagePoster.Common.RandomizationService;

public interface IRandomizationService
{
    ImageIndexMetadata? GetRandomImage(ImageIndex imageIndex);
}
