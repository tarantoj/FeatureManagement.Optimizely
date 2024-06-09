using System.ComponentModel.DataAnnotations;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely configuration options
/// </summary>
public class OptimizelyOptions
{
    /// <summary>
    /// Your Optimizely sdk key. Supplied to <see cref="OptimizelySDK.OptimizelyFactory.NewDefaultInstance(string)"/>.
    /// </summary>
    [Required]
    public string SdkKey { get; init; } = string.Empty;

    /// <summary>
    /// Enables or disables logging using
    /// <see cref="Microsoft.Extensions.Logging.ILogger"/>,
    /// enabled by default.
    /// </summary>
    public bool Logging { get; init; } = true;
}
