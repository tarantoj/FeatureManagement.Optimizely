using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using OptimizelySDK.Config;
using OptimizelySDK.ErrorHandler;
using OptimizelySDK.Event;
using OptimizelySDK.Event.Dispatcher;
using OptimizelySDK.Notifications;

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
            )
            .ValidateOnStart();

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

                var optimizelyLogger = microsoftLogger is null
                    ? null
                    : new LoggerAdapter(microsoftLogger);

                return CreateOptimizely(options.SdkKey, optimizelyLogger);
            }
        );

        services.TryAddSingleton<
            IFeatureDefinitionProvider,
            OptimizelyFeatureDefinitionProvider
        >();

        return services;
    }

    private static OptimizelySDK.Optimizely CreateOptimizely(
        string sdkKey,
        OptimizelySDK.Logger.ILogger? logger
    )
    {
        var effectiveLogger = logger ?? new OptimizelySDK.Logger.NoOpLogger();
        var notificationCenter = new NotificationCenter();
        var errorHandler = new DefaultErrorHandler(effectiveLogger, false);
        var eventDispatcher = new DefaultEventDispatcher(effectiveLogger);

        var configManager = new HttpProjectConfigManager.Builder()
            .WithSdkKey(sdkKey)
            .WithLogger(effectiveLogger)
            .WithErrorHandler(errorHandler)
            .WithNotificationCenter(notificationCenter)
            .Build(true);

        var eventProcessor = new BatchEventProcessor.Builder()
            .WithLogger(effectiveLogger)
            .WithEventDispatcher(eventDispatcher)
            .WithNotificationCenter(notificationCenter)
            .Build();

        return OptimizelyFactory.NewDefaultInstance(
            configManager,
            notificationCenter,
            eventDispatcher,
            errorHandler,
            effectiveLogger,
            eventProcessor: eventProcessor
        );
    }
}
