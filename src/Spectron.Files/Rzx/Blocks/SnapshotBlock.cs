using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Snapshot block.
/// </summary>
public class SnapshotBlock
{
    /// <summary>
    /// Flags.
    /// b0: External Data (this block contains a snapshot descriptor structure instead of the snapshot image).
    /// b1: Compressed data.
    /// </summary>
    public DWord Flags { get; private init; }

    /// <summary>
    /// Snapshot filename extension ("SNA", "Z80", etc).
    /// </summary>
    public string Extension { get; private set; } = string.Empty;

    /// <summary>
    /// Uncompressed snapshot length (same as SL is the snapshot is not compressed
    /// </summary>
    public DWord UncompressedSize { get; private set; }

    /// <summary>
    /// Input recording block.
    /// </summary>
    public RecordingBlock Recording { get; internal set; } = new();

    /// <summary>
    /// Snapshot data.
    /// </summary>
    public byte[]? Data { get; private set; }

    /// <summary>
    /// Creates a snapshot block. Like a block read from a file, the data is held uncompressed,
    /// compression is applied only when the block is written.
    /// </summary>
    /// <param name="extension">Snapshot file extension, e.g. "szx".</param>
    /// <param name="data">Uncompressed snapshot data.</param>
    public static SnapshotBlock Create(string extension, byte[] data) => new()
    {
        Extension = extension,
        UncompressedSize = (DWord)data.Length,
        Data = data
    };

    internal static SnapshotBlock Read(ByteStreamReader reader, DWord blockLength)
    {
        var block = new SnapshotBlock
        {
            Flags = reader.ReadDWord()
        };

        if ((block.Flags & 0x01) == 0x01)
        {
            throw new NotSupportedException("Only compressed snapshots are supported.");
        }

        var isCompressed = (block.Flags & 0x02) == 0x02;

        var bytes = reader.ReadBytes(4);
        block.Extension = bytes.ToAsciiString().Trim();
        block.UncompressedSize = reader.ReadDWord();

        var data = reader.ReadBytes((int)(blockLength - 17));
        block.Data = isCompressed ? ZLibHelper.Decompress(data) : data;

        return block;
    }
}