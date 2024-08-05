using FluentAssertions;
using OldBit.Spectral.Emulation.Devices;
using OldBit.ZXSpectrum.Emulator.Tests.Fixtures;

namespace OldBit.ZXSpectrum.Emulator.Tests.Devices;

public class Memory16KTests
{
    [Fact]
    public void Rom_ShouldBePopulated()
    {
        var random = new Random(15001900);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory16K(rom);

        memory.ReadRom().Should().BeEquivalentTo(rom);
    }

    [Fact]
    public void Rom_ShouldBeReadOnly()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0; address < 0x4000; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        memory.ReadRom().Should().AllSatisfy(x => x.Should().Be(0));
    }

    [Fact]
    public void RamBelow32K_ShouldBeWritable()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0x4000; address < 0x8000; address++)
        {
            memory.Write((Word)address, 0x55);
        }

        memory.ReadRange(0x4000, 0x4000).Should().AllSatisfy(x => x.Should().Be(0x55));
    }

    [Fact]
    public void RamAbove32K_ShouldNotBeWritable()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0x8000; address <= 0xFFFF; address++)
        {
            var value = memory.Read((Word)address);
            value.Should().Be(0xFF);

            memory.Write((Word)address, 0xAA);

            value = memory.Read((Word)address);
            value.Should().Be(0xFF);
        }
    }
}