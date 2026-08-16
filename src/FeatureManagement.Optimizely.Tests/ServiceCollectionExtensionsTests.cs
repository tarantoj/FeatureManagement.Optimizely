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
    public void AddOptimizelyFeatureDefinitionProvider_GenericOverloadRegistersUserProviderAsScoped()
    {
        var services = new ServiceCollection();

        services.AddOptimizelyFeatureDefinitionProvider<FakeUserProvider>(options =>
            options.SdkKey = "test"
        );

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IUserProvider));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(FakeUserProvider), descriptor.ImplementationType);
    }

    [Fact]
    public void AddOptimizelyFeatureFilter_RegistersOptimizelyFilter()
    {
        var services = new ServiceCollection();

        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        var descriptor = Assert.Single(
            services,
            d =>
                d.ServiceType == typeof(IFeatureFilterMetadata)
                && d.ImplementationType == typeof(OptimizelyFeatureFilter)
        );
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public async Task FeatureManagementPipeline_EvaluatesOptimizelyFeatures()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider<FakeUserProvider>(options =>
            options.SdkKey = "test"
        );
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("boolean_feature"));
        Assert.False(await features.IsEnabledAsync("empty_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_WithScopedUserProvider_PassesScopeValidation()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider<FakeUserProvider>(options =>
            options.SdkKey = "test"
        );
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
        );
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_WithoutUserProvider_UsesDefaultUser()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.True(await features.IsEnabledAsync("forced_feature"));
        Assert.False(await features.IsEnabledAsync("empty_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_CustomUserProviderRegisteredBefore_IsNotShadowedByDefault()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IUserProvider>(
            new FakeUserProvider { Result = ("user2", null) }
        );
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_CustomUserProviderRegisteredAfter_IsNotShadowedByDefault()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        services.AddSingleton<IUserProvider>(
            new FakeUserProvider { Result = ("user2", null) }
        );
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
    }

    [Fact]
    public async Task FeatureManagementPipeline_UsesConfiguredDefaultUser()
    {
        var services = new ServiceCollection();
        services.AddOptimizelyFeatureDefinitionProvider(options =>
        {
            options.SdkKey = "test";
            options.DefaultUserId = "user2";
        });
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddFeatureManagement().AddOptimizelyFeatureFilter();

        await using var provider = services.BuildServiceProvider();
        var features = provider.GetRequiredService<IFeatureManager>();

        Assert.False(await features.IsEnabledAsync("forced_feature"));
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
