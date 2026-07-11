using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Loop Start' block.
/// </summary>
public class LoopStartBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.LoopStart;

    /// <summary>
    /// Gets or sets the number of repetitions.
    /// </summary>
    [FileData(Order = 1)]
    public Word Count { get; set; }

    /// <summary>
    /// Creates a new instance of the 'Loop Start' block.
    /// </summary>
    public LoopStartBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Loop Start' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the LoopStartBlock properties.</param>
    internal LoopStartBlock(ByteStreamReader reader)
    {
        Count = reader.ReadWord();
    }

    /// <summary>
    /// Converts the 'Loop Start' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Loop Start' object which corresponds to Name value.</returns>
    public override string ToString() => $"Loop for {Count} time(s)";
}