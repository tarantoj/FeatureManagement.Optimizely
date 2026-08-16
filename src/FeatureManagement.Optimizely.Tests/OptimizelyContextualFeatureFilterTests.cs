using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Entity;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyContextualFeatureFilterTests
{
    [Fact]
    public async Task EvaluateAsync_ReturnsTrueWhenOptimizelyEnablesTheFeature()
    {
        var filter = CreateFilter();

        var enabled = await filter.EvaluateAsync(
            Context("forced_feature"),
            UserContext("user1")
        );

        Assert.True(enabled);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsFalseWhenOptimizelyDisablesTheFeature()
    {
        var filter = CreateFilter();

        var enabled = await filter.EvaluateAsync(Context("empty_feature"), UserContext("user1"));

        Assert.False(enabled);
    }

    [Fact]
    public async Task EvaluateAsync_UsesTheUserIdFromTheContext()
    {
        var filter = CreateFilter();

        var enabledForUser1 = await filter.EvaluateAsync(
            Context("forced_feature"),
            UserContext("user1")
        );
        var enabledForUser2 = await filter.EvaluateAsync(
            Context("forced_feature"),
            UserContext("user2")
        );

        Assert.True(enabledForUser1);
        Assert.False(enabledForUser2);
    }

    [Fact]
    public async Task EvaluateAsync_PassesUserAttributesFromTheContext()
    {
        var filter = CreateFilter();

        var premiumUser = await filter.EvaluateAsync(
            Context("premium_feature"),
            UserContext("anyone", new UserAttributes { ["plan"] = "premium" })
        );
        var freeUser = await filter.EvaluateAsync(
            Context("premium_feature"),
            UserContext("anyone")
        );

        Assert.True(premiumUser);
        Assert.False(freeUser);
    }

    private static OptimizelyContextualFeatureFilter CreateFilter() =>
        new(NullLogger<OptimizelyContextualFeatureFilter>.Instance);

    private static OptimizelyUserContext UserContext(
        string userId,
        UserAttributes? attributes = null
    ) => TestDataFile.CreateOptimizely().CreateUserContext(userId, attributes);

    private static FeatureFilterEvaluationContext Context(string featureName) =>
        new() { FeatureName = featureName };
}
