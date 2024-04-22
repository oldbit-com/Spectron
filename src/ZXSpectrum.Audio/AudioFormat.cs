namespace ZXSpectrum.Audio;

/// <summary>
///  Represents the audio format.
/// </summary>
public enum AudioFormat
{
    /// <summary>
    /// 8-bit unsigned integer.
    /// </summary>
    Unsigned8Bit,

    /// <summary>
    /// 16-bit signed integer (little-endian).
    /// </summary>
    Signed16BitIntegerLittleEndian,

    /// <summary>
    /// 32-bit float (little-endian).
    /// </summary>
    Float32BitLittleEndian,
}