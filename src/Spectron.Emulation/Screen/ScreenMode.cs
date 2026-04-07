namespace OldBit.Spectron.Emulation.Screen;

internal enum ScreenMode
{
    /// <summary>
    /// Standard Spectrum screen.
    /// </summary>
    Spectrum = 0x00,

    /// <summary>
    /// Timex screen 1, the same as Spectrum, but at 0x6000.
    /// </summary>
    TimexScreen1 = 0x01,

    /// <summary>
    /// Hi-color Timex screen. Standard screen is used for data, and Screen1 for attributes.
    /// </summary>
    TimexHiColor = 0x02,

    /// <summary>
    /// Hi-color Timex screen. Similar to TimexHiColor. Screen1 is used for both data and attributes.
    /// </summary>
    TimexHiColorAlt = 0x03,

    /// <summary>
    /// Hi-res Timex screen. Standard screen is used to display even bytes and
    /// Screen1 for odd bytes to create a 512x192 pixel screen.
    /// </summary>
    TimexHiRes = 0x06,
}