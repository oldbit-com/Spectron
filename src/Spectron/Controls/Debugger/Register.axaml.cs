using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace OldBit.Spectron.Controls.Debugger;

public partial class Register : UserControl
{
    public static readonly StyledProperty<string> RegisterNameProperty =
        AvaloniaProperty.Register<Register, string>(nameof(RegisterName));

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<Register, string>(nameof(Value), coerce: ValueChanged);

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
        if (control is Register register &&
            register.Resources["RegisterAnimation"] is Animation animation)
        {
            animation.RunAsync(register.ValueControl);
        }

        return value;
    }

    public Register() => InitializeComponent();
}