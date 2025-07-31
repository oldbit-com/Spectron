using OldBit.Spectron.Emulation;
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
    bool IsAudioMuted,
    bool? IsAyEnabled,
    BorderSize? BorderSize,
    Theme? Theme);