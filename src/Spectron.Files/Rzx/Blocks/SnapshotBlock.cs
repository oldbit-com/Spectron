using System.IO.Compression;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Snapshot block.
/// </summary>
public sealed class SnapshotBlock
{
    private const byte ExternalFileFlag = 0x01;
    private const byte CompressedFlag = 0x02;

    /// <summary>
    /// Flags.
    /// b0: External Data (this block contains a snapshot descriptor structure instead of the snapshot image).
    /// b1: Compressed data.
    /// </summary>
    private DWord Flags { get; init; }

    /// <summary>
    /// Snapshot filename extension ("SNA", "Z80", etc).
    /// </summary>
    public string Extension { get; private set; } = string.Empty;

    /// <summary>
    /// Uncompressed snapshot length (same as SL is the snapshot is not compressed).
    /// </summary>
    private DWord UncompressedSize { get; set; }

    /// <summary>
    /// Input recording block.
    /// </summary>
    public RecordingBlock Recording { get; internal set; } = new();

    /// <summary>
    /// Snapshot data.
    /// </summary>
    public byte[] Data { get; private set; } = [];

    private bool IsCompressed => (Flags & CompressedFlag) != 0;
    private bool IsExternalFile => (Flags & ExternalFileFlag) != 0;

    private SnapshotBlock()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotBlock"/> class using the specified snapshot file extension and snapshot data.
    /// </summary>
    /// <param name="extension">
    /// The snapshot format extension, such as the file type identifier stored with the snapshot block.
    /// </param>
    /// <param name="data">
    /// The snapshot payload bytes to store in the block.
    /// </param>
    public SnapshotBlock(string extension, byte[] data)
    {
        Extension = extension;
        UncompressedSize = (DWord)data.Length;
        Flags = CompressedFlag;
        Data = data;
    }

    internal static SnapshotBlock Read(ByteStreamReader reader, DWord blockLength)
    {
        var block = new SnapshotBlock
        {
            Flags = reader.ReadDWord()
        };

        if (block.IsExternalFile)
        {
            throw new NotSupportedException("External data files are not supported.");
        }

        var bytes = reader.ReadBytes(4);
        block.Extension = bytes.ToAsciiString().Trim();
        block.UncompressedSize = reader.ReadDWord();

        var data = reader.ReadBytes((int)(blockLength - 17));
        block.Data = block.IsCompressed ? ZLibHelper.Decompress(data) : data;

        return block;
    }

    internal void Write(Stream stream)
    {
        var data = IsCompressed ? ZLibHelper.Compress(Data, CompressionLevel.Fastest) : Data;

        stream.WriteByte(BlockIds.Snapshot);
        stream.WriteDWord(17 + data.Length);
        stream.WriteDWord(Flags);
        stream.WriteChars(Extension, 4);
        stream.WriteDWord(Data.Length);
        stream.WriteBytes(data);

        Recording.Write(stream);
    }
}