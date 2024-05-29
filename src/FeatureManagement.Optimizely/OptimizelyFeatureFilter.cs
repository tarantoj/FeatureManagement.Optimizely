using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

[FilterAlias(Alias)]
public class OptimizelyFeatureFilter : IFeatureFilter
{
    public const string Alias = "Optimizely";
    private readonly IOptimizely _optimizely;
    private readonly IUserProvider _userProvider;

    public OptimizelyFeatureFilter(
        IOptimizely optimizely,
        IUserProvider userProvider)
    {
        _optimizely = optimizely;
        _userProvider = userProvider;
    }

    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var (userId, userAttributes) = await _userProvider.GetUser();

        var userContext = _optimizely.CreateUserContext(userId, userAttributes);

        return userContext.Decide(context.FeatureName).Enabled;
    }
}
