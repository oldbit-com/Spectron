using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Debugger.Settings;

public record DebuggerSettings
{
    public NumberFormat NumberFormat { get; init; } = NumberFormat.HexPrefixDollar;
}