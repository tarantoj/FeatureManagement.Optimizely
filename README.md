# FeatureManagement.Optimizely

A bridge between [Microsoft Feature Management](https://learn.microsoft.com/dotnet/microsoft.extensions.featuremanagement)
and [Optimizely Feature Experimentation](https://developers.optimizely.com/x/experiments/).

Provides an Optimizely-backed `IFeatureDefinitionProvider` and `IFeatureFilter`, letting you drive
ASP.NET Core feature flags from your Optimizely project with the `IFeatureManager` API you already use.

- Targets `net8.0`, `net9.0`, `net10.0`, and `net11.0`
- Documentation: <https://tarantoj.github.io/FeatureManagement.Optimizely/>

## Installation

```bash
dotnet add package TarantoJ.FeatureManagement.Optimizely
```

## Configuration

Register the Optimizely definition provider with your SDK key, then add feature management with the
Optimizely filter. The provider must be registered **before** `AddFeatureManagement()`, and the filter
must be added **after** it.

```csharp
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptimizelyFeatureDefinitionProvider(options =>
{
    options.SdkKey = "your-sdk-key";
    options.Logging = true;
});

builder.Services
    .AddFeatureManagement()
    .AddOptimizelyFeatureFilter();

var app = builder.Build();
app.Run();
```

`OptimizelyOptions.SdkKey` is required. `Logging` is enabled by default and forwards Optimizely log
output to `ILogger`.

## Provide the current user

Optimizely needs to know which user is evaluating the feature so that audiences and targeting rules
can be applied. Implement `IUserProvider` and register it with the generic overload:

```csharp
public class MyUserProvider : IUserProvider
{
    public Task<(string userId, UserAttributes? userAttributes)> GetUser() =>
        Task.FromResult(("user-123", new UserAttributes { ["plan"] = "premium" }));
}

builder.Services.AddOptimizelyFeatureDefinitionProvider<MyUserProvider>(options =>
{
    options.SdkKey = "your-sdk-key";
});
```

If your application always evaluates for the same user, you can use the non-generic overload and skip
`IUserProvider`.

## Usage

Consume features with the standard Microsoft Feature Management API:

```csharp
public class ExampleService(IFeatureManager features)
{
    public async Task<bool> IsBetaEnabled() =>
        await features.IsEnabledAsync("beta_flag");
}
```

The feature name must match the flag key in your Optimizely project.

## License

[MIT](LICENSE)
