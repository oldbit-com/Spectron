using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tzx.Extensions;

/// <summary>
/// Provides extension methods for working with TZX file blocks.
/// </summary>
public static class BlockExtensions
{
    /// <summary>
    /// Gets the display name of the specified block.
    /// </summary>
    /// <param name="block">The block object.</param>
    /// <returns>The display name of the block.</returns>
    public static string GetBlockName(this IBlock block)
    {
        return block.GetType().Name switch
        {
            nameof(HeaderBlock) => "TZX Header",
            nameof(StandardSpeedDataBlock) => "Standard Speed Data",
            nameof(TurboSpeedDataBlock) => "Turbo Speed Data",
            nameof(PureToneBlock) => "Pure Tone",
            nameof(PulseSequenceBlock) => "Pulse Sequence",
            nameof(PureDataBlock) => "Pure Data",
            nameof(DirectRecordingBlock) => "Direct Recording",
            nameof(CswRecordingBlock) => "CSW Recording",
            nameof(GeneralizedDataBlock) => "Generalized Data",
            nameof(PauseBlock) => "Pause",
            nameof(GroupStartBlock) => "Group Start",
            nameof(GroupEndBlock) => "Group End",
            nameof(JumpToBlock) => "Jump To Block",
            nameof(LoopStartBlock) => "Loop Start",
            nameof(LoopEndBlock) => "Loop End",
            nameof(CallSequenceBlock) => "Call Sequence",
            nameof(ReturnFromSequenceBlock) => "Return From Sequence",
            nameof(SelectBlock) => "Select Block",
            nameof(StopTheTape48Block) => "Stop the Tape if in 48k Mode",
            nameof(SetSignalLevelBlock) => "Set Signal Level",
            nameof(TextDescriptionBlock) => "Text Description",
            nameof(MessageBlock) => "Message",
            nameof(ArchiveInfoBlock) => "Archive Info",
            nameof(HardwareTypeBlock) => "Hardware Type",
            nameof(CustomInfoBlock) => "Custom Info",
            nameof(GlueBlock) => "Glue",
            _ => "Unknown Block Type"
        };
    }

    /// <summary>
    /// Retrieves the text associated with the specified ID from the 'Archive Info' block.
    /// </summary>
    /// <param name="archiveInfoBlock">The archive information block containing the textual information.</param>
    /// <param name="id">The identifier used to locate the specific text within the archive info block.</param>
    /// <returns>The text associated with the specified ID if found; otherwise, null.</returns>
    public static string? GetInfoText(this ArchiveInfoBlock archiveInfoBlock, int id) =>
        archiveInfoBlock.Infos.FirstOrDefault(i => i.Id == id)?.Text;
}