using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class InMemoryLogStore : ILogStore
{
    public void Add(LogLevel logLevel, string message)
    {
        //
    }
}