using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public interface ILogStore
{
    void Add(LogLevel logLevel, string message);
}