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
    /// LD_BYTES subroutine.
    /// </summary>
    internal const Word LD_BYTES = 0x0556;

    /// <summary>
    /// LD_BYTES subroutine return
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