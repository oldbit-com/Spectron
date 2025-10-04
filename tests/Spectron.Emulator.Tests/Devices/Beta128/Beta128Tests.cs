using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128;

public class Beta128Tests
{
    private readonly Memory48K _memory;
    private readonly Z80 _z80;
    private readonly Beta128Device _beta128;
    private readonly byte[] _beta128Rom = RomReader.ReadRom(RomType.TrDos);
    private readonly byte[] _spectrumRom = RomReader.ReadRom(RomType.Original48);

    public Beta128Tests()
    {
        _memory = new Memory48K(_spectrumRom);
        _z80 = new Z80(_memory);

        _beta128 = new Beta128Device(_z80, 3.5f, _memory, ComputerType.Spectrum48K);
    }

    [Fact]
    public void Enable_ShouldEnableBet128()
    {
        _beta128.Enable();

        _beta128.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Disable_ShouldDisableBet128()
    {
        _beta128.Enable();
        _beta128.Disable();

        _beta128.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void ShadowRom_ShouldActivateAndDeactivate()
    {
        _beta128.Enable();

        _z80.Registers.PC = 0x3D00;
        _z80.Step();

        _memory.Rom.ToArray().ShouldBe(_beta128Rom);

        _z80.Registers.PC = 0x4D00;
        _z80.Step();

        _memory.Rom.ToArray().ShouldBe(_spectrumRom);
    }

    [Fact]
    public void ShadowRom_ShouldNotActivateIfBeta128IsNotEnabled()
    {
        _z80.Registers.PC = 0x3D00;
        _z80.Step();

        _memory.Rom.ToArray().ShouldBe(_spectrumRom);
    }
}