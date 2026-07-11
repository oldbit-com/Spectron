using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Group End' block.
/// </summary>
public class GroupEndBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.GroupEnd;

    /// <summary>
    /// Converts the 'Group End' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of 'Group End' object.</returns>
    public override string ToString() => "Group End";
}