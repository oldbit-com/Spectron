using System.Text;

namespace OldBit.Spectron.Files.Extensions;

internal static class StreamExtensions
{
    extension(Stream stream)
    {
        internal void WriteWord(int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }

        internal void WriteDWord(DWord value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        internal void WriteBytes(byte[] value) => stream.Write(value, 0, value.Length);

        internal void WriteChars(string value, int length)
        {
            if (length == 0)
            {
                return;
            }

            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, Math.Min(bytes.Length, length));

            if (bytes.Length < length)
            {
                stream.Write(new byte[length - bytes.Length], 0, length - bytes.Length);
            }
        }
    }
}