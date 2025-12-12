using Avalonia;
using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Controls.Hex;

internal class HexViewerPanel(Spectron.Debugger.Controls.Hex.HexViewer parent) : Panel
{
    private readonly Dictionary<int, HexViewerRow> _visibleRows = [];

    internal void Add(HexViewerRow row)
    {
        Children.Add(row);
        _visibleRows.Add(row.RowIndex, row);
    }

    internal bool ContainsRow(int rowIndex) => _visibleRows.ContainsKey(rowIndex);

    internal void Clear()
    {
        _visibleRows.Clear();
        Children.Clear();
    }

    internal void RemoveNotVisibleRows(int startIndex, int endIndex)
    {
        var removeIndices = _visibleRows.Where(row => row.Key > endIndex || row.Key < startIndex)
            .Select(row => row.Key)
            .ToList();

        foreach (var index in removeIndices)
        {
            Children.Remove(_visibleRows[index]);
            _visibleRows.Remove(index);
        }
    }

    internal void UpdateSelected(int rowIndex, int position)
    {
        foreach (var visibleRow in _visibleRows.Values)
        {
            visibleRow.SelectedIndex = -1;
        }

        if (!_visibleRows.TryGetValue(rowIndex, out var  row))
        {
            return;
        }

        row.SelectedIndex = position;
    }

    internal new void InvalidateVisual()
    {
        foreach (var row in _visibleRows.Values)
        {
            row.InvalidateVisual();
        }

        base.InvalidateVisual();
    }

    internal void InvalidateRow(int rowIndex)
    {
        if (_visibleRows.TryGetValue(rowIndex, out var row))
        {
            row.InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var rowCount = (parent.Data.Length + parent.BytesPerRow - 1) / parent.BytesPerRow;
        double totalHeight = rowCount * parent.RowHeight;

        return new Size(parent.RowWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var (rowIndex, row) in _visibleRows)
        {
            var rect = new Rect(0, rowIndex * parent.RowHeight, finalSize.Width, parent.RowHeight);
            row.Arrange(rect);
        }

        return finalSize;
    }
}