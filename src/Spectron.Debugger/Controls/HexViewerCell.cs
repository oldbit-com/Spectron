using ReactiveUI;

namespace OldBit.Spectron.Debugger.Controls;

public class HexViewerCell : ReactiveObject
{
    private bool _isSelected;
    private byte _value;

    public byte Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public int RowIndex { get; init; }
    public int ColumnIndex { get; init; }
}