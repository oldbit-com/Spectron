using Avalonia;
using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Controls.Hex;

internal class HexViewerPanel(HexViewer viewer) : Panel
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

    internal void UpdateSelected(Selection selection)
    {
        var selections = new Dictionary<int, List<int>>();

        for (var index = selection.Start; index <= selection.End; index++)
        {
            var rowIndex = index / viewer.BytesPerRow;

            if (!selections.TryGetValue(rowIndex, out var cells))
            {
                cells = [];
                selections.Add(rowIndex, cells);
            }

            cells.Add(index % viewer.BytesPerRow);
        }

        // Select new range
        foreach (var (rowIndex, cells) in selections)
        {
            _visibleRows.GetValueOrDefault(rowIndex)?.SelectedIndexes = cells.ToArray();
        }

        // Unselect previous selection
        foreach (var row in _visibleRows.Values)
        {
            if (selections.ContainsKey(row.RowIndex))
            {
                continue;
            }

            if (row.SelectedIndexes.Length > 0)
            {
                row.SelectedIndexes = [];
            }
        }
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
        var rowCount = (viewer.Data.Length + viewer.BytesPerRow - 1) / viewer.BytesPerRow;
        double totalHeight = rowCount * viewer.RowHeight;

        return new Size(viewer.RowWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var (rowIndex, row) in _visibleRows)
        {
            var rect = new Rect(0, rowIndex * viewer.RowHeight, finalSize.Width, viewer.RowHeight);
            row.Arrange(rect);
        }

        return finalSize;
    }
}