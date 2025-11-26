using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public sealed class InMemoryLoggerProvider(ILogStore logStore) : ILoggerProvider
{
    public ILogger CreateLogger(string name) => new InMemoryLogger(logStore);

    public void Dispose() { }
}