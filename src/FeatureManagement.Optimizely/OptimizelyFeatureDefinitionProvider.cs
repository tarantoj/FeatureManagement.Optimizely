using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely feature definition provider
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
public class OptimizelyFeatureDefinitionProvider(IOptimizely optimizely)
    : IFeatureDefinitionProvider
{
    /// <inheritdoc/>
    public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() =>
        optimizely
            .GetOptimizelyConfig()
            .FeaturesMap.Values.Select(feature => new FeatureDefinition
            {
                Name = feature.Key,
                RequirementType = RequirementType.All,
                EnabledFor = [OptimizelyFeatureFilter.Configuration],
            })
            .ToAsyncEnumerable();

    /// <inheritdoc/>
    public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName) =>
        GetAllFeatureDefinitionsAsync()
            .SingleAsync(feature => feature.Name == featureName)
            .AsTask();
}
