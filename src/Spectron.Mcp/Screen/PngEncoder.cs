using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Mcp.Screen;

/// <summary>
/// Minimal PNG encoder (8-bit RGB, no filtering), so screenshots don't need a UI or imaging library.
/// </summary>
internal static class PngEncoder
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static readonly uint[] CrcTable = CreateCrcTable();

    /// <summary>
    /// Encodes a region of the frame buffer as PNG, optionally scaled up by an integer factor.
    /// </summary>
    internal static byte[] Encode(FrameBuffer frameBuffer, int left, int top, int width, int height, int scale = 1)
    {
        var scaledWidth = width * scale;
        var scaledHeight = height * scale;

        using var output = new MemoryStream();
        output.Write(Signature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, scaledWidth);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], scaledHeight);
        header[8] = 8;  // Bit depth
        header[9] = 2;  // Color type: RGB
        WriteChunk(output, "IHDR", header);

        WriteChunk(output, "IDAT", Compress(frameBuffer, left, top, width, height, scale));
        WriteChunk(output, "IEND", []);

        return output.ToArray();
    }

    private static byte[] Compress(FrameBuffer frameBuffer, int left, int top, int width, int height, int scale)
    {
        var row = new byte[1 + width * scale * 3];

        using var compressed = new MemoryStream();

        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                var offset = 1;
                var lineStart = (top + y) * frameBuffer.Width + left;

                for (var x = 0; x < width; x++)
                {
                    var color = frameBuffer.Pixels[lineStart + x];

                    for (var i = 0; i < scale; i++)
                    {
                        row[offset++] = color.Red;
                        row[offset++] = color.Green;
                        row[offset++] = color.Blue;
                    }
                }

                for (var i = 0; i < scale; i++)
                {
                    zlib.Write(row);
                }
            }
        }

        return compressed.ToArray();
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> buffer = stackalloc byte[4];

        BinaryPrimitives.WriteInt32BigEndian(buffer, data.Length);
        output.Write(buffer);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);

        var crc = UpdateCrc(0xFFFFFFFF, typeBytes);
        crc = UpdateCrc(crc, data) ^ 0xFFFFFFFF;
        BinaryPrimitives.WriteUInt32BigEndian(buffer, crc);
        output.Write(buffer);
    }

    private static uint UpdateCrc(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
        {
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];

        for (uint n = 0; n < table.Length; n++)
        {
            var c = n;

            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
