namespace DiscordImagePoster.Common.IndexService;

/// <summary>
/// Parameters for updating image metadata.
/// </summary>
public class ImageMetadataUpdate
{
    public required string Name { get; set; }
    public required string? Caption { get; set; }
    public required string[]? Tags { get; set; }
}
