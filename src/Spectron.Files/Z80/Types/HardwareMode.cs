namespace OldBit.Spectron.Files.Z80.Types;

/// <summary>
/// Represents the hardware mode of the ZX Spectrum.
/// </summary>
[Flags]
public enum HardwareMode
{
    /// <summary>
    /// No hardware mode specified.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Represents ZX Spectrum 48K.
    /// </summary>
    Spectrum48 = 0x01,

    /// <summary>
    /// Represents ZX Spectrum 128K.
    /// </summary>
    Spectrum128 = 0x02,

    /// <summary>
    /// Represents the SamRam interface..
    /// </summary>
    SamRam = 0x04,

    /// <summary>
    /// Represents the MGT Sam Coupe.
    /// </summary>
    Mgt = 0x08,

    /// <summary>
    /// Represents the Interface 1.
    /// </summary>
    Interface1 = 0x10,

    /// <summary>
    /// Represents the Spectrum Plus 3.
    /// </summary>
    SpectrumPlus3 = 0x20,

    /// <summary>
    /// Represents the Pentagon 128.
    /// </summary>
    Pentagon128 = 0x40,

    /// <summary>
    /// Represents the Scorpion 256.
    /// </summary>
    Scorpion256 = 0x80,

    /// <summary>
    /// Represents the Didaktik Kompakt hardware mode.
    /// </summary>
    DidaktikKompakt = 0x100,

    /// <summary>
    /// Represents the Spectrum Plus 2 hardware mode.
    /// </summary>
    SpectrumPlus2 = 0x200,

    /// <summary>
    /// Represents the Spectrum Plus 2A hardware mode.
    /// </summary>
    SpectrumPlus2A = 0x400,

    /// <summary>
    /// Represents the TC2048.
    /// </summary>
    TC2048 = 0x800,

    /// <summary>
    /// Represents the TC2068.
    /// </summary>
    TC2068 = 0x1000,

    /// <summary>
    /// Represents the TS2068.
    /// </summary>
    TS2068 = 0x2000,
}