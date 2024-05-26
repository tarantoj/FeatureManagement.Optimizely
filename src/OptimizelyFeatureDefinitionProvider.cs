using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

public class OptimizelyFeatureDefinitionProvider : IFeatureDefinitionProvider
{
    private readonly IOptimizely _optimizely;

    public OptimizelyFeatureDefinitionProvider(IOptimizely optimizely)
    {
        _optimizely = optimizely;
    }

    public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() =>
        _optimizely.GetOptimizelyConfig()
            .FeaturesMap.Values.Select(feature => new FeatureDefinition
            {
                Name = feature.Key,
                RequirementType = RequirementType.All,
                EnabledFor = new FeatureFilterConfiguration[] { new() { Name = OptimizelyFeatureFilter.Alias } }
            })
            .ToAsyncEnumerable();

    public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName) =>
      GetAllFeatureDefinitionsAsync()
        .FirstAsync(feature => feature.Name == featureName)
        .AsTask();
}
