using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Logging;
using Avalonia.Utilities;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class ConsoleLogSink(ILogger<Program> logger, LogEventLevel minimumLevel, IList<string>? areas = null) : ILogSink
{
    private readonly IList<string>? _areas = areas?.Count > 0 ? areas : null;

    public bool IsEnabled(LogEventLevel level, string area) => level >=
        minimumLevel && (_areas?.Contains(area) ?? true);

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate) =>
        Log(level, area, source, messageTemplate, []);

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (!IsEnabled(level, area))
        {
            return;
        }

        var message = FormatMessage(area, messageTemplate, source, propertyValues);

        switch (level)
        {
            case LogEventLevel.Verbose:
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("{Message}", message);
                }
                break;

            case LogEventLevel.Debug:
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("{Message}", message);
                }
                break;

            case LogEventLevel.Information:
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("{Message}", message);
                }
                break;

            case LogEventLevel.Warning:
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("{Message}", message);
                }
                break;

            case LogEventLevel.Error:
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("{Message}", message);
                }
                break;

            case LogEventLevel.Fatal:
                if (logger.IsEnabled(LogLevel.Critical))
                {
                    logger.LogCritical("{Message}", message);
                }
                break;
        }
    }

    private static string FormatMessage(
        string area,
        string template,
        object? source,
        object?[] v)
    {
        var result = new StringBuilder(template.Length);
        var r = new CharacterReader(template.AsSpan());
        var i = 0;

        result.Append('[');
        result.Append(area);
        result.Append(']');

        while (!r.End)
        {
            var c = r.Take();

            if (c != '{')
            {
                result.Append(c);
            }
            else
            {
                if (r.Peek != '{')
                {
                    result.Append('\'');
                    result.Append(i < v.Length ? v[i++] : null);
                    result.Append('\'');
                    r.TakeUntil('}');
                    r.Take();
                }
                else
                {
                    result.Append('{');
                    r.Take();
                }
            }
        }

        if (source != null)
        {
            result.Append('(');
            result.Append(source.GetType().Name);
            result.Append(" #");
            result.Append(source.GetHashCode());
            result.Append(')');
        }

        return result.ToString();
    }
}