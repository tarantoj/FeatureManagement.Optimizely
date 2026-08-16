# Getting Started

## Install

Install the `TarantoJ.FeatureManagement.Optimizely` NuGet package:

```bash
dotnet add package TarantoJ.FeatureManagement.Optimizely
```

## Configure

Register the Optimizely definition provider with your SDK key, then add feature management with the
Optimizely filter:

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
can be applied. Register an `IOptimizelyUserContextAccessor` that returns an `OptimizelyUserContext`
built from the current user:

```csharp
public class MyUserContextAccessor(IOptimizely optimizely) : IOptimizelyUserContextAccessor
{
    public ValueTask<OptimizelyUserContext?> GetUserContextAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(optimizely.CreateUserContext(
            "user-123",
            new UserAttributes { ["plan"] = "premium" }
        ));
}

builder.Services.AddScoped<IOptimizelyUserContextAccessor, MyUserContextAccessor>();
```

If your application always evaluates for the same user (or you pass an `OptimizelyUserContext` to
`IsEnabledAsync` yourself), you can skip the accessor; features are then evaluated for the configured
`DefaultUserId`.

## Use feature flags

Consume features with the standard Microsoft Feature Management API:

```csharp
public class ExampleService(IFeatureManager features)
{
    public async Task<bool> IsBetaEnabled() =>
        await features.IsEnabledAsync("beta_flag");
}
```

The feature name must match the flag key in your Optimizely project.
