namespace OldBit.Spectron.Files.Tap.Extensions;

/// <summary>
/// Extensions for the TapHeader class.
/// </summary>
public static class TapeHeaderExtensions
{
    /// <summary>
    /// Retrieves the name of the data type based on the header's data type.
    /// </summary>
    /// <param name="header">The TapHeader instance containing the data type information.</param>
    /// <returns>A string representing the name of the data type, such as "Program", "Number Array", "Character Array", "Bytes", or "Unknown" if the data type is unrecognized.</returns>
    public static string GetDataTypeName(this TapHeader header)
    {
        return header.DataType switch
        {
            HeaderDataType.Program => "Program",
            HeaderDataType.NumberArray => "Number Array",
            HeaderDataType.CharArray => "Character Array",
            HeaderDataType.Code => "Bytes",
            _ => "Unknown"
        };
    }
}