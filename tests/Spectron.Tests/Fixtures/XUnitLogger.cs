using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Tests.Fixtures;

public sealed class XUnitLogger(ITestOutputHelper output, string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        output.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");

        if (exception is not null)
        {
            output.WriteLine(exception.ToString());
        }
    }
}