namespace DiscordImagePoster.Common.IndexService;

public interface IIndexStorageService
{
    Task<ImageIndex?> GetImageIndexAsync();
    Task UpdateIndexAsync(ImageIndex index);
}
