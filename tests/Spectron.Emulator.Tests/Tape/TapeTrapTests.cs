using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Emulator.Tests.Tape;

public class TapeTrapTests
{
    private readonly Emulation.Emulator _emulator;

    public TapeTrapTests()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        _emulator = provider.GetRequiredService<EmulatorFactory>().Create(ComputerType.Spectrum48K, RomType.Original);
        _emulator.TapeManager.TapeSaveSpeed = TapeSpeed.Instant;
        _emulator.TapeManager.IsTapeSaveEnabled = true;
    }

    [Fact]
    public void InstantSave_WhenSpectrumRomActive_ShouldTrapSaveBytes()
    {
        _emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        _emulator.Cpu.Run(4);

        _emulator.Cpu.Registers.PC.ShouldBeInRange((Word)(RomRoutines.SA_DELAY - 2), RomRoutines.SA_BYTES_RET);
    }

    [Fact]
    public void InstantSave_WhenShadowRomActive_ShouldNotTrapSaveBytes()
    {
        // Shadow ROM filled with NOPs, e.g. DivMMC esxDOS code located at the same address
        _emulator.Memory.ShadowRom(new RomMemory(new byte[0x4000]));
        _emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        _emulator.Cpu.Run(4);

        _emulator.Cpu.Registers.PC.ShouldBe((Word)(RomRoutines.SA_BYTES + 1));
    }
}
