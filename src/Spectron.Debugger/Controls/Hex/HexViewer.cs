using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace OldBit.Spectron.Debugger.Controls.Hex;

public class HexViewer : ContentControl
{
    public static readonly StyledProperty<int> RowHeightProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(RowHeight), defaultValue: 20);

    public static readonly StyledProperty<int> BytesPerRowProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(BytesPerRow), defaultValue: 16, inherits: true);

    public static readonly StyledProperty<bool> IsOffsetVisibleProperty =
        AvaloniaProperty.Register<HexViewer, bool>(nameof(IsOffsetVisible), true, inherits: true);

    public static readonly StyledProperty<bool> IsHeaderVisibleProperty =
        AvaloniaProperty.Register<HexViewer, bool>(nameof(IsHeaderVisible), defaultValue: true);

    public static readonly StyledProperty<int> GroupSizeProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(GroupSize), defaultValue: 8);

    public static readonly DirectProperty<HexViewer, byte[]> DataProperty =
        AvaloniaProperty.RegisterDirect<HexViewer, byte[]>(
            nameof(Data),
            getter: o => o.Data,
            setter: (o, v) => o.Data = v,
            unsetValue: []);

    public static readonly StyledProperty<int[]> SelectedIndexesProperty =
        AvaloniaProperty.Register<HexViewer, int[]>(nameof(SelectedIndexes), []);

    internal static readonly StyledProperty<Typeface> TypefaceProperty =
        AvaloniaProperty.Register<HexViewer, Typeface>(nameof(Typeface), inherits: true);

    internal static readonly StyledProperty<RowTextBuilder> RowTextBuilderProperty =
        AvaloniaProperty.Register<HexViewer, RowTextBuilder>(nameof(RowTextBuilder), inherits: true);

    public static readonly StyledProperty<bool> IsMultiSelectProperty =
        AvaloniaProperty.Register<HexViewer, bool>(nameof(IsMultiSelect), defaultValue: true);

    public int RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public int BytesPerRow
    {
        get => GetValue(BytesPerRowProperty);
        set => SetValue(BytesPerRowProperty, value);
    }

    public bool IsOffsetVisible
    {
        get => GetValue(IsOffsetVisibleProperty);
        set => SetValue(IsOffsetVisibleProperty, value);
    }

    public bool IsHeaderVisible
    {
        get => GetValue(IsHeaderVisibleProperty);
        set => SetValue(IsHeaderVisibleProperty, value);
    }

    public int GroupSize
    {
        get => GetValue(GroupSizeProperty);
        set => SetValue(GroupSizeProperty, value);
    }

    public bool IsMultiSelect
    {
        get => GetValue(IsMultiSelectProperty);
        set => SetValue(IsMultiSelectProperty, value);
    }

    public byte[] Data
    {
        get;
        set
        {
            if (value.Length == Data.Length)
            {
                Array.Copy(value, Data, value.Length);
                _hexPanel.InvalidateVisual();
            }
            else
            {
                if (!SetAndRaise(DataProperty, ref field, value))
                {
                    return;
                }

                SelectedIndexes = [];
                _hexPanel.Clear();
                UpdateView();
            }
        }
    } = [];

    internal Typeface Typeface
    {
        get => GetValue(TypefaceProperty);
        private set => SetValue(TypefaceProperty, value);
    }

    internal RowTextBuilder RowTextBuilder
    {
        get => GetValue(RowTextBuilderProperty);
        set => SetValue(RowTextBuilderProperty, value);
    }

    public int[] SelectedIndexes
    {
        get => GetValue(SelectedIndexesProperty);
        set => SetValue(SelectedIndexesProperty, value);
    }

    private readonly HexViewerHeader _header;
    private readonly ScrollViewer _scrollViewer;
    private readonly HexViewerPanel _hexPanel;

    private readonly Selection _selection = new();
    private int _cursorPosition = -1;
    private int _selectionAnchor = -1;

    private int CurrentRowIndex => _selection.Start / BytesPerRow;
    private int VisibleRowCount => (int)(_scrollViewer.Viewport.Height / RowHeight);

    private int PageHeight => VisibleRowCount * RowHeight;
    private int PageSize => VisibleRowCount * BytesPerRow;

    internal double RowWidth { get; private set; }

    public HexViewer()
    {
        Focusable = true;

        AffectsMeasure<HexViewer>(RowHeightProperty);
        AffectsMeasure<HexViewer>(BytesPerRowProperty);

        _header = new HexViewerHeader()
        {
            Height = RowHeight,
        };
        _hexPanel = new HexViewerPanel(this);

        _scrollViewer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _hexPanel,
        };

        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;

        var headerHost = new Border
        {
            ClipToBounds = true,
            Child = _header
        };

        var dockPanel = new DockPanel();
        dockPanel.Children.Add(headerHost);
        dockPanel.Children.Add(_scrollViewer);
        DockPanel.SetDock(headerHost, Dock.Top);

        this.GetObservable(FontFamilyProperty).Subscribe(_ => Invalidate());
        this.GetObservable(FontSizeProperty).Subscribe(_ => Invalidate());
        this.GetObservable(FontStyleProperty).Subscribe(_ => Invalidate());
        this.GetObservable(FontStretchProperty).Subscribe(_ => Invalidate());
        this.GetObservable(IsOffsetVisibleProperty).Subscribe(_ => Invalidate());
        this.GetObservable(BytesPerRowProperty).Subscribe(_ => Invalidate());
        this.GetObservable(IsHeaderVisibleProperty).Subscribe(_ => _header.IsVisible = IsHeaderVisible);
        this.GetObservable(SelectedIndexesProperty).Subscribe(_ => _hexPanel.UpdateSelected(_selection));

        _scrollViewer.GetObservable(ScrollViewer.OffsetProperty).Subscribe(offset =>
        {
            _header.RenderTransform = new TranslateTransform(-offset.X, 0);
            UpdateView();
        });
        _scrollViewer.GetObservable(ScrollViewer.ViewportProperty).Subscribe(_ => UpdateView());

        Content = dockPanel;
    }

    public void UpdateValues(int offset, params byte[] values)
    {
        var rowIndices = new HashSet<int>();

        for (var i = 0; i < values.Length; i++)
        {
            if (offset + i >= Data.Length)
            {
                break;
            }

            Data[offset + i] = values[i];

            var rowIndex = offset / BytesPerRow;
            rowIndices.Add(rowIndex);
        }

        foreach (var rowIndex in rowIndices)
        {
            _hexPanel.InvalidateRow(rowIndex);
        }
    }

    public void Select(int start, int length = 1)
    {
        _selection.Start = start;
        _selection.End = start + length - 1;
        _cursorPosition = _selection.End;
        _selectionAnchor = _selection.Start;

        var rowIndex = _selection.Start / BytesPerRow;
        if (!IsRowVisible(rowIndex))
        {
            ScrollToRow(rowIndex);
        }

        RefreshSelection();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                MoveLeft(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: 1);
                break;

            case Key.Right:
                MoveRight(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: 1);
                break;

            case Key.Up:
                MoveLeft(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: BytesPerRow);

                if (!IsRowVisible(CurrentRowIndex))
                {
                    ScrollToRow(CurrentRowIndex);
                }
                break;

            case Key.Down:
                MoveRight(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: BytesPerRow);

                if (!IsRowVisible(CurrentRowIndex))
                {
                    var viewportRows = (int)(_scrollViewer.Viewport.Height / RowHeight);
                    var targetTopRow = CurrentRowIndex - Math.Max(0, viewportRows) + 1;

                    ScrollToRow(targetTopRow);
                }
                break;

            case Key.Home:
                _cursorPosition = 0;
                _selection.Start = _cursorPosition;

                if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    _selection.End = _selection.Start;
                    _selectionAnchor = _cursorPosition;
                }

                _scrollViewer.ScrollToHome();
                break;

            case Key.End:
                _cursorPosition = Data.Length - 1;
                _selection.End = _cursorPosition;

                if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    _selection.Start = _selection.End;
                    _selectionAnchor = _cursorPosition;
                }

                _scrollViewer.ScrollToEnd();
                break;

            case Key.PageUp:
                MoveLeft(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: PageSize);

                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y - PageHeight);
                break;

            case Key.PageDown:
                MoveRight(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset: PageSize);

                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y + PageHeight);
                break;

            default:
                return;
        }

        RefreshSelection();
    }

    private void MoveLeft(bool isShiftPressed, int offset)
    {
        if (_cursorPosition == 0)
        {
            return;
        }

        _cursorPosition = _cursorPosition >= offset ? _cursorPosition - offset : 0;

        if (!IsMultiSelect || !isShiftPressed)
        {
            _selection.Start = _cursorPosition;
            _selection.End = _cursorPosition;
            _selectionAnchor = _cursorPosition;
        }
        else
        {
            if (_cursorPosition == _selection.End - offset && _selection.Length > 1)
            {
                _selection.End = _cursorPosition;
            }

            if (_cursorPosition < _selection.Start)
            {
                _selection.Start = _cursorPosition;
            }
        }
    }

    private void MoveRight(bool isShiftPressed, int offset)
    {
        _cursorPosition = _cursorPosition < Data.Length - offset ? _cursorPosition + offset : Data.Length - 1;

        if (!IsMultiSelect || !isShiftPressed)
        {
            _selection.Start = _cursorPosition;
            _selection.End = _cursorPosition;
            _selectionAnchor = _cursorPosition;
        }
        else
        {
            if (_cursorPosition == _selection.Start + offset && _selection.Length > 1)
            {
                _selection.Start = _cursorPosition;
            }

            if (_cursorPosition > _selection.End)
            {
                _selection.End = _cursorPosition;
            }
        }
    }


    private HexViewerRow CreateRow(int rowIndex)
    {
        var startOffset = rowIndex * BytesPerRow;
        var endOffset = startOffset + BytesPerRow;

        if (endOffset > Data.Length)
        {
            endOffset = Data.Length;
        }

        var row = new HexViewerRow
        {
            RowIndex = rowIndex,
            Offset = startOffset,
            Data = new ArraySegment<byte>(Data, startOffset, endOffset - startOffset),
            Height = RowHeight,
            Width = RowWidth,
            SelectedIndexes = GetRowSelectedIndexes(rowIndex).ToArray()
        };

        row.CellClicked += (_, e) => { HandleCellClicked(e); };

        return row;
    }

    private void HandleCellClicked(HexCellClickedEventArgs e)
    {
        var targetPosition = e.RowIndex * BytesPerRow + e.Position;

        if (!IsMultiSelect || !e.IsShiftPressed || _cursorPosition < 0)
        {
            _cursorPosition = targetPosition;
            _selection.Start = _cursorPosition;
            _selection.End = _cursorPosition;
            _selectionAnchor = _cursorPosition;
        }
        else
        {
            int selectionAnchor;

            if (_selection.Length > 0)
            {
                if (targetPosition < _selection.Start)
                {
                    selectionAnchor = _selection.End;
                }
                else if (targetPosition > _selection.End)
                {
                    selectionAnchor = _selection.Start;
                }
                else
                {
                    selectionAnchor = _selectionAnchor >= 0 ? _selectionAnchor : _cursorPosition;
                }
            }
            else
            {
                selectionAnchor = _selectionAnchor >= 0 ? _selectionAnchor : _cursorPosition;
            }

            _cursorPosition = targetPosition;
            _selection.Start = Math.Min(selectionAnchor, targetPosition);
            _selection.End = Math.Max(selectionAnchor, targetPosition);
        }

        RefreshSelection();
    }

    private IEnumerable<int> GetRowSelectedIndexes(int rowIndex)
    {
        foreach (var selectedIndex in SelectedIndexes)
        {
            if (selectedIndex / BytesPerRow == rowIndex)
            {
                yield return selectedIndex % BytesPerRow;
            }
        }
    }

    private void UpdateView()
    {
        var offset = _scrollViewer.Offset.Y;
        var viewportHeight = _scrollViewer.Viewport.Height;

        var startIndex = Math.Max(0, (int)(offset/ RowHeight));
        var endIndex = Math.Min(
            (Data.Length + BytesPerRow - 1) / BytesPerRow,
            (int)((offset + viewportHeight) / RowHeight) + 2);

        if (Data.Length == 0)
        {
            _hexPanel.Clear();
            return;
        }

        _hexPanel.RemoveNotVisibleRows(startIndex, endIndex);

        for (var rowIndex = startIndex; rowIndex < endIndex; rowIndex++)
        {
            if (_hexPanel.ContainsRow(rowIndex))
            {
                continue;
            }

            var row = CreateRow(rowIndex);
            _hexPanel.Add(row);
        }

        _hexPanel.InvalidateArrange();
    }

    private bool IsRowVisible(int rowIndex)
    {
        var rowTop = rowIndex * RowHeight;
        var rowBottom = rowTop + RowHeight;

        var viewportTop = _scrollViewer.Offset.Y;
        var viewportBottom = viewportTop + _scrollViewer.Viewport.Height;

        return rowTop >= viewportTop && rowBottom <= viewportBottom;
    }

    private void UpdateTypeface()
    {
        Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        var textLength = RowTextBuilder.CalculateTotalLength(IsOffsetVisible, GroupSize, BytesPerRow);
        var formattedText = HexViewerRow.CreateFormattedText("X", Typeface, FontSize, Foreground);

        RowWidth = textLength * formattedText.Width;
        RowTextBuilder = new RowTextBuilder(IsOffsetVisible, GroupSize, BytesPerRow, formattedText.Width);
    }

    private void Invalidate()
    {
        UpdateTypeface();

        _hexPanel.InvalidateVisual();
        _header.InvalidateVisual();
    }

    private void ScrollToRow(int rowIndex) => _scrollViewer.Offset =
        new Vector(_scrollViewer.Offset.X, rowIndex * RowHeight);

    private void RefreshSelection() => SelectedIndexes = Enumerable.Range(_selection.Start, _selection.Length).ToArray();
}
