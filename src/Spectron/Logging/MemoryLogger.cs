using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class MemoryLogger(string name) : ILogger
{
    private const int Capacity = 2000;

    private readonly List<string> _log = [];

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var formatted = formatter(state, exception);

        if (string.IsNullOrEmpty(formatted) && exception == null)
        {
            return;
        }

        var message = string.Empty;

        if (string.IsNullOrEmpty(formatted))
        {
            System.Diagnostics.Debug.Assert(exception != null);
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

        _log.Add(message);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    private class NullScope : IDisposable
    {
        internal static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}