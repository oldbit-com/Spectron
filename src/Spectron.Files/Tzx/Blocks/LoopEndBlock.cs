using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Loop End' block.
/// </summary>
public class LoopEndBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.LoopEnd;

    /// <summary>
    /// Converts the 'Loop End' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of 'Loop End' object.</returns>
    public override string ToString() => "Loop End";
}