using Microsoft.Extensions.Logging;
using OptimizelySDK;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

internal static class OptimizelyDecisionService
{
    public static OptimizelyDecision? Decide(
        OptimizelyUserContext? userContext,
        ILogger logger,
        string featureName
    )
    {
        if (userContext is null)
        {
            return null;
        }

        var decision = userContext.Decide(featureName);

        logger.LogDecision(featureName, decision);

        return decision;
    }
}
