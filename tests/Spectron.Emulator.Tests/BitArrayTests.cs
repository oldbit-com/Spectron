using System.Collections;
using OldBit.Spectron.Emulation.Extensions;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests;

public class BitArrayTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(6565, true)]
    public void BitArray_ShouldSetBitValue(int value, bool expected)
    {
        var bitArray = new BitArray(8);

        bitArray.Set(1, value);

        bitArray.Get(1).ShouldBe(expected);
    }

    [Fact]
    public void BitArray_ShouldSetBitValues_1()
    {
        var bitArray = new BitArray(128);

        bitArray.Set(6, 0, 0b1101_0101);

        var bytes = bitArray.ToBytes();
        bytes[15].ShouldBe((byte)0b0101_0101);

        bytes[..15].ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void BitArray_ShouldSetBitValues_2()
    {
        var bitArray = new BitArray(128);

        bitArray.Set(127, 120, 0b1010101);

        var bytes = bitArray.ToBytes();
        bytes[0].ShouldBe((byte)0b1010101);

        bytes[1..].ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void BitArray_ShouldSetBitValues_3()
    {
        var bitArray = new BitArray(128);

        bitArray.Set(127, 127, 0b1010101);

        var bytes = bitArray.ToBytes();
        bytes[0].ShouldBe((byte)128);

        bytes[1..].ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void BitArray_ShouldSetBitValues_4()
    {
        var bitArray = new BitArray(128);

        bitArray.Set(127, 126, 0b1010111);

        var bytes = bitArray.ToBytes();
        bytes[0].ShouldBe((byte)192);

        bytes[1..].ShouldAllBe(x => x == 0);
    }

    [Fact]
    public void BitArray_ShouldSetBitValues_5()
    {
        var bitArray = new BitArray(128);

        bitArray.Set(8, 7, 0b11);
        bitArray.Set(0, 0, 1);
        bitArray.Set(10, 10, 1);

        var bytes = bitArray.ToBytes();

        bytes[14].ShouldBe((byte)0b0000_0101);
        bytes[15].ShouldBe((byte)0b1000_0001);
    }
}