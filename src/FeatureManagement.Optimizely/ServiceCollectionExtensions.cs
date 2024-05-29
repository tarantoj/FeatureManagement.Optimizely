using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Logger;

namespace TarantoJ.FeatureManagement.Optimizely;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureDefinitionProvider"/>, must be called before <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement"/>.
    /// </summary>
    public static IServiceCollection AddOptimizelyFeatureDefinitionProvider(this IServiceCollection services, string configSectionPath = "Optimizely")
    {
        services.AddOptions<OptimizelyOptions>()
                .BindConfiguration(configSectionPath)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<ILogger, LoggerAdapter>();

        services.AddSingleton<IOptimizely>((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<OptimizelyOptions>();

            if (options.Logging)
            {
                var logger = serviceProvider.GetRequiredService<ILogger>();
                OptimizelyFactory.SetLogger(logger);
            }

            return OptimizelyFactory.NewDefaultInstance(options.SdkKey);
        });

        return services.AddSingleton<IFeatureDefinitionProvider, OptimizelyFeatureDefinitionProvider>();
    }

    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureFilter"/>, must be registered after <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement"/>
    /// </summary>
    public static IFeatureManagementBuilder AddOptimizelyFeatureFilter(this IFeatureManagementBuilder features) =>
      features.AddFeatureFilter<OptimizelyFeatureFilter>();
}
