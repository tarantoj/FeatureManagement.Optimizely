using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Logger;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Extensions used to add Optimizely feature management functionality
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureDefinitionProvider"/>,
    /// must be called before <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement(IServiceCollection)"/>.
    /// </summary>
    public static IServiceCollection AddOptimizelyFeatureDefinitionProvider(
        this IServiceCollection services,
        Action<OptimizelyOptions> configureOptions
    )
    {
        services.Configure(configureOptions);

        services.AddSingleton<ILogger, LoggerAdapter>();

        services.AddSingleton<IOptimizely>(
            (serviceProvider) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<OptimizelyOptions>>()
                    .Value;
                if (options?.Logging ?? false)
                {
                    var logger = serviceProvider.GetRequiredService<LoggerAdapter>();
                    OptimizelyFactory.SetLogger(logger);
                }

                return OptimizelyFactory.NewDefaultInstance(options?.SdkKey);
            }
        );

        return services.AddSingleton<
            IFeatureDefinitionProvider,
            OptimizelyFeatureDefinitionProvider
        >();
    }

    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureFilter"/>,
    /// must be registered after <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement(IServiceCollection)"/>
    /// </summary>
    public static IFeatureManagementBuilder AddOptimizelyFeatureFilter(
        this IFeatureManagementBuilder features
    ) => features.AddFeatureFilter<OptimizelyFeatureFilter>();
}
