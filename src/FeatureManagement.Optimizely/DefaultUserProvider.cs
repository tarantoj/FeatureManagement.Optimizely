using Microsoft.Extensions.Options;
using OptimizelySDK.Entity;

namespace TarantoJ.FeatureManagement.Optimizely;

internal sealed class DefaultUserProvider(IOptions<OptimizelyOptions> options) : IUserProvider
{
    public Task<(string userId, UserAttributes? userAttributes)> GetUser() =>
        Task.FromResult((options.Value.DefaultUserId, (UserAttributes?)null));
}
