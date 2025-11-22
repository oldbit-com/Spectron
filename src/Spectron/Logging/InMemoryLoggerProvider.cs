using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public class InMemoryLoggerProvider(InMemoryLogStore logStore) : ILoggerProvider
{
    public ILogger CreateLogger(string name)
    {
        return new InMemoryLogger(name, logStore);
    }

    public void Dispose() { }
}