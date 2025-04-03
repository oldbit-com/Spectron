namespace OldBit.Spectron.Emulator.Tests.Fixtures;

public static class ShouldlyExtensions
{
    public static void ShouldBe(this byte actual, int expected, string? message = null) =>
        actual.ShouldBe<byte>((byte)expected, message);
}