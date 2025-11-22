using System;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class InMemoryLogger(string name, ILogStore logStore) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var formatted = formatter(state, exception);

        if (string.IsNullOrEmpty(formatted) && exception == null)
        {
            return;
        }

        string message;

        if (string.IsNullOrEmpty(formatted))
        {
            message = $"{logLevel}: {exception}";
        }
        else if (exception == null)
        {
            message = $"{logLevel}: {formatted}";
        }
        else
        {
            message = $"{logLevel}: {formatted}{Environment.NewLine}{Environment.NewLine}{exception}";
        }

        logStore.Add(message);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    private class NullScope : IDisposable
    {
        internal static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}