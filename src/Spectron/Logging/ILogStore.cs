using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public interface ILogStore
{
    IReadOnlyList<LogEntry> Entries { get; }

    void Add(LogLevel logLevel, string message);
}