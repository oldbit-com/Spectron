using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class InMemoryLogStore : ILogStore
{
    private readonly List<LogEntry> _entries = [];

    public IReadOnlyList<LogEntry> Entries => _entries;

    public void Add(LogLevel logLevel, string message)
    {
        _entries.Add(new LogEntry(logLevel, message));

        if (_entries.Count > 1500)
        {
            _entries.RemoveRange(0, 500);
        }
    }
}