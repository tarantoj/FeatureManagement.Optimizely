using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OptimizelySDK;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

internal static class OptimizelyDecisionService
{
    public static async Task<OptimizelyDecision?> DecideAsync(
        IOptimizely optimizely,
        IServiceProvider serviceProvider,
        ILogger logger,
        string featureName,
        CancellationToken cancellationToken = default
    )
    {
        using var scope = serviceProvider.CreateScope();
        var userProvider = scope.ServiceProvider.GetRequiredService<IUserProvider>();

        var (userId, userAttributes) = await userProvider.GetUser().WaitAsync(cancellationToken);

        var userContext = optimizely.CreateUserContext(userId, userAttributes);
        if (userContext is null)
        {
            logger.LogInvalidUserId(userId);
            return null;
        }

        var decision = userContext.Decide(featureName);

        logger.LogDecision(featureName, decision);

        return decision;
    }
}
