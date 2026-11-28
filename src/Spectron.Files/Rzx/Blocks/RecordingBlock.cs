using System.IO.Compression;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Actual input recording data.
/// </summary>
public sealed class RecordingBlock
{
    private const byte ProtectedFlag = 0x01;
    private const byte CompressedFlag = 0x02;

    /// <summary>
    /// Number of frames in the block.
    /// </summary>
    public DWord FrameCount { get; init; }

    /// <summary>
    /// Reserved.
    /// </summary>
    public byte Reserved { get; private set; }

    /// <summary>
    /// T-STATES counter at the beginning.
    /// </summary>
    public DWord TStatesCounter { get; set; }

    /// <summary>
    /// Flags (b0: Protected (frames are encrypted with x-key), b1: Compressed data.)
    /// </summary>
    public DWord Flags { get; init; } = CompressedFlag;

    /// <summary>
    /// Sequence of input frames.
    /// </summary>
    public List<RecordingFrame> Frames { get; } = [];

    private bool IsCompressed => (Flags & CompressedFlag) != 0;
    private bool IsProtected => (Flags & ProtectedFlag) != 0;

    internal static RecordingBlock Read(ByteStreamReader reader, DWord blockLength)
    {
        var block = new RecordingBlock
        {
            FrameCount = reader.ReadDWord(),
            Reserved = reader.ReadByte(),
            TStatesCounter = reader.ReadDWord(),
            Flags = reader.ReadDWord()
        };

        if (block.IsProtected)
        {
            throw new NotSupportedException("Protected recording blocks are not supported.");
        }

        var data = reader.ReadBytes((int)(blockLength - 18));

        if (block.IsCompressed)
        {
            data = ZLibHelper.Decompress(data);
        }

        ReadFrames(block, data);

        return block;
    }

    internal void Write(Stream stream)
    {
        stream.WriteByte(BlockIds.Recording);

        RecordingFrame? previousFrame = null;
        using var memoryStream = new MemoryStream();

        foreach (var frame in Frames)
        {
            // Check for repeated frame, e.g. the same reads as previous frame
            var isRepeatedFrame = frame.InValues.Count == previousFrame?.InValues.Count &&
                                  frame.InValues.SequenceEqual(previousFrame.InValues);

            frame.Write(memoryStream, isRepeatedFrame);
            previousFrame = frame;
        }

        var data = IsCompressed ?
            ZLibHelper.Compress(memoryStream.ToArray(), CompressionLevel.Fastest) :
            memoryStream.ToArray();

        stream.WriteDWord(18 + data.Length);
        stream.WriteDWord(Frames.Count);
        stream.WriteByte(0);
        stream.WriteDWord(TStatesCounter);
        stream.WriteDWord(Flags);
        stream.WriteBytes(data);
    }

    private static void ReadFrames(RecordingBlock block, byte[] data)
    {
        using var stream = new MemoryStream(data);
        var reader = new ByteStreamReader(stream);

        for (var i = 0; i < block.FrameCount; i++)
        {
            var frame = RecordingFrame.Read(reader);

            // Skip bad frames, some emulators generate then. Otherwise, this will
            // "hang" the execution (0 means no limit in terms of CPU instruction per frame).
            if (frame.FetchCounter == 0)
            {
                continue;
            }

            if (frame.InCounter == 65535)
            {
                // Repeated frame, copy "in" values from the previous frame
                frame.InValues = block.Frames.LastOrDefault()?.InValues ?? [];
            }

            block.Frames.Add(frame);
        }
    }
}