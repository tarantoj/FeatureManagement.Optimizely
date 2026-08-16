using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// An adapter between <see cref="OptimizelySDK.Logger"/> and <see cref="ILogger"/>
/// </summary>
/// <param name="logger">A logger instance</param>
[PublicAPI]
public class LoggerAdapter(ILogger<IOptimizely>? logger) : OptimizelySDK.Logger.ILogger
{
    /// <inheritdoc/>
    public void Log(OptimizelySDK.Logger.LogLevel level, string message) =>
        logger?.Log(MapLogLevel(level), "{OptimizelyMessage}", message);

    private static LogLevel MapLogLevel(OptimizelySDK.Logger.LogLevel level) =>
        level switch
        {
            OptimizelySDK.Logger.LogLevel.DEBUG => LogLevel.Debug,
            OptimizelySDK.Logger.LogLevel.INFO => LogLevel.Information,
            OptimizelySDK.Logger.LogLevel.WARN => LogLevel.Warning,
            OptimizelySDK.Logger.LogLevel.ERROR => LogLevel.Error,
            _ => LogLevel.None,
        };
}
