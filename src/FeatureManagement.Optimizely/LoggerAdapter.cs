using Microsoft.Extensions.Logging;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

public class LoggerAdapter : OptimizelySDK.Logger.ILogger
{
    private readonly ILogger<IOptimizely> _logger;

    public LoggerAdapter(ILogger<IOptimizely> logger)
    {
        _logger = logger;
    }

    public void Log(OptimizelySDK.Logger.LogLevel level, string message) =>
      _logger.Log(MapLogLevel(level), message);

    private static LogLevel MapLogLevel(OptimizelySDK.Logger.LogLevel level) => level switch
    {
        OptimizelySDK.Logger.LogLevel.DEBUG => LogLevel.Debug,
        OptimizelySDK.Logger.LogLevel.INFO => LogLevel.Information,
        OptimizelySDK.Logger.LogLevel.WARN => LogLevel.Warning,
        OptimizelySDK.Logger.LogLevel.ERROR => LogLevel.Error,
        _ => LogLevel.Debug
    };
}
