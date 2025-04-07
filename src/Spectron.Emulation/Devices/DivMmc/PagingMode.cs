namespace OldBit.Spectron.Emulation.Devices.DivMmc;

internal enum PagingMode
{
    /// <summary>
    /// No paging mode specified.
    /// </summary>
    None,

    /// <summary>
    /// Paging is enabled. The ROM is shadowed.
    /// </summary>
    On,

    /// <summary>
    /// Paging is disabled. The ROM is not shadowed.
    /// </summary>
    Off,
}