using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Debugger.Settings;

public record DebuggerSettings
{
    public NumberFormat PreferredNumberFormat { get; set; } = NumberFormat.HexPrefixDollar;
}