using Avalonia;
using Avalonia.Controls.Primitives;

namespace OldBit.Spectron.Controls;

public class HeaderSeparator : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<HeaderSeparator, string>(nameof(HeaderSeparator), defaultValue: string.Empty);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}