using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.IO;

/// <summary>
/// Methods to write data to a stream.
/// </summary>
internal class DataWriter(Stream stream)
{
    /// <summary>
    /// Writes a data block to the stream.
    /// </summary>
    /// <param name="block">The block to write to the stream.</param>
    internal void Write(IBlock block)
    {
        var data = FileDataSerializer.Serialize(block);

        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Writes a TAP data to the stream.
    /// </summary>
    /// <param name="tapData">The TAP data to write to the stream..</param>
    internal void Write(TapData tapData)
    {
        var data = FileDataSerializer.Serialize(tapData);

        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Writes a word to the stream.
    /// </summary>
    /// <param name="value">The word value to write to the stream..</param>
    internal void WriteWord(Word value) => stream.WriteWord(value);
}