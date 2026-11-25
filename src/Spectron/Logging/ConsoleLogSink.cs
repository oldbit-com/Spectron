using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Logging;
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

        var logLevel = ToLogLevel(level);

        if (!logger.IsEnabled(logLevel))
        {
            return;
        }

        var message = FormatMessage(area, source, messageTemplate, propertyValues);
        var exception = FindException(propertyValues);

        logger.Log(logLevel, exception, "{Message}", message);
    }

    /// <summary>
    /// Formats an Avalonia message template by positionally replacing each named placeholder,
    /// e.g. "{Property}", with the corresponding property value. This mirrors how Avalonia's
    /// own TraceLogSink renders messages.
    /// </summary>
    internal static string FormatMessage(string area, object? source, string messageTemplate, object?[] propertyValues)
    {
        var result = new StringBuilder(messageTemplate.Length + 64);
        result.Append('[').Append(area).Append("] ");

        var valueIndex = 0;
        var i = 0;

        while (i < messageTemplate.Length)
        {
            var c = messageTemplate[i];

            if (c == '{' && i + 1 < messageTemplate.Length && messageTemplate[i + 1] == '{')
            {
                result.Append('{');
                i += 2;
                continue;
            }

            if (c == '}' && i + 1 < messageTemplate.Length && messageTemplate[i + 1] == '}')
            {
                result.Append('}');
                i += 2;
                continue;
            }

            if (c == '{')
            {
                var end = messageTemplate.IndexOf('}', i + 1);

                if (end < 0)
                {
                    // Unterminated placeholder, output the rest verbatim
                    result.Append(messageTemplate, i, messageTemplate.Length - i);
                    break;
                }

                var placeholder = messageTemplate.AsSpan(i + 1, end - i - 1);
                var formatSeparator = placeholder.IndexOf(':');
                var format = formatSeparator >= 0 ? placeholder[(formatSeparator + 1)..].ToString() : null;

                var value = valueIndex < propertyValues.Length ? propertyValues[valueIndex] : null;
                valueIndex += 1;

                result.Append('\'').Append(FormatValue(value, format)).Append('\'');

                i = end + 1;
                continue;
            }

            result.Append(c);
            i += 1;
        }

        if (source != null)
        {
            result.Append(" (").Append(source.GetType().Name).Append(" #").Append(source.GetHashCode()).Append(')');
        }

        return result.ToString();
    }

    private static string? FormatValue(object? value, string? format) => value switch
    {
        null => null,
        Exception exception => exception.Message,
        IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    private static Exception? FindException(object?[] propertyValues)
    {
        foreach (var value in propertyValues)
        {
            if (value is Exception exception)
            {
                return exception;
            }
        }

        return null;
    }

    private static LogLevel ToLogLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => LogLevel.Trace,
        LogEventLevel.Debug => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Warning => LogLevel.Warning,
        LogEventLevel.Error => LogLevel.Error,
        LogEventLevel.Fatal => LogLevel.Critical,
        _ => LogLevel.None,
    };
}
