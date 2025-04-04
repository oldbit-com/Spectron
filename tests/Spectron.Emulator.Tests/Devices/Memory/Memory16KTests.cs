using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulator.Tests.Fixtures;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Memory;

public class Memory16KTests
{
    [Fact]
    public void Rom_ShouldBePopulated()
    {
        var random = new Random(15001900);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory16K(rom);

        memory.Rom.ToArray().ShouldBeEquivalentTo(rom);
    }

    [Fact]
    public void Rom_ShouldBeReadOnly()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0; address < 0x4000; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        memory.Rom.ToArray().ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void RamBelow32K_ShouldBeWritable()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0x4000; address < 0x8000; address++)
        {
            memory.Write((Word)address, 0x55);
        }

        var data = memory.ReadRange(0x4000, 0x4000);

        data.ShouldAllBe(x => x == 0x55);
        memory.Ram.ToArray().ShouldBeEquivalentTo(data);
    }

    [Fact]
    public void RamAbove32K_ShouldNotBeWritable()
    {
        var memory = new Memory16K(new byte[16384]);

        for (var address = 0x8000; address <= 0xFFFF; address++)
        {
            var value = memory.Read((Word)address);
            value.ShouldBe((byte)0xFF);

            memory.Write((Word)address, 0xAA);

            value = memory.Read((Word)address);
            value.ShouldBe((byte)0xFF);
        }
    }
}