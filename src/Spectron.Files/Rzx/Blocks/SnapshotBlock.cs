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
    public byte[]? Data { get; set; }

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