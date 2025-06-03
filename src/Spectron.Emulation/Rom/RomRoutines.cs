using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Rom;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class RomRoutines
{
    /// <summary>
    /// The 'Error' restart routine.
    /// </summary>
    internal const Word ERROR_1 = 0x0008;

    /// <summary>
    /// This subroutine is called when a data block is about to start loading from a tape.
    /// <remarks>
    ///     A'  00 (header block) or FF (data block)
    ///     F'  Carry flag set if loading, reset if verifying
    ///     DE  Block length
    ///     IX  Start address
    /// </remarks>
    /// </summary>
    internal const Word LD_START = 0x056C;

    /// <summary>
    /// This subroutine is called to save the header information and later the actual program/data block to tape.
    /// <remarks>
    ///     A   00 (header block) or FF (data block)
    ///     DE  Block length
    ///     IX  Start address
    /// </remarks>
    /// </summary>
    internal const Word SA_BYTES = 0x04C2;

    /// <summary>
    /// SA_BYTES subroutine return.
    /// </summary>
    internal const Word SA_BYTES_RET = 0x053E;

    /// <summary>
    /// Short delay before saving is complete.
    /// </summary>
    internal const Word SA_DELAY = 0x053C;

    /// <summary>
    /// LD_BYTES subroutine return.
    /// </summary>
    internal const Word LD_BYTES_RET = 0x05E2;

    /// <summary>
    /// The 'SAVE, LOAD, VERIFY and MERGE' command routine.
    /// </summary>
    internal const Word SAVE_ETC = 0x0605;

    /// <summary>
    /// Address of next item in parameter table.
    /// </summary>
    internal const Word T_ADDR = 0x5C74;
}