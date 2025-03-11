using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Disassembly;

public sealed class DisassemblerOptions
{
    public NumberFormat NumberFormat { get; set; } = NumberFormat.HexDollarPrefix;
}