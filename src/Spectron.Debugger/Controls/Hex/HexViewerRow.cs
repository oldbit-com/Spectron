using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace OldBit.Spectron.Debugger.Controls.Hex;

public sealed class HexViewerRow : Control
{
    public static readonly StyledProperty<IBrush?> AlternatingRowBackgroundProperty =
        AvaloniaProperty.Register<HexViewerRow, IBrush?>(nameof(AlternatingRowBackground));

    public static readonly StyledProperty<IBrush?> SelectedBackgroundProperty =
        AvaloniaProperty.Register<HexViewerRow, IBrush?>(nameof(SelectedBackground));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TemplatedControl.ForegroundProperty.AddOwner<HexViewerRow>();

    public static readonly StyledProperty<IBrush?> OffsetForegroundProperty =
        AvaloniaProperty.Register<HexViewerRow, IBrush?>(nameof(OffsetForeground));

    private static readonly StyledProperty<int> BytesPerRowProperty =
        HexViewer.BytesPerRowProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<double> FontSizeProperty =
        TemplatedControl.FontSizeProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<Typeface> TypefaceProperty =
        HexViewer.TypefaceProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<RowTextBuilder> RowTextBuilderProperty =
        HexViewer.RowTextBuilderProperty.AddOwner<HexViewerRow>();

    public static readonly StyledProperty<Selection> SelectionProperty =
        HexViewer.SelectionProperty.AddOwner<HexViewerRow>();

    public IBrush? AlternatingRowBackground
    {
        get => GetValue(AlternatingRowBackgroundProperty);
        set => SetValue(AlternatingRowBackgroundProperty, value);
    }

    public IBrush? SelectedBackground
    {
        get => GetValue(SelectedBackgroundProperty);
        set => SetValue(SelectedBackgroundProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? OffsetForeground
    {
        get => GetValue(OffsetForegroundProperty);
        set => SetValue(OffsetForegroundProperty, value);
    }

    public Selection Selection => GetValue(SelectionProperty);

    private int BytesPerRow => GetValue(BytesPerRowProperty);

    private double FontSize => GetValue(FontSizeProperty);

    private Typeface Typeface => GetValue(TypefaceProperty);

    private RowTextBuilder RowTextBuilder => GetValue(RowTextBuilderProperty);

    private int RowStartIndex => RowIndex * BytesPerRow;
    private int RowEndIndex => (RowIndex + 1) * BytesPerRow - 1;

    public required int RowIndex { get; init; }
    public required int Offset { get; init; }
    public required ArraySegment<byte> Data { get; init; }

    internal event EventHandler<HexCellClickedEventArgs>? CellClicked;

    internal HexViewerRow()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;

        this.GetObservable(TypefaceProperty).Subscribe(_ => InvalidateVisual());
        this.GetObservable(SelectionProperty).Subscribe(_ => InvalidateVisual());
    }

    public override void Render(DrawingContext context)
    {
        var background = AlternatingRowBackground != null && RowIndex % 2 == 0 ? AlternatingRowBackground : null;
        var formattedText = GetFormattedText();
        var rect = new Rect(new Size(formattedText.Width, Bounds.Height));

        context.FillRectangle(background ?? Brushes.Transparent, rect);

        foreach (var selectedIndex in GetSelectedIndexes())
        {
            var layout = RowTextBuilder.GetLayout(selectedIndex);
            rect = new Rect(layout.Position * RowTextBuilder.CharWidth - 4, 1, layout.Width * RowTextBuilder.CharWidth + 8, Height - 2);
            context.DrawRectangle(SelectedBackground, null, rect);

            layout = RowTextBuilder.GetLayout(selectedIndex + BytesPerRow);
            rect = new Rect(layout.Position * RowTextBuilder.CharWidth, 1, layout.Width * RowTextBuilder.CharWidth, Height - 2);
            context.DrawRectangle(SelectedBackground, null, rect);
        }

        if (RowTextBuilder.IsOffsetVisible)
        {
            formattedText.SetForegroundBrush(OffsetForeground, 0, 5);
        }

        var y = (Bounds.Height - formattedText.Height) / 2;
        context.DrawText(formattedText, new Point(0, y));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var x = e.GetPosition(this).X;
        var cellIndex = RowTextBuilder.GetIndexFromPosition(x);

        if (cellIndex == null)
        {
            return;
        }

        var isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        CellClicked?.Invoke(this, new HexCellClickedEventArgs(RowIndex, cellIndex.Value, isShiftPressed));
    }

    internal static FormattedText CreateFormattedText(string text, Typeface typeface, double fontSize, IBrush? foreground) => new(
        text,
        CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        typeface,
        fontSize,
        foreground);

    private FormattedText GetFormattedText()
    {
        var text = RowTextBuilder.Build(Data, Offset);

        return CreateFormattedText(text, Typeface, FontSize, Foreground);
    }

    private IEnumerable<int> GetSelectedIndexes()
    {
        for (var i = Selection.Start; i <= Selection.End; i++)
        {
            if (i >= RowStartIndex && i <= RowEndIndex)
            {
                yield return i - RowStartIndex;
            }
        }
    }
}
