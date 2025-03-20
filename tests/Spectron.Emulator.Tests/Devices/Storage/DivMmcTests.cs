using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Storage;

public class DivMmcTests
{
    [Theory]
    [InlineData(0x0000)]
    [InlineData(0x0008)]
    [InlineData(0x0038)]
    [InlineData(0x0066)]
    [InlineData(0x04C6)]
    [InlineData(0x0562)]
    [InlineData(0x3D00)]
    [InlineData(0x3DFF)]
    public void WhenSpecificAddressFetch_ShouldAutoMap(Word address)
    {
        var emulatorMemory = new Memory48K(RomReader.ReadRom(RomType.Original48));
        var cpu = new Z80(emulatorMemory)
        {
            Registers =
            {
                PC = address
            }
        };

        cpu.Step();

        var divMmc = new DivMmc(cpu, emulatorMemory);
        //divMmc.Memory.PageMemory();

        // TODO: Complete the test
    }
}