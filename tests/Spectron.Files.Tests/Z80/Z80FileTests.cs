using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Files.Tests.Z80;

public class Z80FileTests
{
    [Fact]
    public void Memory48_ShouldHaveCorrectBlocsAndValues()
    {
        var memory = new byte[0xC000];

        Array.Fill<byte>(memory, 8, 0x0000, 0x4000);
        Array.Fill<byte>(memory, 4, 0x4000, 0x4000);
        Array.Fill<byte>(memory, 5, 0x8000, 0x4000);

        var header = new Z80Header
        {
            HardwareMode = HardwareMode.Spectrum48,
            PC = 0xE000,
        };

        var snapshot = new Z80File(header, memory);

        snapshot.MemoryBlocks.Count.ShouldBe(3);

        snapshot.MemoryBlocks[0].PageNumber.ShouldBe(4);
        snapshot.MemoryBlocks[0].Data.Length.ShouldBe(0x4000);
        snapshot.MemoryBlocks[0].Data.ShouldAllBe(b => b == 4);

        snapshot.MemoryBlocks[1].PageNumber.ShouldBe(5);
        snapshot.MemoryBlocks[1].Data.Length.ShouldBe(0x4000);
        snapshot.MemoryBlocks[1].Data.ShouldAllBe(b => b == 5);

        snapshot.MemoryBlocks[2].PageNumber.ShouldBe(8);
        snapshot.MemoryBlocks[2].Data.Length.ShouldBe(0x4000);
        snapshot.MemoryBlocks[2].Data.ShouldAllBe(b => b == 8);

    }
}