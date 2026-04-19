using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulator.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void Concatenate_NonEmptyArrays_ReturnsCombined()
    {
        var first = new byte[] { 1, 2, 3 };
        var second = new byte[] { 4, 5, 6 };
        var result = first.Concatenate(second);

        result.Length.ShouldBe(6);
        result[0].ShouldBe(1);
        result[1].ShouldBe(2);
        result[2].ShouldBe(3);
        result[3].ShouldBe(4);
        result[4].ShouldBe(5);
        result[5].ShouldBe(6);
    }

    [Fact]
    public void Concatenate_EmptyWithEmpty_ReturnsEmpty()
    {
        var first = Array.Empty<byte>();
        var second = Array.Empty<byte>();
        var result = first.Concatenate(second);

        result.Length.ShouldBe(0);
    }

    [Fact]
    public void Concatenate_EmptyFirst_WithSecond_ReturnsSecond()
    {
        var second = new byte[] { 1, 2 };
        var first = Array.Empty<byte>();
        var result = first.Concatenate(second);

        result.Length.ShouldBe(2);
        result[0].ShouldBe(1);
        result[1].ShouldBe(2);
    }

    [Fact]
    public void Concatenate_EmptySecond_WithFirst_ReturnsFirst()
    {
        var first = new byte[] { 1, 2 };
        var second = Array.Empty<byte>();
        var result = first.Concatenate(second);

        result.Length.ShouldBe(2);
        result[0].ShouldBe(1);
        result[1].ShouldBe(2);
    }

    [Fact]
    public void Concatenate_NonEmptyArrays_ReturnsCombinedInts()
    {
        var first = new[] { 11, 22, 33 };
        var second = new[] { 44 };
        var result = first.Concatenate(second);

        result.Length.ShouldBe(4);
        result[0].ShouldBe(11);
        result[1].ShouldBe(22);
        result[2].ShouldBe(33);
        result[3].ShouldBe(44);
    }

    [Fact]
    public void Concatenate_StringArrays_ReturnsCombinedStrings()
    {
        var first = new[] { "hello", "world" };
        var second = new[] { "!" };
        var result = first.Concatenate(second);

        result.Length.ShouldBe(3);
        result[0].ShouldBe("hello");
        result[1].ShouldBe("world");
        result[2].ShouldBe("!");
    }
}
