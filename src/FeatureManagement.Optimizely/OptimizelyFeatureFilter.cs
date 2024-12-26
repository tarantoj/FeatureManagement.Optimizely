using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A feature filter that can be used to activate features from Optimizely
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
/// <param name="logger">A logger instance</param>
/// <param name="userProvider">An instance of <see cref="IUserProvider"/></param>
[FilterAlias(Alias)]
public class OptimizelyFeatureFilter(
    IOptimizely optimizely,
    ILogger<OptimizelyFeatureFilter> logger,
    IUserProvider userProvider
) : IFeatureFilter
{
    internal const string Alias = "Optimizely";
    internal static readonly FeatureFilterConfiguration Configuration = new() { Name = Alias };

    /// <inheritdoc/>
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var (userId, userAttributes) = await userProvider.GetUser();

        var userContext = optimizely.CreateUserContext(userId, userAttributes);

        var decision = userContext.Decide(context.FeatureName);

        logger.LogDecision(context.FeatureName, decision);

        return decision.Enabled;
    }
}
