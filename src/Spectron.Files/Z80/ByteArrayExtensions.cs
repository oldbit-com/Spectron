namespace OldBit.Spectron.Files.Z80;

internal static class ByteArrayExtensions
{
    /// <summary>
    /// Gets a 16-bit word from the specified index (LSB first).
    /// </summary>
    internal static Word GetWord(this byte[] data, int index) =>
        (Word)((data[index + 1] << 8) | data[index]);

    /// <summary>
    /// Sets a 16-bit word at the specified index (LSB first).
    /// </summary>
    internal static void SetWord(this byte[] data, int index, Word value)
    {
        data[index] = (byte)value;
        data[index + 1] = (byte)(value >> 8);
    }
}