using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulator.Tests.Fixtures;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Memory;

public class Memory48KTests
{
    [Fact]
    public void Rom_ShouldBePopulated()
    {
        var random = new Random(15001900);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory48K(rom);

        memory.Rom.ToArray().ShouldBeEquivalentTo(rom);
    }

    [Fact]
    public void Rom_ShouldBeReadOnly()
    {
        var memory = new Memory48K(new byte[16384]);

        for (var address = 0; address < 0x4000; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        memory.Rom.ToArray().ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void Ram_ShouldBeWritable()
    {
        var memory = new Memory48K(new byte[16384]);

        for (var address = 0x4000; address <= 0xFFFF; address++)
        {
            memory.Write((Word)address, 0xFF);
        }

        var data = memory.ReadRange(0x4000, 0xC000);

        data.ShouldAllBe(x => x == 0xFF);
        memory.Ram.ToArray().ShouldBeEquivalentTo(data);
    }
}