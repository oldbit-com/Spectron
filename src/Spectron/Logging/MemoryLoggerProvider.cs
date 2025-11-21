using System;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public class MemoryLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string name)
    {
        return new MemoryLogger(name);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}