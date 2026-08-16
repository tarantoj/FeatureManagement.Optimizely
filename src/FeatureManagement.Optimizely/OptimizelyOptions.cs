using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely configuration options
/// </summary>
[PublicAPI]
public class OptimizelyOptions
{
    /// <summary>
    /// Your Optimizely sdk key. Supplied to <see cref="OptimizelySDK.OptimizelyFactory.NewDefaultInstance(string)"/>.
    /// </summary>
    [Required]
    public string SdkKey { get; set; } = string.Empty;

    /// <summary>
    /// Enables or disables logging using
    /// <see cref="Microsoft.Extensions.Logging.ILogger"/>,
    /// enabled by default.
    /// </summary>
    public bool Logging { get; set; } = true;

    /// <summary>
    /// The user id used when evaluating features without a registered
    /// <see cref="IUserProvider"/>.
    /// </summary>
    public string DefaultUserId { get; set; } = "anonymous-user";
}
