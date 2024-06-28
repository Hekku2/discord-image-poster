using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.Discord;

/// <summary>
/// A no-op implementation of <see cref="IDiscordImagePoster"/>.
/// </summary>
public class NoOpDiscordImagePoster : IDiscordImagePoster
{
    private readonly ILogger<NoOpDiscordImagePoster> _logger;

    public NoOpDiscordImagePoster(
        ILogger<NoOpDiscordImagePoster> logger
    )
    {
        _logger = logger;
    }

    public Task SendImage(Stream stream, string fileName, string? description)
    {
        _logger.LogWarning("Discord sending is disabled. Filename was {fileName} with description {description}.", fileName, description);
        return Task.CompletedTask;
    }
}
