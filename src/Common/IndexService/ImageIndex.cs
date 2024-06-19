namespace DiscordImagePoster.Common.IndexService;

/// <summary>
/// Data Transfer Object representing image index. This image index is used so
/// that we can keep track of which images have been posted to Discord and that
/// images don't need to be always enumerated from the Blob Storage.
/// </summary>
public class ImageIndex
{
    /// <summary>
    /// The name of the image.
    /// </summary>
    public required DateTimeOffset RefreshedAt { get; set; }

    /// <summary>
    /// All indexed images.
    public required List<ImageIndexMetadata> Images { get; set; }
}
