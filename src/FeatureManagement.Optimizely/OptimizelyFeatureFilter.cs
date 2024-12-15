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

    private readonly IOptimizely _optimizely = optimizely;
    private readonly ILogger<OptimizelyFeatureFilter> _logger = logger;
    private readonly IUserProvider _userProvider = userProvider;

    /// <inheritdoc/>
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var (userId, userAttributes) = await _userProvider.GetUser();

        var userContext = _optimizely.CreateUserContext(userId, userAttributes);

        var decision = userContext.Decide(context.FeatureName);

        _logger.LogDebug(
            "Feature {FeatureName} has decision {@Decision}",
            context.FeatureName,
            decision
        );

        return decision.Enabled;
    }
}
