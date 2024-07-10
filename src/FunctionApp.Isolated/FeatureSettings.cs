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
    /// If true, the function will not analyze images.
    /// </summary>
    public bool DisableImageAnalysis { get; set; }

    /// <summary>
    /// If true, the function will not send to discord
    /// This is mostly used for debugging purposes.
    /// </summary>
    public bool DisableDiscordSending { get; set; }

    /// <summary>
    /// Creates instance of <see cref="FeatureSettings"/> with default values.
    /// </summary>
    public static FeatureSettings Default => new FeatureSettings
    {
        DisableTimedSending = false,
        DisableImageAnalysis = false,
        DisableDiscordSending = false
    };
}
