using JetBrains.Annotations;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.OptlyConfig;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely feature definition provider
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
[PublicAPI]
public class OptimizelyFeatureDefinitionProvider(IOptimizely optimizely)
    : IFeatureDefinitionProvider
{
    /// <inheritdoc/>
    public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
    {
        var config = optimizely.GetOptimizelyConfig();

        return config is null
            ? Array.Empty<FeatureDefinition>().ToAsyncEnumerable()
            : config.FeaturesMap.Values.Select(ToDefinition).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public Task<FeatureDefinition?> GetFeatureDefinitionAsync(string featureName)
    {
        var config = optimizely.GetOptimizelyConfig();

        FeatureDefinition? definition = null;
        if (config is not null && config.FeaturesMap.TryGetValue(featureName, out var feature))
        {
            definition = ToDefinition(feature);
        }

        return Task.FromResult(definition);
    }

    private static FeatureDefinition ToDefinition(OptimizelyFeature feature) =>
        new()
        {
            Name = feature.Key,
            RequirementType = RequirementType.All,
            EnabledFor = [OptimizelyFeatureFilter.Configuration],
        };
}
