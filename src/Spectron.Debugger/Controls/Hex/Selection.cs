namespace OldBit.Spectron.Debugger.Controls.Hex;

internal sealed class Selection
{
    public int Start { get; set; } = -1;
    public int End { get; set; } = -1;

    public int Length => End - Start + 1;
}