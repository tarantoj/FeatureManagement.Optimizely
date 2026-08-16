using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using OptimizelySDK;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyVariantServiceProviderTests
{
    private interface IWidget;

    [VariantServiceAlias("var_on")]
    private sealed class VarOnWidget : IWidget;

    [VariantServiceAlias("var_off")]
    private sealed class VarOffWidget : IWidget;

    [Fact]
    public async Task GetServiceAsync_ReturnsServiceForAssignedVariation()
    {
        var provider = BuildProvider("user1", services =>
        {
            services.AddScoped<IWidget, VarOnWidget>();
            services.AddScoped<IWidget, VarOffWidget>();
            services.AddOptimizelyVariantService<IWidget>("forced_feature");
        });

        await using var scope = provider.CreateAsyncScope();
        var variantService = scope.ServiceProvider.GetRequiredService<
            IVariantServiceProvider<IWidget>
        >();

        var widget = await variantService.GetServiceAsync(CancellationToken.None);

        Assert.IsType<VarOnWidget>(widget);
    }

    [Fact]
    public async Task GetServiceAsync_MatchesVariantServiceAlias()
    {
        var provider = BuildProvider("user2", services =>
        {
            services.AddScoped<IWidget, VarOnWidget>();
            services.AddScoped<IWidget, VarOffWidget>();
            services.AddOptimizelyVariantService<IWidget>("forced_feature");
        });

        await using var scope = provider.CreateAsyncScope();
        var variantService = scope.ServiceProvider.GetRequiredService<
            IVariantServiceProvider<IWidget>
        >();

        var widget = await variantService.GetServiceAsync(CancellationToken.None);

        Assert.IsType<VarOffWidget>(widget);
    }

    [Fact]
    public async Task GetServiceAsync_MatchesKeyedService()
    {
        var provider = BuildProvider("user1", services =>
        {
            services.AddKeyedScoped<IWidget, VarOnWidget>("var_on");
            services.AddKeyedScoped<IWidget, VarOffWidget>("var_off");
            services.AddOptimizelyVariantService<IWidget>("forced_feature");
        });

        await using var scope = provider.CreateAsyncScope();
        var variantService = scope.ServiceProvider.GetRequiredService<
            IVariantServiceProvider<IWidget>
        >();

        var widget = await variantService.GetServiceAsync(CancellationToken.None);

        Assert.IsType<VarOnWidget>(widget);
    }

    [Fact]
    public async Task GetServiceAsync_ReturnsNullWhenNoVariation()
    {
        var provider = BuildProvider("user1", services =>
        {
            services.AddScoped<IWidget, VarOnWidget>();
            services.AddOptimizelyVariantService<IWidget>("empty_feature");
        });

        await using var scope = provider.CreateAsyncScope();
        var variantService = scope.ServiceProvider.GetRequiredService<
            IVariantServiceProvider<IWidget>
        >();

        var widget = await variantService.GetServiceAsync(CancellationToken.None);

        Assert.Null(widget);
    }

    private static ServiceProvider BuildProvider(
        string userId,
        Action<ServiceCollection> configure
    )
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUserProvider>(new FakeUserProvider { Result = (userId, null) });
        services.AddSingleton<IOptimizely>(TestDataFile.CreateOptimizely());
        services.AddOptimizelyFeatureDefinitionProvider(options => options.SdkKey = "test");
        configure(services);

        return services.BuildServiceProvider();
    }
}
