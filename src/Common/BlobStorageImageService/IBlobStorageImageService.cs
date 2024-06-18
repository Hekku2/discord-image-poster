using Azure.Storage.Blobs.Models;

namespace DiscordImagePoster.Common.BlobStorageImageService;

public interface IBlobStorageImageService
{
    Task<(string, BlobDownloadStreamingResult)?> GetRandomImageStream();
}
