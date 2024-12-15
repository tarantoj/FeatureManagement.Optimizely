using Microsoft.Extensions.Logging;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// An adapter between <see cref="OptimizelySDK.Logger"/> and <see cref="ILogger"/>
/// </summary>
public class LoggerAdapter : OptimizelySDK.Logger.ILogger
{
    private readonly ILogger<IOptimizely>? _logger;

    /// <summary>
    /// An adapter between <see cref="OptimizelySDK.Logger"/> and <see cref="ILogger"/>
    /// </summary>
    /// <param name="logger">A logger instance</param>
    public LoggerAdapter(ILogger<IOptimizely>? logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Log(OptimizelySDK.Logger.LogLevel level, string message) =>
#pragma warning disable CA2254 // Template should be a static expression
        _logger?.Log(MapLogLevel(level), message);
#pragma warning restore CA2254 // Template should be a static expression

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
