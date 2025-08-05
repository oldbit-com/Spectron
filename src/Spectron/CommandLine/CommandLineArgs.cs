using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.CommandLine;

public record CommandLineArgs(
    string? FilePath,
    TapeSpeed? TapeLoadSpeed,
    ComputerType? ComputerType,
    RomType? RomType,
    string[]? CustomRomFiles,
    JoystickType? JoystickType,
    MouseType? MouseType,
    bool IsAudioMuted,
    bool? IsAyEnabled,
    StereoMode? AyStereoMode,
    bool? IsDivMmcEnabled,
    string? DivMmcImageFile,
    bool? IsDivMmcReadOnly,
    bool? IsZxPrinterEnabled,
    bool? IsUlaPlusEnabled,
    bool? IsTimeMachineEnabled,
    bool? IsResumeEnabled,
    BorderSize? BorderSize,
    Theme? Theme);