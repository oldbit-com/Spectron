namespace OldBit.Spectron.Emulation.Screen;

public enum ScreenMode
{
    /// <summary>
    /// Standard Spectrum screen.
    /// </summary>
    Spectrum = 0x00,

    /// <summary>
    /// Timex second screen, the same as Spectrum, but at 0x6000.
    /// </summary>
    TimexSecondScreen = 0x01,

    /// <summary>
    /// Hi-color Timex screen. The standard screen is used for data, and the second screen for attributes.
    /// </summary>
    TimexHiColor = 0x02,

    /// <summary>
    /// Hi-color Timex screen. Similar to TimexHiColor. The second screen is used for both data and attributes.
    /// </summary>
    TimexHiColorAlt = 0x03,

    /// <summary>
    /// Hi-res Timex screen. The standard screen is used to display even bytes and
    /// attributes for odd bytes.
    /// </summary>
    TimexHiResAttr = 0x04,

    /// <summary>
    /// Hi-res Timex screen. The second screen is used to display even bytes and
    /// attributes for odd bytes.
    /// </summary>
    TimexHiResAttrAlt = 0x05,

    /// <summary>
    /// Hi-res Timex screen. The standard screen is used to display even bytes and
    /// a second screen for odd bytes to create a 512x192 pixel screen.
    /// </summary>
    TimexHiRes = 0x06,

    /// <summary>
    /// Hi-res Timex screen. The second is used to display even and odd bytes (duplicated).
    /// </summary>
    TimexHiResDouble = 0x07,
}