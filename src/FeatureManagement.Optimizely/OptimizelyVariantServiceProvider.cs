using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using OptimizelySDK.OptimizelyDecisions;

namespace TarantoJ.FeatureManagement.Optimizely;

internal sealed class OptimizelyVariantServiceProvider<TService>(
    string featureName,
    IOptimizelyFeatureClient optimizely,
    IServiceProvider serviceProvider
) : IVariantServiceProvider<TService>
    where TService : class
{
    public async ValueTask<TService> GetServiceAsync(CancellationToken cancellationToken)
    {
        OptimizelyDecision? decision = await optimizely.GetVariantAsync(
            featureName,
            cancellationToken
        );

        TService? service = decision?.VariationKey is { } variationKey
            ? ResolveVariantService(variationKey)
            : null;

        return service!;
    }

    private TService? ResolveVariantService(string variantName)
    {
        if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
        {
            TService? keyedService = keyedServiceProvider.GetKeyedService<TService>(variantName);

            if (keyedService is not null)
            {
                return keyedService;
            }
        }

        return serviceProvider
            .GetServices<TService>()
            .FirstOrDefault(service => IsMatchingVariantName(service.GetType(), variantName));
    }

    private static bool IsMatchingVariantName(Type implementationType, string variantName)
    {
        string? implementationName =
            ((VariantServiceAliasAttribute?)Attribute.GetCustomAttribute(
                implementationType,
                typeof(VariantServiceAliasAttribute)
            ))?.Alias;

        implementationName ??= implementationType.Name;

        return string.Equals(implementationName, variantName, StringComparison.OrdinalIgnoreCase);
    }
}
