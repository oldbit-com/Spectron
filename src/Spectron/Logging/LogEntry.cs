using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public record LogEntry(LogLevel Level, string Message);