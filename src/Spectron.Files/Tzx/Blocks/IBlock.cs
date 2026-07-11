namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents a block in the TZX file structure.
/// </summary>
public interface IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    byte BlockId { get; }
}