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
        AvaloniaProperty.Register<HexViewer, int>(nameof(RowHeight), 20);

    public static readonly StyledProperty<int> BytesPerRowProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(BytesPerRow), 16, inherits: true);

    public static readonly StyledProperty<bool> IsOffsetVisibleProperty =
        AvaloniaProperty.Register<HexViewer, bool>(nameof(IsOffsetVisible), true, inherits: true);

    public static readonly StyledProperty<bool> IsHeaderVisibleProperty =
        AvaloniaProperty.Register<HexViewer, bool>(nameof(IsHeaderVisible), true);

    public static readonly StyledProperty<int> GroupSizeProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(GroupSize), 8);

    public static readonly DirectProperty<HexViewer, byte[]> DataProperty =
        AvaloniaProperty.RegisterDirect<HexViewer, byte[]>(
            nameof(Data),
            getter: o => o.Data,
            setter: (o, v) => o.Data = v,
            unsetValue: []);

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<HexViewer, int>(nameof(SelectedIndex), -1);

    internal static readonly StyledProperty<Typeface> TypefaceProperty =
        AvaloniaProperty.Register<HexViewer, Typeface>(nameof(Typeface), inherits: true);

    internal static readonly StyledProperty<RowTextBuilder> RowTextBuilderProperty =
        AvaloniaProperty.Register<HexViewer, RowTextBuilder>(nameof(RowTextBuilder), inherits: true);

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

                SelectedIndex = -1;
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

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private readonly HexViewerHeader _header;
    private readonly ScrollViewer _scrollViewer;
    private readonly HexViewerPanel _hexPanel;

    private int CurrentRowIndex => SelectedIndex / BytesPerRow;
    private int VisibleRowCount => (int)(_scrollViewer.Viewport.Height / RowHeight);
    private int PageHeight => VisibleRowCount * RowHeight;

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
        this.GetObservable(SelectedIndexProperty).Subscribe(_ => UpdateSelected());

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                UpdateSelectedIndex(-1);
                break;

            case Key.Right:
                UpdateSelectedIndex(1);
                break;

            case Key.Up:
                UpdateSelectedIndex(-BytesPerRow);

                if (!IsRowVisible(CurrentRowIndex))
                {
                    _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, CurrentRowIndex * RowHeight);
                }
                break;

            case Key.Down:
                UpdateSelectedIndex(BytesPerRow);

                if (!IsRowVisible(CurrentRowIndex))
                {
                    var viewportRows = (int)(_scrollViewer.Viewport.Height / RowHeight);
                    var targetTopRow = CurrentRowIndex - Math.Max(0, viewportRows) + 1;

                    _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, targetTopRow * RowHeight);
                }
                break;

            case Key.Home:
                SelectedIndex = 0;

                _scrollViewer.ScrollToHome();
                break;

            case Key.End:
                SelectedIndex = Data.Length - 1;

                _scrollViewer.ScrollToEnd();
                break;

            case Key.PageUp:
                UpdateSelectedIndex(-VisibleRowCount * BytesPerRow);

                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y - PageHeight);
                break;

            case Key.PageDown:
                UpdateSelectedIndex(VisibleRowCount * BytesPerRow);

                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y + PageHeight);
                break;
        }

        base.OnKeyDown(e);
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
            SelectedIndex = CurrentRowIndex == rowIndex ? SelectedIndex % BytesPerRow : -1
        };

        row.CellSelected += (_, e) => SelectedIndex = e.RowIndex * BytesPerRow + e.Position;

        return row;
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

    private void UpdateSelected() => _hexPanel.UpdateSelected(
        SelectedIndex == -1 ? -1 : CurrentRowIndex,
        SelectedIndex == -1 ? -1 : SelectedIndex % BytesPerRow);

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

    private void UpdateSelectedIndex(int amount)
    {
        SelectedIndex += amount;

        if (SelectedIndex < 0)
        {
            SelectedIndex = 0;
        }
        else if (SelectedIndex > Data.Length - 1)
        {
            SelectedIndex = Data.Length - 1;
        }
    }
}