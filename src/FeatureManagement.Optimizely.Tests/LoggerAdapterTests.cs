using Microsoft.Extensions.Logging;
using OptimizelySDK;
using OptimizelyLogLevel = OptimizelySDK.Logger.LogLevel;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class LoggerAdapterTests
{
    [Theory]
    [InlineData(OptimizelyLogLevel.DEBUG, LogLevel.Debug)]
    [InlineData(OptimizelyLogLevel.INFO, LogLevel.Information)]
    [InlineData(OptimizelyLogLevel.WARN, LogLevel.Warning)]
    [InlineData(OptimizelyLogLevel.ERROR, LogLevel.Error)]
    public void Log_MapsOptimizelyLogLevelToMicrosoftLogLevel(
        OptimizelyLogLevel optimizelyLevel,
        LogLevel expectedLevel
    )
    {
        var logger = new CapturingLogger<IOptimizely>();
        var adapter = new LoggerAdapter(logger);

        adapter.Log(optimizelyLevel, "message");

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(expectedLevel, entry.Level);
        Assert.Equal("message", entry.Message);
    }

    [Fact]
    public void Log_WithNullLogger_DoesNotThrow()
    {
        var adapter = new LoggerAdapter(logger: null);

        var exception = Record.Exception(() =>
            adapter.Log(OptimizelyLogLevel.WARN, "message")
        );

        Assert.Null(exception);
    }

    [Fact]
    public void Log_MessageContainingBraces_DoesNotThrow()
    {
        var logger = new CapturingLogger<IOptimizely>();
        var adapter = new LoggerAdapter(logger);
        const string message = "message with {braces} and {0} placeholders";

        adapter.Log(OptimizelyLogLevel.INFO, message);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(message, entry.Message);
    }
}

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) => Entries.Add((logLevel, formatter(state, exception)));
}
