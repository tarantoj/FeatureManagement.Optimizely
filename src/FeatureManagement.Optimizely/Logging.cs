using Microsoft.Extensions.Logging;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely
{
    internal static partial class Logging
    {
        [LoggerMessage(LogLevel.Debug, "Feature {featureName} has decision {decision}")]
        internal static partial void LogDecision(
            this ILogger logger,
            string featureName,
            [LogProperties] OptimizelyDecision decision
        );
    }
}
