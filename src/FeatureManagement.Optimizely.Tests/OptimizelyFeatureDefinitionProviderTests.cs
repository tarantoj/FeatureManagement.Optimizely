using Microsoft.FeatureManagement;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyFeatureDefinitionProviderTests
{
    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsFeatureForEachFeatureInConfig()
    {
        var provider = new OptimizelyFeatureDefinitionProvider(TestDataFile.CreateOptimizely());

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        Assert.Equal(
            new[] { "boolean_feature", "empty_feature", "premium_feature", "forced_feature" },
            definitions.Select(d => d.Name)
        );
        Assert.All(definitions, definition =>
        {
            Assert.Equal(RequirementType.All, definition.RequirementType);
            var filter = Assert.Single(definition.EnabledFor);
            Assert.Equal("Optimizely", filter.Name);
        });
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsDefinitionForKnownFeature()
    {
        var provider = new OptimizelyFeatureDefinitionProvider(TestDataFile.CreateOptimizely());

        var definition = await provider.GetFeatureDefinitionAsync("empty_feature");

        Assert.NotNull(definition);
        Assert.Equal("empty_feature", definition!.Name);
        Assert.Equal(RequirementType.All, definition.RequirementType);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsNullForUnknownFeature()
    {
        var provider = new OptimizelyFeatureDefinitionProvider(TestDataFile.CreateOptimizely());

        var definition = await provider.GetFeatureDefinitionAsync("unknown_feature");

        Assert.Null(definition);
    }

    private static async Task<List<FeatureDefinition>> ToListAsync(
        IAsyncEnumerable<FeatureDefinition> source
    )
    {
        var result = new List<FeatureDefinition>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }
}
