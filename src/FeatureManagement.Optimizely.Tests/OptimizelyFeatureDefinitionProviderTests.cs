using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyFeatureDefinitionProviderTests
{
    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsFeatureForEachFeatureInConfig()
    {
        var provider = CreateProvider();

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        Assert.Equal(
            ["boolean_feature", "empty_feature", "forced_feature", "premium_feature"],
            definitions.Select(d => d.Name).OrderBy(name => name)
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
        var provider = CreateProvider();

        var definition = await provider.GetFeatureDefinitionAsync("empty_feature");

        Assert.NotNull(definition);
        Assert.Equal("empty_feature", definition.Name);
        Assert.Equal(RequirementType.All, definition.RequirementType);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsNullForUnknownFeature()
    {
        var provider = CreateProvider();

        var definition = await provider.GetFeatureDefinitionAsync("unknown_feature");

        Assert.Null(definition);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsEmptyWhenConfigIsUnavailable()
    {
        var provider = CreateUnavailableProvider();

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        Assert.Empty(definitions);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsNullWhenConfigIsUnavailable()
    {
        var provider = CreateUnavailableProvider();

        var definition = await provider.GetFeatureDefinitionAsync("boolean_feature");

        Assert.Null(definition);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_DoesNotSetTelemetryByDefault()
    {
        var provider = CreateProvider();

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        Assert.All(definitions, definition => Assert.Null(definition.Telemetry));
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_SetsConfiguredTelemetry()
    {
        var provider = CreateProvider(
            new OptimizelyOptions
            {
                Telemetry = new TelemetryConfiguration
                {
                    Enabled = true,
                    Metadata = new Dictionary<string, string> { ["source"] = "optimizely" },
                },
            }
        );

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        foreach (var definition in definitions)
        {
            Assert.NotNull(definition.Telemetry);
            Assert.True(definition.Telemetry.Enabled);
            Assert.Equal("optimizely", definition.Telemetry.Metadata["source"]);
        }
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_DoesNotShareTelemetryInstances()
    {
        var provider = CreateProvider(
            new OptimizelyOptions
            {
                Telemetry = new TelemetryConfiguration { Enabled = true },
            }
        );

        var definitions = await ToListAsync(provider.GetAllFeatureDefinitionsAsync());

        Assert.NotEmpty(definitions);
        foreach (var definition in definitions)
        {
            Assert.NotNull(definition.Telemetry);
        }

        Assert.NotSame(definitions[0].Telemetry, definitions[1].Telemetry);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsCachedDefinitions()
    {
        var provider = CreateProvider();

        var first = await provider.GetFeatureDefinitionAsync("empty_feature");
        var second = await provider.GetFeatureDefinitionAsync("empty_feature");

        Assert.Same(first, second);
    }

    private static OptimizelyFeatureDefinitionProvider CreateProvider(
        OptimizelyOptions? options = null
    ) =>
        new(
            TestDataFile.CreateOptimizely(),
            Options.Create(options ?? new OptimizelyOptions())
        );

    private static OptimizelyFeatureDefinitionProvider CreateUnavailableProvider() =>
        new(
            new OptimizelySDK.Optimizely("not a datafile", skipJsonValidation: true),
            Options.Create(new OptimizelyOptions())
        );

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
