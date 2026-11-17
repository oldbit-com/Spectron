using Avalonia;
using Avalonia.Controls;

namespace OldBit.Spectron.Controls;

public partial class PaletteCell : UserControl
{
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<PaletteCell, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public PaletteCell() => InitializeComponent();
}
