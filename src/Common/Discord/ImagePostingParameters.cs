namespace DiscordImagePoster.Common.Discord;

public class ImagePostingParameters
{
    public required Stream ImageStream { get; set; }
    public required string FileName { get; set; }
    public required string? Description { get; set; }
}
