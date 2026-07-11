using System.Diagnostics.CodeAnalysis;
using OldBit.Spectron.Files.Extensions;

namespace OldBit.Spectron.Files.Tap;

/// <summary>
/// Represents the .tap file header.
/// </summary>
public sealed class TapHeader
{
    /// <summary>
    /// Gets or sets the type of the data stored.
    /// </summary>
    public HeaderDataType DataType { get; set; }

    /// <summary>
    /// Gets or sets the file name. Maximum 10 characters.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the length of the data block.
    /// </summary>
    public Word DataLength { get; set; }

    /// <summary>
    /// Gets or sets the parameter 1 value.
    /// The value is specific to the type of the data block.
    /// </summary>
    public Word Parameter1 { get; set; }

    /// <summary>
    /// Gets or sets the parameter 2 value.
    /// The value is specific to the type of the data block.
    /// </summary>
    public Word Parameter2 { get; set; }

    /// <summary>
    /// Tries to parse the provided data into a TapeHeader object.
    /// </summary>
    /// <param name="data">The data to parse.</param>
    /// <param name="header">When this method returns, contains the TapeHeader object created from the parsed data,
    /// if the parse operation was successful, or null if the parse operation failed.</param>
    /// <returns>True if the data was successfully parsed into a TapeHeader object; otherwise, false.</returns>
    public static bool TryParse(IEnumerable<byte> data, [NotNullWhen(true)] out TapHeader? header)
    {
        header = null;
        var headerData = data.ToArray();
        if (headerData.Length != 17)
        {
            return false;
        }

        header = new TapHeader
        {
            DataType = (HeaderDataType)headerData[0],
            FileName = headerData[1..11].ToAsciiString(),
            DataLength = (Word)(headerData[11] | (headerData[12] << 8)),
            Parameter1 = (Word)(headerData[13] | (headerData[14] << 8)),
            Parameter2 = (Word)(headerData[15] | (headerData[16] << 8))
        };

        return true;
    }
}