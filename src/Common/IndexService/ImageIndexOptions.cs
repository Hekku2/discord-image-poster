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
    /// Use this only for development. Use managed identity in production.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The name of the container where the images are stored.
    /// This is not needed (nor used) if BlobContainerUri is used.
    /// </summary>
    public required string? ContainerName { get; set; }

    /// <summary>
    /// The URI to the container where the images are stored.
    /// https://{account_name}.blob.core.windows.net/{container_name}
    /// 
    /// If this is used, the ConnectionString is not needed and managed identity is used
    /// </summary>
    public required string? BlobContainerUri { get; set; }

    /// <summary>
    /// The path to the index file. Defaults to index.json in root.
    public required string IndexFileName { get; set; } = "index.json";
}
