namespace DiscordImagePoster.Common.RandomizationService;

/// <summary>
/// Asbtraction for sending a random image.
/// This is a separate service so this can be used from multiple functions or other places.
/// </summary>
public interface IRandomImagePoster
{
    Task PostRandomImageAsync();
}
