using Microsoft.Extensions.Logging;
using OldBit.Spectron.Logging;

namespace OldBit.Spectron.Tests.Fixtures;

public class TestLogStore(ITestOutputHelper output) : ILogStore
{
    public IReadOnlyList<LogEntry> Entries { get; } = [];

    public void Add(LogLevel logLevel, string message) =>
        output.WriteLine($"[{logLevel}] {message}");
}