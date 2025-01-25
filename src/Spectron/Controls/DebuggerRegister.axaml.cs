using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace OldBit.Spectron.Controls;

public partial class DebuggerRegister : UserControl
{
    public static readonly StyledProperty<string> RegisterNameProperty =
        AvaloniaProperty.Register<DebuggerRegister, string>(nameof(RegisterName));

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<DebuggerRegister, string>(nameof(Value), coerce: ValueChanged);

    public string RegisterName
    {
        get => GetValue(RegisterNameProperty);
        set => SetValue(RegisterNameProperty, value);
    }

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static string ValueChanged(AvaloniaObject control, string value)
    {
        if (control is DebuggerRegister register &&
            register.Resources["RegisterAnimation"] is Animation animation)
        {
            animation.RunAsync(register.ValueControl);
        }

        return value;
    }

    public DebuggerRegister() => InitializeComponent();
}