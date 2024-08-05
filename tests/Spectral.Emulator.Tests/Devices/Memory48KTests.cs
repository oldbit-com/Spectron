using FluentAssertions;
using OldBit.Spectral.Emulation.Devices;
using OldBit.ZXSpectrum.Emulator.Tests.Fixtures;

namespace OldBit.ZXSpectrum.Emulator.Tests.Devices;

public class Memory48KTests
{
    [Fact]
    public void Rom_ShouldBePopulated()
    {
        var random = new Random(15001900);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory48K(rom);

        memory.ReadRom().Should().BeEquivalentTo(rom);
    }

    [Fact]
    public void Rom_ShouldBeReadOnly()
    {
        var memory = new Memory48K(new byte[16384]);

        for (var address = 0; address < 0x4000; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        memory.ReadRom().Should().AllSatisfy(x => x.Should().Be(0));
    }

    [Fact]
    public void Ram_ShouldBeWritable()
    {
        var memory = new Memory48K(new byte[16384]);

        for (var address = 0x4000; address <= 0xFFFF; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        memory.ReadRange(0x4000, 0xC000).Should().AllSatisfy(x => x.Should().Be(0xFF));
    }
}