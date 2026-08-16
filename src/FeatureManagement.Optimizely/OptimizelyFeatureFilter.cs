using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A feature filter that can be used to activate features from Optimizely
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve the current <see cref="IOptimizelyUserContextAccessor"/></param>
/// <param name="logger">A logger instance</param>
[FilterAlias(Alias)]
[PublicAPI]
public class OptimizelyFeatureFilter(
    IServiceProvider serviceProvider,
    ILogger<OptimizelyFeatureFilter> logger
) : IFeatureFilter
{
    private const string Alias = "Optimizely";
    internal static readonly FeatureFilterConfiguration Configuration = new() { Name = Alias };

    /// <inheritdoc/>
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        using var scope = serviceProvider.CreateScope();
        var userContextAccessor = scope.ServiceProvider.GetRequiredService<
            IOptimizelyUserContextAccessor
        >();

        OptimizelyUserContext? userContext = await userContextAccessor
            .GetUserContextAsync(context.CancellationToken)
            .ConfigureAwait(false);

        OptimizelyDecision? decision = OptimizelyDecisionService.Decide(
            userContext,
            logger,
            context.FeatureName
        );

        return decision?.Enabled ?? false;
    }
}
