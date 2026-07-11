using System.Diagnostics.CodeAnalysis;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tap.Extensions;

namespace OldBit.Spectron.Files.Tap;

/// <summary>
/// Represents a block of data in the tape.
/// </summary>
public sealed class TapData
{
    /// <summary>
    /// Gets the length of the block data.
    /// </summary>
    public Word Length => (Word)(Data.Count + 2);

    /// <summary>
    /// Gets or sets type of the block data.
    /// Typically, there are two standard values: 0x00 for header and 0xFF for data.
    /// </summary>
    [FileData(Order = 1)]
    public byte Flag { get; set; }

    /// <summary>
    /// Gets or sets data.
    /// </summary>
    [FileData(Order = 2)]
    public List<byte> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the control sum of the data.
    /// </summary>
    [FileData(Order = 3)]
    public byte Checksum { get; set; }

    /// <summary>
    /// Initializes a new empty instance of the <see cref="TapData"/> class.
    /// </summary>
    public TapData()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TapData"/> class with the specified flag and data.
    /// </summary>
    /// <param name="flag">The type of the block data. Typically, there are two standard values: 0x00 for header and 0xFF for data.</param>
    /// <param name="data">The block data as a list of bytes.</param>
    public TapData(byte flag, List<byte> data)
    {
        Flag = flag;
        Data = data;
        Checksum = CalculateChecksum();
    }

    /// <summary>
    /// Tries to parse the given data into a TapeData object.
    /// </summary>
    /// <param name="data">The data to parse.</param>
    /// <param name="tapData">The resulting TapeData object if the parsing is successful.</param>
    /// <returns>True if the parsing is successful, otherwise false.</returns>
    public static bool TryParse(IEnumerable<byte> data, [NotNullWhen(true)] out TapData? tapData)
    {
        tapData = null;

        var dataBytes = data.ToArray();
        if (dataBytes.Length < 2)
        {
            return false;
        }

        tapData = new TapData
        {
            Flag = dataBytes[0],
            Data = dataBytes[1..^1].ToList(),
            Checksum = dataBytes[^1]
        };

        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the block is a header.
    /// </summary>
    public bool IsHeader => Flag == 0x00;

    /// <summary>
    /// Calculates the checksum of the data.
    /// The checksum is calculated by performing a bitwise exclusive OR (XOR) operation
    /// on the flag and each byte in the data.
    /// </summary>
    /// <returns>The calculated checksum as a byte.</returns>
    public byte CalculateChecksum()
    {
        var checksum = Flag;

        Data.ForEach(b => { checksum ^= b; });

        return checksum;
    }

    /// <summary>
    /// Converts the TapData to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of this object.</returns>
    public override string ToString()
    {
        if (IsHeader && TapHeader.TryParse(Data, out var header))
        {
            var name = header.GetDataTypeName();
            return $"{name}: {header.FileName}";
        }

        return Data.Count == 1 ? "1 byte" : $"{Data.Count} bytes";;
    }
}