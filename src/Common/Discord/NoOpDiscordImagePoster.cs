namespace DiscordImagePoster.Common.Discord;

/// <summary>
/// A no-op implementation of <see cref="IDiscordImagePoster"/>.
/// </summary>
public class NoOpDiscordImagePoster : IDiscordImagePoster
{
    public Task SendImage(Stream stream, string fileName)
    {
        return Task.CompletedTask;
    }
}
