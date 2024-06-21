namespace DiscordImagePoster.Common.IndexService;

public interface IIndexService
{
    /// <summary>
    /// Gets the current image index, if it exists.
    /// </summary>
    /// <returns>The current image index, or null if it does not exist.</returns>
    Task<ImageIndex?> GetIndexAsync();

    /// <summary>
    /// Gets the current image index, or creates a new one if it does not exist.
    /// </summary>
    /// <returns>The image index.</returns>
    Task<ImageIndex> GetIndexOrCreateNew();

    /// <summary>
    /// Refreshes the image index.
    /// </summary>
    /// <returns>The refreshed image index.</returns>
    Task<ImageIndex> RefreshIndexAsync();

    /// <summary>
    /// Increases the posting count for an image.
    /// </summary>
    Task IncreasePostingCountAsync(string imagePath);
}
