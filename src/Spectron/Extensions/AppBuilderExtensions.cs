using Avalonia;
using Avalonia.Logging;

namespace OldBit.Spectron.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder LogToConsole(
        this AppBuilder builder,
        LogEventLevel level = LogEventLevel.Warning,
        params string[] areas)
    {
        Logger.Sink = new ConsoleLogSink(level, areas);
        return builder;
    }
}