namespace DiscordImagePoster.FunctionApp.Isolated;

/// <summary>
/// General settings for features
/// </summary>
public class FeatureSettings
{
    /// <summary>
    /// If true, the function will not send images on a timer.
    /// This is mostly used for debugging purposes.
    /// </summary>
    public bool DisableTimedSending { get; set; }

    /// <summary>
    /// If true, the function will not send to discord
    /// This is mostly used for debugging purposes.
    /// </summary>
    public bool DisableDiscordSending { get; set; }
}
