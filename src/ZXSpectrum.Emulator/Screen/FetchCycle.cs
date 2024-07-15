namespace OldBit.ZXSpectrum.Emulator.Screen;

/// <summary>
/// Defines the fetch cycle of the ULA.
/// <remarks>
/// Each scanline of video memory fetches breaks down into a 16×8 cycle sequence with two sets of display and attribute bytes
/// (order: bitmap, attribute, bitmap+1, attribute+1) being fetched during the first 4 cycles followed by 4 idle cycles.
/// https://sinclair.wiki.zxnet.co.uk/wiki/Floating_bus
/// </remarks>
/// </summary>
internal enum FetchCycle
{
    /// <summary>
    /// Fetching first bitmap byte from memory.
    /// </summary>
    BitmapA = 0,

    /// <summary>
    /// Fetching first attribute byte from memory.
    /// </summary>
    AttrA = 1,

    /// <summary>
    /// Fetching second bitmap byte from memory.
    /// </summary>
    BitmapB = 2,

    /// <summary>
    /// Fetching second attribute byte from memory.
    /// </summary>
    AttrB = 3,

    /// <summary>
    /// Idle cycle 1.
    /// </summary>
    Idle1 = 4,

    /// <summary>
    /// Idle cycle 2.
    /// </summary>
    Idle2 = 5,

    /// <summary>
    /// Idle cycle 3.
    /// </summary>
    Idle3 = 6,

    /// <summary>
    /// Idle cycle 4.
    /// </summary>
    Idle4 = 7
}
