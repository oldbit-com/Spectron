namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal static class AddressMark
{
    /// <summary>
    /// Normal Data Address Mark (DAM).
    /// </summary>
    internal const byte Normal = 0xFB;

    /// <summary>
    /// Deleted Data Address Mark (DDAM).
    /// </summary>
    internal const byte Deleted = 0xF8;

    /// <summary>
    /// ID Address Mark (IDAM).
    /// </summary>
    internal const byte Id = 0xFE;
}