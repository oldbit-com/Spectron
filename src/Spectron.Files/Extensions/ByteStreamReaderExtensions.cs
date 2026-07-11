using System.Text;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Extensions;

internal static class ByteStreamReaderExtensions
{
    internal static string ReadChars(this ByteStreamReader reader, int count)
    {
        var buffer = reader.ReadBytes(count);

        return Encoding.ASCII.GetString(buffer).Trim('\0');
    }
}