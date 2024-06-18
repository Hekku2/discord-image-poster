namespace DiscordImagePoster.Common.Discord;

public interface IDiscordImagePoster
{
    Task SendImage(Stream stream, string fileName);
}
