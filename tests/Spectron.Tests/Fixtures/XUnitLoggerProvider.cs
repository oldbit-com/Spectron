using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Tests.Fixtures;

public class XUnitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new XUnitLogger(output, categoryName);

    public void Dispose() { }
}