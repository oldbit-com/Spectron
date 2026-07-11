namespace OldBit.Spectron.Files.Tests.Extensions;

public static class ShouldlyExtensions
{
    public static void ShouldBe(this Word actual, int expected, string? message = null) =>
        actual.ShouldBe<Word>((Word)expected, message);

    public static void ShouldBe(this byte actual, int expected, string? message = null) =>
        actual.ShouldBe<byte>((byte)expected, message);

    public static void ShouldBe(this short actual, int expected, string? message = null) =>
        actual.ShouldBe<short>((short)expected, message);

    public static void ShouldBe(this DWord actual, int expected, string? message = null) =>
        actual.ShouldBe<DWord>((DWord)expected, message);
}