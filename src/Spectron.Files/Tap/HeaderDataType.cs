namespace OldBit.Spectron.Files.Tap;

/// <summary>
/// Defines standard data types for the TAP header.
/// </summary>
public enum HeaderDataType
{
    /// <summary>
    /// The 'Program' value indicates that following block contains program in
    ///  ZX BASIC.
    /// </summary>
    Program = 0,

    /// <summary>
    /// The 'NumberArray' value indicates that following block contains data saved
    /// as an array of numbers.
    /// </summary>
    NumberArray = 1,

    /// <summary>
    /// The 'CharArray' value indicates that following block contains data saved
    /// as an array of characters.
    /// </summary>
    CharArray = 2,

    /// <summary>
    /// The 'Code' value indicates that following block contains binary code.
    /// </summary>
    Code = 3
}