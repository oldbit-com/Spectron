using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace OldBit.Spectron.Controls;

public partial class PaletteCell : UserControl
{
    public static readonly StyledProperty<string?> HexProperty =
        AvaloniaProperty.Register<PaletteCell, string?>(nameof(Hex), defaultBindingMode: BindingMode.TwoWay);

    public string? Hex
    {
        get => GetValue(HexProperty);
        set => SetValue(HexProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<PaletteCell, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public PaletteCell() => InitializeComponent();
}
