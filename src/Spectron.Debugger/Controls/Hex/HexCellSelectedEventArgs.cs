namespace OldBit.Spectron.Debugger.Controls.Hex;

internal sealed class HexCellSelectedEventArgs(int rowIndex, int position) : EventArgs
{
    internal int Position { get; } = position;

    internal int RowIndex { get; } = rowIndex;
}