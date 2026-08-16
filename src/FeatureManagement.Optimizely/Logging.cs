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
            [TagProvider(typeof(DecisionTagProvider), nameof(DecisionTagProvider.Decision))]
            OptimizelyDecision decision
        );

        [LoggerMessage(
            LogLevel.Warning,
            "Could not create an Optimizely user context for user {userId}"
        )]
        internal static partial void LogInvalidUserId(this ILogger logger, string? userId);
    }

    internal static class DecisionTagProvider
    {
        public static void Decision(ITagCollector tags, OptimizelyDecision decision)
        {
            tags.Add(nameof(decision.FlagKey), decision.FlagKey);
            tags.Add(nameof(decision.Enabled), decision.Enabled);
            tags.Add(nameof(decision.VariationKey), decision.VariationKey);
            tags.Add(nameof(decision.RuleKey), decision.RuleKey);
            tags.Add(nameof(decision.Reasons), decision.Reasons);
        }
    }
}
