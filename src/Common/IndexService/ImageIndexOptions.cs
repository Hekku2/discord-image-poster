using System.ComponentModel.DataAnnotations;

namespace DiscordImagePoster.Common.IndexService;

/// <summary>
/// Image index options decide where the index of the images is stored.
/// This is used to keep track of the images that have been posted to Discord
/// and what images are available to post.
/// </summary>
public class ImageIndexOptions
{
    /// <summary>
    /// The connection string to the Azure Storage account.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Container name where the index is stored.
    /// </summary>
    [Required]
    public required string ContainerName { get; set; }
}
