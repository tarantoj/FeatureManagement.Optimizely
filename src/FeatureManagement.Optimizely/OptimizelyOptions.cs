using System.ComponentModel.DataAnnotations;

namespace TarantoJ.FeatureManagement.Optimizely;

public class OptimizelyOptions
{
    /// <summary>
    /// Your Optimizely sdk key. Supplied to <see cref="OptimizelySDK.OptimizelyFactory.NewDefaultInstance"/>.
    /// </summary>
    [Required]
    public string SdkKey { get; init; }

    /// <summary>
    /// Enables or disables logging using
    /// <see cref="Microsoft.Extensions.Logging.ILogger"/>,
    /// enabled by default.
    /// </summary>
    public bool Logging { get; init; } = true;
}
