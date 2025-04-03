using OldBit.Spectron.Emulation.Utilities;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests;

public class BitVectorTests
{
    [Fact]
    public void BitVector_ShouldSetBitsCorrectly_1()
    {
        var bitVector = new BitVector(9);

        bitVector.Set(8, 1, 0b10000001);

        var result = bitVector.ToArray();

        result[0].ShouldBe((byte)0b0000_0001);
        result[1].ShouldBe((byte)0b0000_0010);
    }

    [Fact]
    public void BitVector_ShouldSetBitsCorrectly_2()
    {
        var bitVector = new BitVector(9);

        bitVector.Set(1, 8, 0b10000001);

        var result = bitVector.ToArray();

        result[0].ShouldBe((byte)0b0000_0001);
        result[1].ShouldBe((byte)0b0000_0010);
    }

    [Fact]
    public void BitVector_ShouldSetBitsCorrectly_3()
    {
        var bitVector = new BitVector(128);

        bitVector.Set(127, 127, 1);
        bitVector.Set(120, 119, 3);
        bitVector.Set(105, 102, 0b1101);
        bitVector.Set(15, 1, 0xFF);

        var result = bitVector.ToArray();

        result[0].ShouldBe((byte)0b1000_0001);
        result[1].ShouldBe((byte)0b1000_0000);
        result[2].ShouldBe((byte)0b0000_0011);
        result[3].ShouldBe((byte)0b0100_0000);
        result[14].ShouldBe((byte)0x01);
        result[15].ShouldBe((byte)0xFE);
    }
}