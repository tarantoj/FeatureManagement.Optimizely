using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OptimizelySDK;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A client for evaluating Optimizely features and retrieving their decisions
/// (variation key, variables, and reasons).
/// </summary>
[PublicAPI]
public interface IOptimizelyFeatureClient
{
    /// <summary>
    /// Gets the Optimizely decision (assigned variation and variables) for the
    /// current user.
    /// </summary>
    /// <param name="featureName">The Optimizely feature key</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The decision, or null if a user context could not be created</returns>
    Task<OptimizelyDecision?> GetVariantAsync(
        string featureName,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// The default implementation of <see cref="IOptimizelyFeatureClient"/>.
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
/// <param name="logger">A logger instance</param>
/// <param name="serviceProvider">The service provider used to resolve the current <see cref="IUserProvider"/></param>
[PublicAPI]
public class OptimizelyFeatureClient(
    IOptimizely optimizely,
    ILogger<OptimizelyFeatureClient> logger,
    IServiceProvider serviceProvider
) : IOptimizelyFeatureClient
{
    /// <inheritdoc/>
    public Task<OptimizelyDecision?> GetVariantAsync(
        string featureName,
        CancellationToken cancellationToken = default
    ) =>
        OptimizelyDecisionService.DecideAsync(
            optimizely,
            serviceProvider,
            logger,
            featureName,
            cancellationToken
        );
}
