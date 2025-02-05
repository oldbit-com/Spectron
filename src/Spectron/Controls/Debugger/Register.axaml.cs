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

    public static readonly StyledProperty<int> ValueWidthProperty =
        AvaloniaProperty.Register<Register, int>(nameof(ValueWidth), defaultValue: 60);

    public static readonly StyledProperty<int> LabelWidthProperty =
        AvaloniaProperty.Register<Register, int>(nameof(LabelWidth), defaultValue: 20);

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

    public int ValueWidth
    {
        get => GetValue(ValueWidthProperty);
        set => SetValue(ValueWidthProperty, value);
    }

    public int LabelWidth
    {
        get => GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
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