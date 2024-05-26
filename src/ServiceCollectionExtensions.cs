using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Logger;

namespace TarantoJ.FeatureManagement.Optimizely;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptimizelyFeatureDefinitionProvider(this IServiceCollection services, string configSectionPath = "Optimizely")
    {
        services.AddOptions<OptimizelyOptions>()
                .BindConfiguration(configSectionPath)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<ILogger, LoggerAdapter>();

        services.AddSingleton<IOptimizely>((x) =>
        {
            var options = x.GetRequiredService<OptimizelyOptions>();
            var logger = x.GetRequiredService<ILogger>();

            OptimizelyFactory.SetLogger(logger);
            return OptimizelyFactory.NewDefaultInstance(options.SdkKey);
        });

        return services.AddSingleton<IFeatureDefinitionProvider, OptimizelyFeatureDefinitionProvider>();
    }

    public static IFeatureManagementBuilder AddOptimizelyFeatureFilter(this IFeatureManagementBuilder features)
    {
        return features.AddFeatureFilter<OptimizelyFeatureFilter>();
    }
}
