using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.Settings;

public class Preferences
{
    public Theme Theme { get; init; } = Theme.Dark;

    public ResumeSettings Resume { get; init; } = new();

    public bool IsUlaPlusEnabled { get; init; }

    public bool IsFloatingBusEnabled { get; init; }

    public bool IsAutoLoadPokeFilesEnabled { get; init; } = true;

    public BorderSize BorderSize { get; set; } = BorderSize.Medium;

    public ComputerType ComputerType { get; init; } = ComputerType.Spectrum48K;

    public RomType RomType { get; init; } = RomType.Original;

    public KeyboardSettings Keyboard { get; init; } = new();

    public JoystickSettings Joystick { get; init; } = new();

    public MouseSettings Mouse { get; init; } = new();

    public TimeMachineSettings TimeMachine { get; init; } = new();

    public AudioSettings Audio { get; init; } = new();

    public TapeSettings Tape { get; init; } = new();

    public RecordingSettings Recording { get; init; } = new();

    public DebuggerSettings Debugger { get; init; } = new();

    public DivMmcSettings DivMmc { get; init; } = new();

    public PrinterSettings Printer { get; init; } = new();

    public Interface1Settings Interface1 { get; init; } = new();
}