using System.Threading;
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

    private CancellationTokenSource _cancellationTokenSource = new();

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
            // Cancel the current animation and start a new one, this prevents overlapping animations.
            register._cancellationTokenSource.Cancel();
            register._cancellationTokenSource.Dispose();
            register._cancellationTokenSource = new CancellationTokenSource();

            animation.RunAsync(register.ValueControl, register._cancellationTokenSource.Token);
        }

        return value;
    }

    public Register() => InitializeComponent();
}