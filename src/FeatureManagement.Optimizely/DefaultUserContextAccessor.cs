using Microsoft.Extensions.Options;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

internal sealed class DefaultUserContextAccessor(
    IOptimizely optimizely,
    IOptions<OptimizelyOptions> options
) : IOptimizelyUserContextAccessor
{
    public ValueTask<OptimizelyUserContext?> GetUserContextAsync(
        CancellationToken cancellationToken = default
    ) =>
        ValueTask.FromResult<OptimizelyUserContext?>(
            optimizely.CreateUserContext(options.Value.DefaultUserId)
        );
}
