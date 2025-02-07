using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using OldBit.Z80Cpu.Extensions;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectron.Debugger.Controls;

public partial class FlagsRegister : UserControl
{
    private const string FlagSet = "\u25cf";        // ●
    private const string FlagUnset = "\u00d7";      // ×

    private Animation? _animation;

    public static readonly StyledProperty<Flags> FlagsProperty =
        AvaloniaProperty.Register<FlagsRegister, Flags>(nameof(Flags), coerce: ValueChanged);

    public Flags Flags
    {
        get => GetValue(FlagsProperty);
        set => SetValue(FlagsProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (Resources["FlagAnimation"] is Animation animation)
        {
            _animation = animation;
        }
    }

    private static Flags ValueChanged(AvaloniaObject control, Flags value)
    {
        if (control is not FlagsRegister flagsControl)
        {
            return value;
        }

        UpdateFlag(flagsControl, flagsControl.SignFlag, value, Flags.S);
        UpdateFlag(flagsControl, flagsControl.ZeroFlag, value, Flags.Z);
        UpdateFlag(flagsControl, flagsControl.YFlag, value, Flags.Y);
        UpdateFlag(flagsControl, flagsControl.HalfCarryFlag, value, Flags.H);
        UpdateFlag(flagsControl, flagsControl.XFlag, value, Flags.X);
        UpdateFlag(flagsControl, flagsControl.ParityFlag, value, Flags.P);
        UpdateFlag(flagsControl, flagsControl.AddSubtractFlag, value, Flags.N);
        UpdateFlag(flagsControl, flagsControl.CarryFlag, value, Flags.C);

        return value;
    }

    private static void UpdateFlag(FlagsRegister control, TextBlock textBlock, Flags value, Flags flag)
    {
        var text = value.IsSet(flag) ? FlagSet : FlagUnset;

        if (textBlock.Text == text)
        {
            return;
        }

        textBlock.Text = text;

        control._animation?.RunAsync(textBlock);
    }

    public FlagsRegister() => InitializeComponent();
}