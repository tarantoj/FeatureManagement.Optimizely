using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.OptlyConfig;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Optimizely feature definition provider
/// </summary>
/// <param name="optimizely">An instance of Optimizely</param>
/// <param name="options">The Optimizely configuration options</param>
[PublicAPI]
public class OptimizelyFeatureDefinitionProvider(
    IOptimizely optimizely,
    IOptions<OptimizelyOptions> options
) : IFeatureDefinitionProvider
{
    private static readonly IReadOnlyDictionary<string, FeatureDefinition> EmptyDefinitions =
        new Dictionary<string, FeatureDefinition>();

    // System.Threading.Lock requires .NET 9; this library also targets net8.0.
    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
    private readonly object _lock = new();
    private string? _cachedRevision;
    private IReadOnlyDictionary<string, FeatureDefinition>? _cache;

    /// <inheritdoc/>
    public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() =>
        GetDefinitions().Values.ToAsyncEnumerable();

    /// <inheritdoc/>
    public Task<FeatureDefinition?> GetFeatureDefinitionAsync(string featureName)
    {
        GetDefinitions().TryGetValue(featureName, out var definition);
        return Task.FromResult(definition);
    }

    private IReadOnlyDictionary<string, FeatureDefinition> GetDefinitions()
    {
        var config = optimizely.GetOptimizelyConfig();
        if (config is null)
        {
            return EmptyDefinitions;
        }

        lock (_lock)
        {
            if (_cache is not null && _cachedRevision == config.Revision)
            {
                return _cache;
            }

            _cachedRevision = config.Revision;
            _cache = config.FeaturesMap.Values.ToDictionary(feature => feature.Key, ToDefinition);

            return _cache;
        }
    }

    private FeatureDefinition ToDefinition(OptimizelyFeature feature) =>
        new()
        {
            Name = feature.Key,
            RequirementType = RequirementType.All,
            EnabledFor = [OptimizelyFeatureFilter.Configuration],
            Telemetry = CreateTelemetry(),
        };

    private TelemetryConfiguration? CreateTelemetry()
    {
        var telemetry = options.Value.Telemetry;
        if (telemetry is null)
        {
            return null;
        }

        return new TelemetryConfiguration
        {
            Enabled = telemetry.Enabled,
            Metadata = telemetry.Metadata is null
                ? null
                : new Dictionary<string, string>(telemetry.Metadata),
        };
    }
}
