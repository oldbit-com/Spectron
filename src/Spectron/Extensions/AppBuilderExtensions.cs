using Avalonia;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Logging;

namespace OldBit.Spectron.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder LogToConsole(
        this AppBuilder builder,
        ILogger<Program> logger,
        LogEventLevel level = LogEventLevel.Warning,
        params string[] areas)
    {
        Logger.Sink = new ConsoleLogSink(logger, level, areas);
        return builder;
    }
}