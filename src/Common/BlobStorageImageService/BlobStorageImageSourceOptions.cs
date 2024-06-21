using System.ComponentModel.DataAnnotations;

namespace DiscordImagePoster.Common.BlobStorageImageService;

/// <summary>
/// Settings for the Azure Blob Storage which hosts the images.
/// This is used to fetch images from the storage account.
/// </summary>
public class BlobStorageImageSourceOptions
{
    /// <summary>
    /// The connection string to the Azure Storage account.
    /// Use this only for development. Use managed identity in production.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The name of the container where the images are stored.
    /// </summary>
    [Required]
    public required string ContainerName { get; set; }

    /// <summary>
    /// The path to the folder where the images are stored.
    /// </summary>
    public required string FolderPath { get; set; }
}
