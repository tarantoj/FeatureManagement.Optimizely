# Testing

The test project (`src/FeatureManagement.Optimizely.Tests`) covers the definition provider, the
feature filter, the DI registration extensions, the log adapter, and the options validation.

Tests use a real Optimizely SDK instance built from a minimal embedded datafile, so they are fast,
deterministic, and run fully offline (no SDK key or network access required).

## Running the tests

```bash
dotnet test src
```

The suite multi-targets the same frameworks as the library and runs against `net8.0`, `net9.0`,
`net10.0`, and `net11.0`:

```bash
# Run against a single framework
dotnet test src/FeatureManagement.Optimizely.Tests -f net10.0

# Run a single test class
dotnet test src --filter FullyQualifiedName~ServiceCollectionExtensionsTests
```
