namespace OldBit.Spectron.Files.Z80;

/// <summary>
/// Provides methods for compressing and decompressing Z80 file data.
/// </summary>
internal static class DataCompressor
{
    private const byte Marker = 0xED;

    /// <summary>
    /// Compresses the given data using a simple run-length encoding algorithm specific to Z80 files.
    /// </summary>
    /// <param name="data">The data to be compressed.</param>
    /// <param name="appendEndMarker">Whether to append the end marker to the compressed data.</param>
    /// <returns>A list of bytes representing the compressed data.</returns>
    internal static List<byte> Compress(byte[] data, bool appendEndMarker)
    {
        var compressed = new List<byte>();
        var position = 0;

        while (position < data.Length)
        {
            var repeats = 0;
            var start = position;
            var current = data[position];

            // Count the number of repeated bytes
            while (position < data.Length && data[position] == current && repeats < 255)
            {
                repeats += 1;
                position += 1;
            }

            if (current == Marker)
            {
                // If the byte is the marker, we need to handle it differently
                if (repeats == 1)
                {
                    compressed.Add(current);

                    if (position < data.Length)
                    {
                        compressed.Add(data[position]);
                        position += 1;
                    }
                }
                else
                {
                    AddRepeated(compressed, repeats, Marker);
                }
            }
            else if (repeats >= 5 || (repeats > 1 && position < data.Length && data[position] == Marker))
            {
                // More than 5 bytes repeated or a sequence of more than 1 ED bytes
                AddRepeated(compressed, repeats, current);
            }
            else
            {
                // If the sequence is less than 5 bytes long, just copy the bytes
                compressed.AddRange(data[start..(repeats + start)]);
            }
        }

        if (appendEndMarker)
        {
            // Append the end marker
            compressed.AddRange([0x00, Marker, Marker, 0x00]);
        }

        return compressed;
    }

    /// <summary>
    /// Decompresses the given data using a simple run-length decoding algorithm specific to Z80 files.
    /// </summary>
    /// <param name="data">The compressed data to be decompressed.</param>
    /// <param name="hasEndMarker">Indicates whether the data has an end marker.</param>
    /// <returns>A list of bytes representing the decompressed data.</returns>
    internal static byte[] Decompress(byte[] data, bool hasEndMarker = false)
    {
        var decompressed = new List<byte>();
        var position = 0;

        while (position < data.Length)
        {
            if (position < data.Length - 1 && data[position] == Marker && data[position + 1] == Marker)
            {
                int count = data[position + 2];
                var value = data[position + 3];

                decompressed.AddRange(Enumerable.Repeat(value, count));
                position += 4;
            }
            else
            {
                decompressed.Add(data[position]);
                position++;
            }

            if (hasEndMarker && IsEndOfData(data, position))
            {
                break;
            }
        }

        return decompressed.ToArray();
    }

    private static void AddRepeated(List<byte> compressed, int count, byte value) =>
        compressed.AddRange([Marker, Marker, (byte)count, value]);

    private static bool IsEndOfData(IReadOnlyList<byte> data, int index)
    {
        return
            index < data.Count - 3 &&
            data[index] == 0x00 &&
            data[index + 1] == Marker &&
            data[index + 2] == Marker &&
            data[index + 3] == 0x00;
    }
}