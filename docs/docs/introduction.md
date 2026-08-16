# Introduction

`FeatureManagement.Optimizely` connects two feature flag systems:

- [Microsoft Feature Management](https://learn.microsoft.com/dotnet/microsoft.extensions.featuremanagement)
  provides the `IFeatureManager` API and the feature flag evaluation pipeline used by your application.
- [Optimizely Feature Experimentation](https://developers.optimizely.com/x/experiments/) is the source
  of truth for your feature flags, targeting rules, and experiments.

The library exposes three pieces that plug into the Microsoft Feature Management pipeline:

## `OptimizelyFeatureDefinitionProvider`

An `IFeatureDefinitionProvider` that lists the features from your Optimizely project
(via `OptimizelyClient.GetOptimizelyConfig()`). Every feature found in Optimizely is registered with
Microsoft Feature Management and wired up to be evaluated by the `OptimizelyFeatureFilter`.

## `OptimizelyFeatureFilter`

An `IFeatureFilter` (alias `Optimizely`) that evaluates a feature by asking Optimizely for a decision.
It creates an `OptimizelyUserContext` from the current user supplied by your `IUserProvider`, calls
`Decide(featureName)`, and returns whether the feature is enabled for that user. This is what makes
Optimizely's audiences, targeting rules, and experiments take effect.

## `IUserProvider`

Your application's implementation of `IUserProvider` supplies the current user (`userId`) and any
user attributes to the filter so Optimizely can evaluate targeting rules for that user.

## Registration order

The definition provider must be registered **before** `AddFeatureManagement()`, and the feature filter
must be added **after** it:

```csharp
builder.Services.AddOptimizelyFeatureDefinitionProvider(options =>
{
    options.SdkKey = "...";
});

builder.Services
    .AddFeatureManagement()
    .AddOptimizelyFeatureFilter();
```
