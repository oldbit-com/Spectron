using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Logging;
using Avalonia.Utilities;

namespace OldBit.Spectron;

public class ConsoleLogSink(
    LogEventLevel minimumLevel,
    IList<string>? areas = null) : ILogSink
{
    private readonly IList<string>? _areas = areas?.Count > 0 ? areas : null;

    public bool IsEnabled(LogEventLevel level, string area) => level >=
        minimumLevel && (_areas?.Contains(area) ?? true);

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (IsEnabled(level, area))
        {
            Console.WriteLine(Format(area, messageTemplate, source, []));
        }
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (IsEnabled(level, area))
        {
            Console.WriteLine(Format(area, messageTemplate, source, propertyValues));
        }
    }

    private static string Format(
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