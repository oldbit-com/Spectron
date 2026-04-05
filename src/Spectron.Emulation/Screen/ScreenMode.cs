namespace OldBit.Spectron.Emulation.Screen;

internal enum ScreenMode
{
    /// <summary>
    /// Standard Spectrum screen.
    /// </summary>
    Spectrum,

    /// <summary>
    /// Timex screen 1, the same as Spectrum, but at 0x6000.
    /// </summary>
    TimexScreen1,

    /// <summary>
    /// Hi-color Timex screen. Standard screen is used for data, and Screen1 for attributes.
    /// </summary>
    TimexHiColor,

    /// <summary>
    /// Hi-res Timex screen. Standard screen is used to display even bytes and
    /// Screen1 for odd bytes to create a 512x192 pixel screen.
    /// </summary>
    TimexHiRes,
}