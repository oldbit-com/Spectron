namespace OldBit.Spectron.Debugger.Controls.Hex;

public sealed record Selection(int Start, int End)
{
    public static Selection Empty { get; } = new(-1, -1);

    public int Start { get; } = Start;
    public int End { get; } = End;

    public int Length => End - Start + 1;
}