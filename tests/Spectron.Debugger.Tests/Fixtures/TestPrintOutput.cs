using OldBit.Spectron.Debugger.Parser;

namespace OldBit.Spectron.Debugger.Tests.Fixtures;

public class TestPrintOutput : IOutput
{
    public List<string> Lines { get; } = [];

    public void Print(string output) => Lines.Add(output);

    public void Clear() => Lines.Clear();
}