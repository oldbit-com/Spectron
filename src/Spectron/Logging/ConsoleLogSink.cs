using System.Collections.Generic;
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

        var message = $"[{area}] {messageTemplate}";

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
}