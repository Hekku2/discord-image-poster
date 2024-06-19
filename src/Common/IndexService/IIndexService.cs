namespace DiscordImagePoster.Common.IndexService;

public interface IIndexService
{
    Task<ImageIndex?> GetImageIndexAsync();
    Task UpdateIndexAsync(ImageIndex index);
}
