namespace DiscordImagePoster.Common.IndexService;

/// <summary>
/// Metadata for single image in the index.
/// </summary>
public class ImageIndexMetadata
{
    /// <summary>
    /// The name of the image.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the image.
    /// </summary>
    public required string? Description { get; set; }

    /// <summary>
    /// The date when the image was added to the index.
    /// </summary>
    public required DateTimeOffset AddedAt { get; set; }

    /// <summary>
    /// The date when the image was last posted to Discord.
    /// </summary>
    public required DateTimeOffset? LastPostedAt { get; set; }

    /// <summary>
    /// The number of times the image has been posted to Discord.
    /// </summary>
    public required int TimesPosted { get; set; }

    /// <summary>
    /// Whether the image should be ignored when selecting random image.
    /// </summary>
    public required bool Ignore { get; set; }
}
