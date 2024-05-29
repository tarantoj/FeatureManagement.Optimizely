using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely feature definition provider
/// </summary>
public class OptimizelyFeatureDefinitionProvider : IFeatureDefinitionProvider
{
    private readonly IOptimizely _optimizely;

    /// <summary>
    /// [TODO:description]
    /// </summary>
    /// <param name="optimizely">[TODO:parameter]</param>
    /// <returns>[TODO:return]</returns>
    public OptimizelyFeatureDefinitionProvider(IOptimizely optimizely)
    {
        _optimizely = optimizely;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() =>
        _optimizely.GetOptimizelyConfig()
            .FeaturesMap.Values.Select(feature => new FeatureDefinition
            {
                Name = feature.Key,
                RequirementType = RequirementType.All,
                EnabledFor = new FeatureFilterConfiguration[]
                    { OptimizelyFeatureFilter.Configuration }
            })
            .ToAsyncEnumerable();

    /// <inheritdoc/>
    public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName) =>
        GetAllFeatureDefinitionsAsync()
            .SingleAsync(feature => feature.Name == featureName)
            .AsTask();
}
