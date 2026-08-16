using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OptimizelySDK;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// Extensions used to add Optimizely feature management functionality
/// </summary>
[PublicAPI]
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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services.AddOptimizelyFeatureDefinitionProviderInternal(configureOptions);
    }

    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureDefinitionProvider"/>,
    /// must be called before <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement(IServiceCollection)"/>.
    /// </summary>
    /// <typeparam name="TUserProvider">An implementation of <see cref="IUserProvider" /></typeparam>
    public static IServiceCollection AddOptimizelyFeatureDefinitionProvider<TUserProvider>(
        this IServiceCollection services,
        Action<OptimizelyOptions> configureOptions
    )
        where TUserProvider : class, IUserProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.TryAddScoped<IUserProvider, TUserProvider>();

        return services.AddOptimizelyFeatureDefinitionProviderInternal(configureOptions);
    }

    /// <summary>
    /// Registers the Optimizely <see cref="IFeatureFilter"/>,
    /// must be registered after <see cref="Microsoft.FeatureManagement.ServiceCollectionExtensions.AddFeatureManagement(IServiceCollection)"/>
    /// </summary>
    public static IFeatureManagementBuilder AddOptimizelyFeatureFilter(
        this IFeatureManagementBuilder features
    )
    {
        ArgumentNullException.ThrowIfNull(features);

        return features.AddFeatureFilter<OptimizelyFeatureFilter>();
    }

    /// <summary>
    /// Registers an <see cref="IVariantServiceProvider{TService}"/> that resolves
    /// <typeparamref name="TService"/> according to the Optimizely variation assigned to the
    /// current user for <paramref name="featureName"/>.
    /// </summary>
    /// <typeparam name="TService">The service type to vary by Optimizely variation key</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="featureName">The Optimizely feature key that determines the assigned variation</param>
    public static IServiceCollection AddOptimizelyVariantService<TService>(
        this IServiceCollection services,
        string featureName
    )
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(featureName);

        services.TryAddScoped<IVariantServiceProvider<TService>>(
            (serviceProvider) =>
                new OptimizelyVariantServiceProvider<TService>(
                    featureName,
                    serviceProvider.GetRequiredService<IOptimizelyFeatureClient>(),
                    serviceProvider
                )
        );

        return services;
    }

    private static IServiceCollection AddOptimizelyFeatureDefinitionProviderInternal(
        this IServiceCollection services,
        Action<OptimizelyOptions> configureOptions
    )
    {
        services.Configure(configureOptions);

        services
            .AddOptions<OptimizelyOptions>()
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SdkKey),
                $"{nameof(OptimizelyOptions.SdkKey)} must not be empty."
            );

        services.TryAddSingleton<IUserProvider, DefaultUserProvider>();

        services.TryAddSingleton<IOptimizely>(
            (serviceProvider) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<OptimizelyOptions>>()
                    .Value;

                var microsoftLogger = options.Logging
                    ? serviceProvider.GetService<ILogger<IOptimizely>>()
                    : null;
                OptimizelyFactory.SetLogger(new LoggerAdapter(microsoftLogger));

                return OptimizelyFactory.NewDefaultInstance(options.SdkKey);
            }
        );

        services.TryAddSingleton<
            IFeatureDefinitionProvider,
            OptimizelyFeatureDefinitionProvider
        >();

        services.TryAddSingleton<IOptimizelyFeatureClient, OptimizelyFeatureClient>();

        return services;
    }
}
