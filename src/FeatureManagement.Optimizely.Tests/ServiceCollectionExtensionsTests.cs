using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOptimizelyFeatureDefinitionProvider_RegistersProviderAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IFeatureDefinitionProvider));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(OptimizelyFeatureDefinitionProvider), descriptor.ImplementationType);
    }

    [Fact]
    public void AddOptimizelyFeatureDefinitionProvider_RegistersOptimizelyAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IOptimizely));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOptimizelyFeatureDefinitionProvider_ConfiguresOptions()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider(options =>
        {
            options.SdkKey = "test-sdk-key";
            options.Logging = false;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OptimizelyOptions>>().Value;

        Assert.Equal("test-sdk-key", options.SdkKey);
        Assert.False(options.Logging);
    }

    [Fact]
    public void AddOptimizelyFeatureDefinitionProvider_RegistersDefaultUserContextAccessorAsScoped()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IOptimizelyUserContextAccessor)
        );
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddOptimizelyFeatureFilter_RegistersOptimizelyFilters()
    {
        var services = new ServiceCollection();

        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        var descriptors = services
            .Where(d => d.ServiceType == typeof(IFeatureFilterMetadata))
            .ToList();

        Assert.Contains(
            descriptors,
            d => d.ImplementationType == typeof(OptimizelyFeatureFilter)
        );
        Assert.Contains(
            descriptors,
            d => d.ImplementationType == typeof(OptimizelyContextualFeatureFilter)
        );
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Singleton, d.Lifetime));
    }

    [Fact]
    public void AddOptimizelyVariantService_RegistersProviderAsScoped()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddOptimizelyVariantService<IWidget>("forced_feature");

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IVariantServiceProvider<IWidget>)
        );
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    private interface IWidget;

    [Fact]
    public async Task FeatureManagementPipeline_EvaluatesOptimizelyFeatures()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddSingleton<IOptimizelyUserContextAccessor>(
            (serviceProvider) =>
                new FakeUserContextAccessor(serviceProvider.GetRequiredService<IOptimizely>())
                {
                    Result = ("user1", null),
                }
        );
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("boolean_feature"));
        Assert.False(await features.IsEnabledAsync("empty_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_WithScopedUserContextAccessor_PassesScopeValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddScoped<IOptimizelyUserContextAccessor>(
            (serviceProvider) =>
                new FakeUserContextAccessor(serviceProvider.GetRequiredService<IOptimizely>())
                {
                    Result = ("user1", null),
                }
        );
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
        );
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_WithoutUserContextAccessor_UsesDefaultUser()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("forced_feature"));
        Assert.False(await features.IsEnabledAsync("empty_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_CustomUserContextAccessorRegisteredBefore_IsNotShadowedByDefault()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddSingleton<IOptimizelyUserContextAccessor>(
            (serviceProvider) =>
                new FakeUserContextAccessor(serviceProvider.GetRequiredService<IOptimizely>())
                {
                    Result = ("user2", null),
                }
        );
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_CustomUserContextAccessorRegisteredAfter_IsNotShadowedByDefault()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddSingleton<IOptimizelyUserContextAccessor>(
            (serviceProvider) =>
                new FakeUserContextAccessor(serviceProvider.GetRequiredService<IOptimizely>())
                {
                    Result = ("user2", null),
                }
        );
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_UsesConfiguredDefaultUser()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddOptimizelyFeatureDefinitionProvider(options =>
        {
            options.SdkKey = "test";
            options.DefaultUserId = "user2";
        });
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_EvaluatesFeatureForPassedUserContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();
        var optimizely = provider.GetRequiredService<IOptimizely>();

        Assert.True(
            await features.IsEnabledAsync("forced_feature", optimizely.CreateUserContext("user1"))
        );
        Assert.False(
            await features.IsEnabledAsync("forced_feature", optimizely.CreateUserContext("user2"))
        );
    }

    [Fact]
    public void AddOptimizelyFeatureDefinitionProvider_EmptySdkKey_ThrowsWhenResolved()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = string.Empty);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            provider.GetRequiredService<IOptimizely>
        );
    }
}
