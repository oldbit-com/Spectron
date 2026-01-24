using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace OldBit.Spectron.Debugger.Controls.Hex;

public sealed class HexViewerHeader : Control
{
    internal static readonly StyledProperty<IBrush?> ForegroundProperty =
        TemplatedControl.ForegroundProperty.AddOwner<HexViewerHeader>();

    private static readonly StyledProperty<double> FontSizeProperty =
        TemplatedControl.FontSizeProperty.AddOwner<HexViewerHeader>();

    private static readonly StyledProperty<Typeface> TypefaceProperty =
        HexViewer.TypefaceProperty.AddOwner<HexViewerHeader>();

    private static readonly StyledProperty<RowTextBuilder> RowTextBuilderProperty =
        HexViewer.RowTextBuilderProperty.AddOwner<HexViewerHeader>();

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    private double FontSize => GetValue(FontSizeProperty);

    private Typeface Typeface => GetValue(TypefaceProperty);

    private RowTextBuilder RowTextBuilder => GetValue(RowTextBuilderProperty);

    public override void Render(DrawingContext context)
    {
        var formattedText = GetFormattedText();
        var rect = new Rect(new Size(formattedText.Width, Bounds.Height));

        context.FillRectangle(Brushes.Transparent, rect);

        var y = (Bounds.Height - formattedText.Height) / 2;
        context.DrawText(formattedText, new Point(0, y));
    }

    private FormattedText GetFormattedText()
    {
        var text = RowTextBuilder.BuildHeader();
        var formattedText = HexViewerRow.CreateFormattedText(text, Typeface, FontSize, Foreground);

        return formattedText;
    }
}