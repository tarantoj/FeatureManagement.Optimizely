# FeatureManagement.Optimizely

A bridge between [Microsoft Feature Management](https://github.com/microsoft/FeatureManagement-Dotnet)
and [Optimizely Feature Experimentation](https://docs.developers.optimizely.com/feature-experimentation/docs/csharp-sdk).

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

### Configuration from appsettings.json

Instead of hard-coding the options, bind `OptimizelyOptions` from your configuration:

```json
// appsettings.json
{
  "Optimizely": {
    "SdkKey": "your-sdk-key",
    "Logging": true,
    "DefaultUserId": "anonymous-user",
    "Telemetry": { "Enabled": true }
  }
}
```

```csharp
builder.Services.Configure<OptimizelyOptions>(
    builder.Configuration.GetSection(OptimizelyOptions.SectionName));

builder.Services.AddOptimizelyFeatureDefinitionProvider(_ => { });
```

`OptimizelyOptions.SectionName` (`"Optimizely"`) is a constant on the options type, so the section name
stays in sync. Values set in the `Action` passed to `AddOptimizelyFeatureDefinitionProvider` are applied
after configuration binding, so they take precedence over `appsettings.json`; you can mix both, for
example by keeping `SdkKey` in configuration and overriding `DefaultUserId` in code.

### Telemetry

Setting `OptimizelyOptions.Telemetry` (`TelemetryConfiguration`, disabled by default) enables
Microsoft Feature Management telemetry for every Optimizely feature: the feature manager emits an
`EvaluationEvent` as a `System.Diagnostics.ActivityEvent` whenever a feature is evaluated. Its
`Metadata` dictionary is carried onto each event. You need an `Activity` listener or an OpenTelemetry
integration to observe the events.

```csharp
builder.Services.AddOptimizelyFeatureDefinitionProvider(options =>
{
    options.SdkKey = "your-sdk-key";
    options.Telemetry = new TelemetryConfiguration
    {
        Enabled = true,
        Metadata = new Dictionary<string, string> { ["source"] = "optimizely" },
    };
});
```

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

`IUserProvider` is resolved from a DI scope for every evaluation, so a scoped implementation can safely
depend on request-scoped services (or on `IHttpContextAccessor` when it needs the current request).

If your application always evaluates for the same user, you can use the non-generic overload and skip
`IUserProvider`; features are then evaluated for the configured `DefaultUserId`
(`"anonymous-user"` by default):

```csharp
builder.Services.AddOptimizelyFeatureDefinitionProvider(options =>
{
    options.SdkKey = "your-sdk-key";
    options.DefaultUserId = "shared-user";
});
```

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

### Variants and variables

When a feature is evaluated for a user, Optimizely also assigns a variation with variables (for
example, which feature flag variant your user is bucketed into). Retrieve the full decision with
`IOptimizelyFeatureClient` (registered with the definition provider):

```csharp
public class ExampleService(IOptimizelyFeatureClient optimizely)
{
    public async Task<string?> GetHeroTitle()
    {
        var decision = await optimizely.GetVariantAsync("hero_content");

        return decision?.Variables.ToDictionary().TryGetValue("title", out var title) == true
            ? title as string
            : null;
    }
}
```

`OptimizelyDecision` exposes `VariationKey` (the assigned variation), `Variables`
(`OptimizelyJSON.ToDictionary()`), `RuleKey`, `Enabled`, and `Reasons`. It is `null` only when no user
context could be created for the current user.

### Switching service implementations by variation

To select a different service implementation based on the variation assigned to the current user,
register `IVariantServiceProvider<TService>` with `AddOptimizelyVariantService<TService>`. The
provider implements the Microsoft Feature Management interface and resolves `TService` by the
Optimizely variation key — via a keyed registration, a `[VariantServiceAlias]`, or the implementation
type name (case-insensitive).

```csharp
builder.Services.AddScoped<IPaymentGateway, CardGateway>();    // assigned to the "card" variation
builder.Services.AddScoped<IPaymentGateway, CryptoGateway>();  // assigned to the "crypto" variation

builder.Services.AddOptimizelyVariantService<IPaymentGateway>("checkout_payment");

public class CheckoutService(IVariantServiceProvider<IPaymentGateway> payment)
{
    public async Task Pay()
    {
        var gateway = await payment.GetServiceAsync(CancellationToken.None);

        await gateway.Charge();
    }
}
```

```csharp
[VariantServiceAlias("card")]
public class CardGateway : IPaymentGateway { }

// or, equivalently, register the implementation keyed by the variation:
builder.Services.AddKeyedScoped<IPaymentGateway, CardGateway>("card");
```

`IVariantServiceProvider<TService>` is registered as scoped, so implementations are resolved from the
current request scope. It returns `null` when the feature is not assigned a variation (or no
implementation matches).

## Testing

The test project (`src/FeatureManagement.Optimizely.Tests`) runs against a real Optimizely SDK
instance built from a minimal embedded datafile, so the suite is fast and fully offline. It targets
the same frameworks as the library.

```bash
dotnet test src
```

This builds and runs the suite against `net8.0`, `net9.0`, `net10.0`, and `net11.0`.

## Development with devenv

The repo uses [devenv](https://devenv.sh) for its development environment, which provisions all four
.NET SDKs (`net8.0`–`net11.0`) and the project tooling. Configuration lives in `devenv.nix`
(`devenv.yaml`/`devenv.lock` pin the inputs).

Enter the development shell:

```bash
devenv shell
```

Inside the shell the following commands are available (each is defined under `scripts.*` in
`devenv.nix`, so they also run as `devenv <name>`):

| Command | Purpose |
| --- | --- |
| `restore` | Restore NuGet packages (`dotnet restore src`) |
| `build` | Build the solution (`dotnet build src`) |
| `test` | Run the test suite on all target frameworks (`dotnet test src`) |
| `docs` | Generate the docfx documentation (`docs/docfx.json`) |
| `inspect` | Run ReSharper inspections with `jb inspectcode` (writes `inspect-report.sarif`) |

ReSharper inspections use the `jetbrains.resharper.globaltools` local tool (pinned in
`.config/dotnet-tools.json`, version `2026.2.0.2`). The CI workflow runs the same inspection
(`.github/workflows/dotnet.yml`) and uploads the SARIF report as a code-scanning alert, so keep the
report free of findings before pushing.

You can also run the built-in devenv checks directly:

```bash
devenv test    # runs the tests
devenv lint    # runs git hooks (e.g. nixfmt)
devenv check   # runs the CI checks
```

After changing `devenv.yaml`, run `devenv update` to refresh `devenv.lock`.

OpenCode is configured declaratively through the `opencode.*` options in `devenv.nix`; the generated
files under `.opencode/` and `opencode.jsonc` are gitignored and should not be edited by hand. After
changing them, regenerate with `devenv shell` and restart OpenCode.

## License

[MIT](LICENSE)
