using JetBrains.Annotations;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Provides the Optimizely user context used to evaluate features for the
/// current user.
/// </summary>
[PublicAPI]
public interface IOptimizelyUserContextAccessor
{
    /// <summary>
    /// Gets the current Optimizely user context, or null when no user is available.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The user context, or null if a user context could not be created</returns>
    ValueTask<OptimizelyUserContext?> GetUserContextAsync(
        CancellationToken cancellationToken = default
    );
}
