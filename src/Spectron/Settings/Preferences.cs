using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;

namespace OldBit.Spectron.Settings;

public record TapeSavingSettings(bool IsEnabled, TapeSpeed Speed);

public class Preferences
{
    public bool IsUlaPlusEnabled { get; init; }

    public BorderSize BorderSize { get; init; } = BorderSize.Medium;

    public ComputerType ComputerType { get; init; } = ComputerType.Spectrum48K;

    public RomType RomType { get; init; } = RomType.Original;

    public JoystickSettings Joystick { get; init; } = new();

    public int MaxRecentFiles { get; set; } = 10;

    public TapeSpeed TapeLoadSpeed { get; init; } = TapeSpeed.Instant;

    public TimeMachineSettings TimeMachine { get; init; } = new();

    public bool IsResumeEnabled { get; init; } = true;

    public TapeSavingSettings TapeSaving { get; init; } = new(true, TapeSpeed.Instant);
}