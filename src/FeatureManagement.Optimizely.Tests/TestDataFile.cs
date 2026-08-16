using System.Reflection;
using OptimizelySDK.Event;
using OptimizelySDK.Event.Dispatcher;
using OptimizelySDK.Logger;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

internal static class TestDataFile
{
    private const string ResourceName =
        "TarantoJ.FeatureManagement.Optimizely.Tests.TestData.datafile.json";

    public static readonly string Json;

    static TestDataFile()
    {
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");
        using var reader = new StreamReader(stream);
        Json = reader.ReadToEnd();
    }

    public static OptimizelySDK.Optimizely CreateOptimizely() =>
        new OptimizelySDK.Optimizely(
            Json,
            eventDispatcher: new NoOpEventDispatcher(),
            skipJsonValidation: true
        );
}

internal sealed class NoOpEventDispatcher : IEventDispatcher
{
    public ILogger Logger { get; set; } = new NoOpLogger();

    public void DispatchEvent(LogEvent logEvent) { }
}
