using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulation.Computers;

internal record EmulatorSettings(
    ComputerType ComputerType,
    EmulatorMemory Memory,
    IContentionProvider ContentionProvider,
    Beeper Beeper,
    bool UseAYSound);