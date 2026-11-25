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
    [Fact]
    public void InstantSave_WhenSpectrumRomActive_ShouldTrapSaveBytes()
    {
        var emulator = CreateEmulator(ComputerType.Spectrum48K, RomType.Original);
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.Cpu.Registers.PC.ShouldBeInRange((Word)(RomRoutines.SA_DELAY - 2), RomRoutines.SA_BYTES_RET);
        emulator.TapeManager.Cassette.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void InstantSave_WhenShadowRomActive_ShouldNotTrapSaveBytes()
    {
        var emulator = CreateEmulator(ComputerType.Spectrum48K, RomType.Original);

        // Shadow ROM filled with NOPs, e.g. DivMMC esxDOS code located at the same address
        emulator.Memory.ShadowRom(new RomMemory(new byte[0x4000]));
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.Cpu.Registers.PC.ShouldBe((Word)(RomRoutines.SA_BYTES + 1));
        emulator.TapeManager.Cassette.IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ComputerType.Spectrum16K, RomType.Original)]
    [InlineData(ComputerType.Timex2048, RomType.Original)]
    [InlineData(ComputerType.Spectrum48K, RomType.BusySoft)]
    [InlineData(ComputerType.Spectrum48K, RomType.GoshWonderful)]
    [InlineData(ComputerType.Spectrum48K, RomType.PrettyBasic)]
    public void InstantSave_WhenRomHasOriginalTapeRoutines_ShouldTrapSaveBytes(ComputerType computerType, RomType romType)
    {
        var emulator = CreateEmulator(computerType, romType);
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.TapeManager.Cassette.IsEmpty.ShouldBeFalse();
    }

    [Theory]
    [InlineData(ComputerType.Spectrum48K, RomType.BbcBasic)]
    [InlineData(ComputerType.Spectrum48K, RomType.Retroleum)]
    [InlineData(ComputerType.Spectrum48K, RomType.BrendanAlford)]
    [InlineData(ComputerType.Spectrum48K, RomType.Harston)]
    [InlineData(ComputerType.Spectrum48K, RomType.HtrSuperBasic)]
    public void InstantSave_WhenRomHasDifferentTapeRoutines_ShouldNotTrapSaveBytes(ComputerType computerType, RomType romType)
    {
        var emulator = CreateEmulator(computerType, romType);
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.TapeManager.Cassette.IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RomType.Original)]
    [InlineData(RomType.Pentagon128)]
    public void InstantSave_When128KRomBank0Active_ShouldNotTrapSaveBytes(RomType romType)
    {
        var emulator = CreateEmulator(ComputerType.Spectrum128K, romType);
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.TapeManager.Cassette.IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RomType.Original)]
    [InlineData(RomType.Pentagon128)]
    [InlineData(RomType.Retroleum)]
    public void InstantSave_When128KRomBank1Active_ShouldTrapSaveBytes(RomType romType)
    {
        var emulator = CreateEmulator(ComputerType.Spectrum128K, romType);
        ((Memory128K)emulator.Memory).SetPagingMode(0x10);
        emulator.Cpu.Registers.PC = RomRoutines.SA_BYTES;

        emulator.Cpu.Run(4);

        emulator.TapeManager.Cassette.IsEmpty.ShouldBeFalse();
    }

    private static Emulation.Emulator CreateEmulator(ComputerType computerType, RomType romType)
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        var emulator = provider.GetRequiredService<EmulatorFactory>().Create(computerType, romType);
        emulator.TapeManager.TapeSaveSpeed = TapeSpeed.Instant;
        emulator.TapeManager.IsTapeSaveEnabled = true;

        return emulator;
    }
}
