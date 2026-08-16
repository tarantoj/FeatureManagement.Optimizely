using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Entity;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyFeatureFilterTests
{
    [Fact]
    public async Task EvaluateAsync_ReturnsTrueWhenOptimizelyEnablesTheFeature()
    {
        var filter = CreateFilter(userId: "user1");

        var enabled = await filter.EvaluateAsync(Context("forced_feature"));

        Assert.True(enabled);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsFalseWhenOptimizelyDisablesTheFeature()
    {
        var filter = CreateFilter(userId: "user1");

        var enabled = await filter.EvaluateAsync(Context("empty_feature"));

        Assert.False(enabled);
    }

    [Fact]
    public async Task EvaluateAsync_UsesTheUserIdFromTheUserContextAccessor()
    {
        var enabledForUser1 = await Evaluate(userId: "user1", feature: "forced_feature");
        var enabledForUser2 = await Evaluate(userId: "user2", feature: "forced_feature");

        Assert.True(enabledForUser1);
        Assert.False(enabledForUser2);
    }

    [Fact]
    public async Task EvaluateAsync_PassesUserAttributesFromTheUserContextAccessor()
    {
        var premiumUser = await Evaluate(
            userId: "anyone",
            feature: "premium_feature",
            attributes: new UserAttributes { ["plan"] = "premium" }
        );
        var freeUser = await Evaluate(userId: "anyone", feature: "premium_feature");

        Assert.True(premiumUser);
        Assert.False(freeUser);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsFalseWhenNoUserContextIsAvailable()
    {
        var filter = CreateFilter(userId: null!);

        var enabled = await filter.EvaluateAsync(Context("forced_feature"));

        Assert.False(enabled);
    }

    private static Task<bool> Evaluate(
        string userId,
        string feature,
        UserAttributes? attributes = null
    ) => CreateFilter(userId, attributes).EvaluateAsync(Context(feature));

    private static OptimizelyFeatureFilter CreateFilter(
        string userId,
        UserAttributes? attributes = null
    )
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddSingleton<IOptimizelyUserContextAccessor>(
            (serviceProvider) =>
                new FakeUserContextAccessor(serviceProvider.GetRequiredService<IOptimizely>())
                {
                    Result = (userId, attributes),
                }
        );
        var serviceProvider = services.BuildServiceProvider();

        return new OptimizelyFeatureFilter(
            serviceProvider,
            NullLogger<OptimizelyFeatureFilter>.Instance
        );
    }

    private static FeatureFilterEvaluationContext Context(string featureName) =>
        new() { FeatureName = featureName };
}

internal sealed class FakeUserContextAccessor(IOptimizely optimizely) : IOptimizelyUserContextAccessor
{
    public (string UserId, UserAttributes? Attributes) Result { get; init; } = ("user1", null);

    public ValueTask<OptimizelyUserContext?> GetUserContextAsync(
        CancellationToken cancellationToken = default
    ) =>
        ValueTask.FromResult<OptimizelyUserContext?>(
            optimizely.CreateUserContext(Result.UserId, Result.Attributes)
        );
}
