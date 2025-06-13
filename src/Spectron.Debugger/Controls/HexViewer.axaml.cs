using Avalonia;
using Avalonia.Controls.Primitives;

namespace OldBit.Spectron.Debugger.Controls;

public record HexViewerDataRow(int Address, HexViewerCell[] Cells);

public class HexViewer : TemplatedControl
{
    private const int DefaultCellHeight = 25;
    private const int DefaultCellWight = 25;
    private const int DefaultBytesPerRow = 16;

    public static readonly StyledProperty<double> CellHeightProperty =
        AvaloniaProperty.Register<HexViewer, double>(nameof(CellHeight), DefaultCellHeight);

    public static readonly StyledProperty<double> CellWidthProperty =
        AvaloniaProperty.Register<HexViewer, double>(nameof(CellWidth), DefaultCellWight);

    public static readonly StyledProperty<IEnumerable<byte>> DataProperty =
        AvaloniaProperty.Register<HexViewer, IEnumerable<byte>>(nameof(Data), []);

    private static readonly StyledProperty<int> BytesPerRowProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(BytesPerRow), DefaultBytesPerRow, validate: i => i > 0);

    public double CellHeight
    {
        get => GetValue(CellHeightProperty);
        set => SetValue(CellHeightProperty, value);
    }

    public double CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    public IEnumerable<byte> Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public int BytesPerRow
    {
        get => GetValue(BytesPerRowProperty);
        set => SetValue(BytesPerRowProperty, value);
    }
}