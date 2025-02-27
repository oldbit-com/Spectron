using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.Settings;

public class Preferences
{
    public Theme Theme { get; init; } = Theme.Dark;

    public ResumeSettings ResumeSettings { get; init; } = new();

    public bool IsUlaPlusEnabled { get; init; }

    public bool IsFloatingBusEnabled { get; init; }

    public BorderSize BorderSize { get; init; } = BorderSize.Medium;

    public ComputerType ComputerType { get; init; } = ComputerType.Spectrum48K;

    public RomType RomType { get; init; } = RomType.Original;

    public JoystickSettings Joystick { get; init; } = new();

    public int MaxRecentFiles { get; set; } = 10;

    public TimeMachineSettings TimeMachine { get; init; } = new();

    public AudioSettings AudioSettings { get; init; } = new();

    public TapeSettings TapeSettings { get; init; } = new();

    public RecordingSettings RecordingSettings { get; init; } = new();
}