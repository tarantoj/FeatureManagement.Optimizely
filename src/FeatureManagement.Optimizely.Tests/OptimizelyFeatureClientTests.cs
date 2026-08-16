using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyFeatureClientTests
{
    [Fact]
    public async Task GetVariantAsync_ReturnsDecisionWithAssignedVariation()
    {
        var client = CreateClient(userId: "user1");

        var decision = await client.GetVariantAsync("forced_feature");

        Assert.NotNull(decision);
        Assert.Equal("var_on", decision.VariationKey);
        Assert.True(decision.Enabled);
    }

    [Fact]
    public async Task GetVariantAsync_ReturnsDisabledDecisionWhenUserIsNotTargeted()
    {
        var client = CreateClient(userId: "user2");

        var decision = await client.GetVariantAsync("forced_feature");

        Assert.NotNull(decision);
        Assert.Equal("var_off", decision.VariationKey);
        Assert.False(decision.Enabled);
    }

    [Fact]
    public async Task GetVariantAsync_ReturnsDecisionForFeatureWithoutVariation()
    {
        var client = CreateClient(userId: "user1");

        var decision = await client.GetVariantAsync("empty_feature");

        Assert.NotNull(decision);
        Assert.False(decision.Enabled);
    }

    [Fact]
    public async Task GetVariantAsync_ReturnsNullWhenUserIdIsNull()
    {
        var client = CreateClient(userId: null!);

        var decision = await client.GetVariantAsync("forced_feature");

        Assert.Null(decision);
    }

    private static OptimizelyFeatureClient CreateClient(string userId)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IUserProvider>(
            new FakeUserProvider { Result = (userId, null) }
        );
        var serviceProvider = services.BuildServiceProvider();

        return new OptimizelyFeatureClient(
            TestDataFile.CreateOptimizely(),
            NullLogger<OptimizelyFeatureClient>.Instance,
            serviceProvider
        );
    }
}
