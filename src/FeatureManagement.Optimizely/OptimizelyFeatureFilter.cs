using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A feature filter that can be used to activate features from Optimizely
/// </summary>
[FilterAlias(Alias)]
public class OptimizelyFeatureFilter : IFeatureFilter
{
    internal const string Alias = "Optimizely";
    internal static readonly FeatureFilterConfiguration Configuration = new() { Name = Alias };

    private readonly IOptimizely _optimizely;
    private readonly IUserProvider _userProvider;

    /// <summary>
    /// Creates an Optimizely based feature filter
    /// </summary>
    /// <param name="optimizely">An instance of Optimizely</param>
    /// <param name="userProvider">An instance </param>
    public OptimizelyFeatureFilter(
        IOptimizely optimizely,
        IUserProvider userProvider)
    {
        _optimizely = optimizely;
        _userProvider = userProvider;
    }

    /// <inheritdoc/>
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var (userId, userAttributes) = await _userProvider.GetUser();

        var userContext = _optimizely.CreateUserContext(userId, userAttributes);

        return userContext.Decide(context.FeatureName).Enabled;
    }
}
