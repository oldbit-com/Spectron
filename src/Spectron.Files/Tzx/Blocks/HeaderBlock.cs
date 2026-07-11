using System.Text;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'TZX Header' block.
/// </summary>
public class HeaderBlock : IBlock
{
    /// <summary>
    /// Expected TZX header signature.
    /// </summary>
    public const string TzxSignature = "ZXTape!";

    /// <summary>
    /// Expected TZX header signature end of text marker.
    /// </summary>
    public const byte TzxEotMarker = 0x1A;

    /// <summary>
    /// Current major TZX version.
    /// </summary>
    public const byte TzxVerMajor = 1;

    /// <summary>
    /// Current minor TZX version.
    /// </summary>
    public const byte TzxVerMinor = 20;

    /// <summary>
    /// Gets the block ID.
    /// </summary>
    public byte BlockId => 0;

    /// <summary>
    /// Gets the TZX header signature.
    /// </summary>
    [FileData(Order = 0, Size = 7)]
    public string Signature { get; private set; }

    /// <summary>
    /// Gets or sets the end of text file marker.
    /// </summary>
    [FileData(Order = 1)]
    public byte EotMarker { get; private set; }

    /// <summary>
    /// Gets or sets the TZX major revision number.
    /// </summary>
    [FileData(Order = 2)]
    public byte VerMajor { get; set; }

    /// <summary>
    /// Gets or sets the TZX minor revision number.
    /// </summary>
    [FileData(Order = 3)]
    public byte VerMinor { get; set; }

    /// <summary>
    /// Creates a new instance of the 'TZX Header' block.
    /// </summary>
    public HeaderBlock()
    {
        Signature = TzxSignature;
        EotMarker = TzxEotMarker;
        VerMajor = TzxVerMajor;
        VerMinor = TzxVerMinor;
    }

    /// <summary>
    /// Creates a new instance of the 'TZX Header' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the HeaderBlock properties.</param>
    internal HeaderBlock(ByteStreamReader reader)
    {
        Signature = Encoding.ASCII.GetString(reader.ReadBytes(7));
        EotMarker = reader.ReadByte();
        VerMajor = reader.ReadByte();
        VerMinor = reader.ReadByte();
    }

    /// <summary>
    /// Gets a value indicating whether the header block is valid.
    /// </summary>
    /// <returns><c>true</c> if the header block is valid; otherwise, <c>false</c>.</returns>
    internal bool IsValid =>
        Signature == TzxSignature && EotMarker == TzxEotMarker;
}