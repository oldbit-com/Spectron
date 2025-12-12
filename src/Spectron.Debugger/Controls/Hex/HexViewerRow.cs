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

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<HexViewerRow, int>(nameof(SelectedIndex), -1);

    private static readonly StyledProperty<int> BytesPerRowProperty =
        Spectron.Debugger.Controls.Hex.HexViewer.BytesPerRowProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<double> FontSizeProperty =
        TemplatedControl.FontSizeProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<Typeface> TypefaceProperty =
        Spectron.Debugger.Controls.Hex.HexViewer.TypefaceProperty.AddOwner<HexViewerRow>();

    private static readonly StyledProperty<RowTextBuilder> RowTextBuilderProperty =
        Spectron.Debugger.Controls.Hex.HexViewer.RowTextBuilderProperty.AddOwner<HexViewerRow>();

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

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private int BytesPerRow => GetValue(BytesPerRowProperty);

    private double FontSize => GetValue(FontSizeProperty);

    private Typeface Typeface => GetValue(TypefaceProperty);

    private RowTextBuilder RowTextBuilder => GetValue(RowTextBuilderProperty);

    public required int RowIndex { get; init; }
    public required int Offset { get; init; }
    public required ArraySegment<byte> Data { get; init; }

    internal event EventHandler<HexCellSelectedEventArgs>? CellSelected;

    internal HexViewerRow()
    {
        HorizontalAlignment = HorizontalAlignment.Left;

        this.GetObservable(SelectedIndexProperty).Subscribe(_ => InvalidateVisual());
        this.GetObservable(TypefaceProperty).Subscribe(_ => InvalidateVisual());

        VerticalAlignment = VerticalAlignment.Center;
    }

    public override void Render(DrawingContext context)
    {
        var background = AlternatingRowBackground != null && RowIndex % 2 == 0 ? AlternatingRowBackground : null;
        var formattedText = GetFormattedText();
        var rect = new Rect(new Size(formattedText.Width, Bounds.Height));

        context.FillRectangle(background ?? Brushes.Transparent, rect);

        if (SelectedIndex != -1)
        {
            var layout = RowTextBuilder.GetLayout(SelectedIndex);
            rect = new Rect(layout.Position * RowTextBuilder.CharWidth - 4, 1, layout.Width * RowTextBuilder.CharWidth + 8, Height - 2);
            context.DrawRectangle(SelectedBackground, null, rect);

            layout = RowTextBuilder.GetLayout(SelectedIndex + BytesPerRow);
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
        var x = e.GetPosition(this).X;
        var cellIndex = RowTextBuilder?.GetIndexFromPosition(x);

        if (cellIndex != null)
        {
            CellSelected?.Invoke(this, new HexCellSelectedEventArgs(RowIndex, cellIndex.Value));
        }

        base.OnPointerPressed(e);
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
}