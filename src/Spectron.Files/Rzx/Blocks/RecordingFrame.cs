using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Layout of the input log for the frame.
/// </summary>
public sealed class RecordingFrame
{
    private const Word RepeatedFrameMarker = 65535;

    /// <summary>
    /// Fetch counter till next interrupt (i.e. number of R increments, INTA excluded)
    /// </summary>
    public Word FetchCounter { get; init; }

    /// <summary>
    /// IN counter. Number of I/O port reads performed by the CPU in this frame (their return values follow).
    /// If equal to 65,535, this was a repeated frame, i.e. the port reads were exactly the same of the last frame.
    /// </summary>
    public Word InCounter { get; private init; }

    /// <summary>
    /// Return values for the CPU I/O port reads.
    /// </summary>
    public List<byte> InValues { get; internal set; } = [];

    internal static RecordingFrame Read(ByteStreamReader reader)
    {
        var block = new RecordingFrame
        {
            FetchCounter = reader.ReadWord(),
            InCounter = reader.ReadWord()
        };

        if (block.InCounter != RepeatedFrameMarker)
        {
            block.InValues.AddRange(reader.ReadBytes(block.InCounter));
        }

        return block;
    }

    internal void Write(Stream stream, bool isRepeatedFrame)
    {
        if (FetchCounter == 0)
        {
            return;
        }

        stream.WriteWord(FetchCounter);

        if (isRepeatedFrame)
        {
            stream.WriteWord(RepeatedFrameMarker);
        }
        else
        {
            stream.WriteWord(InValues.Count);
            stream.WriteBytes(InValues);
        }
    }
}