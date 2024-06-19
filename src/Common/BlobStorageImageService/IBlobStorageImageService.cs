using Azure.Storage.Blobs.Models;

namespace DiscordImagePoster.Common.BlobStorageImageService;

public interface IBlobStorageImageService
{
    Task<string[]> GetAllImagesAsync();
    Task<BlobDownloadStreamingResult?> GetImageStream(string name);
}
