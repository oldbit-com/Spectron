using OldBit.Spectron.Emulation.Devices.Beta128;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128;

public class CrcTests
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void Crc_ShouldBeCalculated(byte[] data, Word initial, Word expected)
    {
        var crc = Crc.Calculate(data, initial);

        crc.ShouldBe(expected);
    }

    public static TheoryData<byte[], Word, Word> TestData => new()
    {
        { [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 0xCDB4, 0x0401 },
        { [255, 0, 120, 4], 0xCDB4, 0x22CA },
        { [0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39], 0xFFFF, 0x29B1 }
    };
}