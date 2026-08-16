using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Microsoft.FeatureManagement;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely configuration options
/// </summary>
[PublicAPI]
public class OptimizelyOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="OptimizelyOptions"/>.
    /// </summary>
    public const string SectionName = "Optimizely";

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
    /// <see cref="IOptimizelyUserContextAccessor"/>.
    /// </summary>
    public string DefaultUserId { get; set; } = "anonymous-user";

    /// <summary>
    /// Telemetry configuration applied to every feature returned by
    /// <see cref="OptimizelyFeatureDefinitionProvider"/>. When null, no
    /// telemetry is emitted. Disabled by default.
    /// </summary>
    public TelemetryConfiguration? Telemetry { get; set; }
}
