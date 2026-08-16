using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A feature filter that can be used to activate features from Optimizely
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
/// <param name="logger">A logger instance</param>
/// <param name="serviceProvider">The service provider used to resolve the current <see cref="IUserProvider"/></param>
[FilterAlias(Alias)]
[PublicAPI]
public class OptimizelyFeatureFilter(
    IOptimizely optimizely,
    ILogger<OptimizelyFeatureFilter> logger,
    IServiceProvider serviceProvider
) : IFeatureFilter
{
    private const string Alias = "Optimizely";
    internal static readonly FeatureFilterConfiguration Configuration = new() { Name = Alias };

    /// <inheritdoc/>
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var decision = await OptimizelyDecisionService.DecideAsync(
            optimizely,
            serviceProvider,
            logger,
            context.FeatureName,
            context.CancellationToken
        );

        return decision?.Enabled ?? false;
    }
}
