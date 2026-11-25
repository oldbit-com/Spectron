namespace OldBit.Spectron.Emulation.Rom;

/// <summary>
/// Determines whether the tape ROM traps can be safely used with a given ROM.
/// </summary>
internal static class RomTraps
{
    // Address ranges (start inclusive, end exclusive) the tape traps depend on:
    // - SA_BYTES up to and including the CALL at LD_START: trap entry points and SA/LD_RET,
    //   which is where both fast load and fast save routines eventually return to.
    // - 'CP $01; RET' at the end of LD_BYTES, where fast load continues.
    private static readonly (int Start, int End)[] TrapRanges =
    [
        (RomRoutines.SA_BYTES, RomRoutines.LD_START + 3),
        (RomRoutines.LD_BYTES_RET - 2, RomRoutines.LD_BYTES_RET + 1),
    ];

    private static readonly Lazy<byte[]> OriginalRom = new(() => RomReader.ReadRom(RomType.Original48));

    /// <summary>
    /// Returns true if the ROM contains the original 48K tape routines at the expected addresses.
    /// Modified ROMs that keep the tape routines intact are supported, too.
    /// </summary>
    internal static bool IsSupported(ReadOnlySpan<byte> rom)
    {
        var originalRom = OriginalRom.Value;

        foreach (var (start, end) in TrapRanges)
        {
            if (rom.Length < end || !rom[start..end].SequenceEqual(originalRom.AsSpan()[start..end]))
            {
                return false;
            }
        }

        return true;
    }
}
