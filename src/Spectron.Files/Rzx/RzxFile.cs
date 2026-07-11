using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Rzx.Blocks;

namespace OldBit.Spectron.Files.Rzx;

/// <summary>
/// Represents a .rzx file.
/// </summary>
public sealed class RzxFile
{
    /// <summary>
    /// Gets the RZX file header.
    /// </summary>
    public RzxHeader Header { get; private set; } = new();

    /// <summary>
    /// Gets the program that created this RZX file.
    /// </summary>
    public CreatorBlock? Creator { get; private set; }

    /// <summary>
    /// Gets a list of snapshots contained in the RZX file.
    /// </summary>
    public List<SnapshotBlock> Snapshots { get; private set; } = [];

    /// <summary>
    /// Loads a RZX file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the RZX data.</param>
    /// <returns>The loaded RzxFile object.</returns>
    public static RzxFile Load(Stream stream)
    {
        var reader = new ByteStreamReader(stream);
        var header = RzxHeader.Read(reader);

        if (header.Signature != "RZX!")
        {
            throw new InvalidDataException("Not a valid RZX file. Invalid header signature.");
        }

        var file = new RzxFile { Header = header };

        while (BlockHeader.Read(reader) is { } blockHeader)
        {
            switch (blockHeader.BlockId)
            {
                case BlockIds.Creator:
                    file.Creator = CreatorBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Snapshot:
                    file.Snapshots.Add(SnapshotBlock.Read(reader, blockHeader.Size));
                    break;

                case BlockIds.Recording:
                    var recording = RecordingBlock.Read(reader, blockHeader.Size);
                    file.Snapshots.LastOrDefault()?.Recording = recording;
                    break;

                default:
                    // Ignore this block, not supported
                    reader.ReadBytes((int)blockHeader.Size - 5);
                    break;
            }
        }

        return file;
    }
}