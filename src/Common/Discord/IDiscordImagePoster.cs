namespace DiscordImagePoster.Common.Discord;

public interface IDiscordImagePoster
{
    Task SendImageAsync(ImagePostingParameters parameters);
}
