using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulation.Computers;

internal record EmulatorSettings(
    Computer Computer,
    EmulatorMemory Memory,
    IContentionProvider ContentionProvider,
    Beeper Beeper,
    bool UseAYSound);