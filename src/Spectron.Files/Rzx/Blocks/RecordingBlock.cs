using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Actual input recording data.
/// </summary>
public class RecordingBlock
{
    /// <summary>
    /// Number of frames in the block.
    /// </summary>
    public DWord FrameCount { get; private init; }

    /// <summary>
    /// Reserved.
    /// </summary>
    public byte Reserved { get; private set; }

    /// <summary>
    /// T-STATES counter at the beginning.
    /// </summary>
    public DWord TStatesCounter { get; private set; }

    /// <summary>
    /// Flags (b0: Protected (frames are encrypted with x-key), b1: Compressed data.)
    /// </summary>
    public DWord Flags { get; private init; }

    /// <summary>
    /// Sequence of input frames.
    /// </summary>
    public List<RecordingFrame> Frames { get; } = [];

    internal static RecordingBlock Read(ByteStreamReader reader, DWord blockLength)
    {
        var block = new RecordingBlock
        {
            FrameCount = reader.ReadDWord(),
            Reserved = reader.ReadByte(),
            TStatesCounter = reader.ReadDWord(),
            Flags = reader.ReadDWord()
        };

        var isProtected = (block.Flags & 0x01) == 0x01;
        if (isProtected)
        {
            throw new NotSupportedException("Protected recording blocks are not supported.");
        }

        var data = reader.ReadBytes((int)(blockLength - 18));

        var isCompressed = (block.Flags & 0x02) == 0x02;
        if (isCompressed)
        {
            data = ZLibHelper.Decompress(data);
        }

        ReadFrames(block, data);

        return block;
    }

    private static void ReadFrames(RecordingBlock block, byte[] data)
    {
        var memoryStream = new MemoryStream(data);
        var reader = new ByteStreamReader(memoryStream);

        for (var i = 0; i < block.FrameCount; i++)
        {
            var frame = RecordingFrame.Read(reader);

            if (frame.InCounter == 65535)
            {
                // Repeated frame, copy "in" values from the previous frame
                frame.InValues = block.Frames.LastOrDefault()?.InValues ?? [];
            }

            block.Frames.Add(frame);
        }
    }
}