using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;

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
    public required string? ContainerName { get; set; }

    /// <summary>
    /// The URI to the container where the images are stored.
    /// https://{account_name}.blob.core.windows.net/{container_name}
    /// 
    /// If this is used, the ConnectionString is not needed and managed identity is used
    /// </summary>
    public required string? BlobContainerUri { get; set; }

    /// <summary>
    /// The path to the folder where the images are stored.
    /// </summary>
    public required string FolderPath { get; set; }
}
