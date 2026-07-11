using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'CSW Recording' block.
/// </summary>
public class CswRecordingBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.CswRecording;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the block length.
    /// </summary>
    [FileData(Order = 1)]
    private int Length => 10 + Data.Count;

    /// <summary>
    /// Gets or sets the pause value (in millisecond) that should be applied after this block.
    /// </summary>
    [FileData(Order = 2)]
    public Word PauseDuration { get; set; }

    /// <summary>
    /// Gets or sets the sampling rate.
    /// </summary>
    [FileData(Order = 3, Size = 3)]
    public int SamplingRate { get; set; }

    /// <summary>
    /// Gets or sets the compression type.
    /// </summary>
    [FileData(Order = 4)]
    public CompressionType CompressionType { get; set; } = CompressionType.Rle;

    /// <summary>
    /// Gets or sets the number of stored pulses (after decompression, for validation purposes).
    /// </summary>
    [FileData(Order = 5)]
    public DWord StoredPulsesCount { get; set; }

    /// <summary>
    /// Gets or sets the CSW data encoded according to the CSW file format specification.
    /// </summary>
    [FileData(Order = 6)]
    public List<byte> Data { get; set; } = [];

    /// <summary>
    /// Creates a new instance of the 'CSW Recording' block.
    /// </summary>
    public CswRecordingBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'CSW Recording' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the CswRecordingBlock properties.</param>
    internal CswRecordingBlock(ByteStreamReader reader)
    {
        var length = reader.ReadDWord() - 10;
        PauseDuration = reader.ReadWord();
        SamplingRate = reader.ReadByte() | reader.ReadByte() << 8 | reader.ReadByte() << 16;
        CompressionType = (CompressionType)reader.ReadByte();
        StoredPulsesCount = reader.ReadDWord();
        Data.AddRange(reader.ReadBytes((int)length));
    }
}