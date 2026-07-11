using System.IO.Compression;

namespace OldBit.Spectron.Files.Szx;

internal static class ZLibHelper
{
    internal static byte[] Decompress(byte[] data)
    {
        using var zlib = new ZLibStream(new MemoryStream(data), CompressionMode.Decompress);

        using var decompressed = new MemoryStream();
        zlib.CopyTo(decompressed);

        return decompressed.ToArray();
    }

    internal static byte[] Compress(byte[] data, CompressionLevel compressionLevel)
    {
        using var compressed = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressed, compressionLevel, leaveOpen: true))
        {
            zlibStream.Write(data, 0, data.Length);
        }

        return compressed.ToArray();
    }
}