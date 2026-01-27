namespace OldBit.Spectron.Debugger.Controls.Hex;

internal sealed class HexCellClickedEventArgs(int rowIndex, int position, bool isShiftPressed) : EventArgs
{
    internal int Position { get; } = position;

    internal int RowIndex { get; } = rowIndex;

    internal bool IsShiftPressed { get; } = isShiftPressed;
}
