using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.Debugger.Controls;

public partial class HexViewerCell : ObservableObject
{
    [ObservableProperty]
    private byte _value;

    [ObservableProperty]
    private bool _isSelected;

    public int RowIndex { get; init; }
    public int ColumnIndex { get;init; }
}