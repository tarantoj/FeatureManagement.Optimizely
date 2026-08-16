using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A feature filter that evaluates features from Optimizely using the
/// <see cref="OptimizelyUserContext"/> supplied by the caller.
/// </summary>
/// <param name="logger">A logger instance</param>
[FilterAlias(Alias)]
[PublicAPI]
public class OptimizelyContextualFeatureFilter(
    ILogger<OptimizelyContextualFeatureFilter> logger
) : IContextualFeatureFilter<OptimizelyUserContext>
{
    private const string Alias = "Optimizely";

    /// <inheritdoc/>
    public Task<bool> EvaluateAsync(
        FeatureFilterEvaluationContext featureFilterContext,
        OptimizelyUserContext userContext
    )
    {
        OptimizelyDecision? decision = OptimizelyDecisionService.Decide(
            userContext,
            logger,
            featureFilterContext.FeatureName
        );

        return Task.FromResult(decision?.Enabled ?? false);
    }
}
