using System.IO.Compression;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('R','O','M',0) contains custom ROM that has been installed for the current Spectrum model.
/// </summary>
public sealed class CustomRomBlock
{
    /// <summary>
    /// Indicates the ROM image is compressed.
    /// </summary>
    public const Word FlagsCompressed = 1;

    /// <summary>
    /// Gets the flags indicating the ROM image is compressed.
    /// </summary>
    public Word Flags { get; private init; }

    /// <summary>
    /// Gets the size in bytes of the custom ROM. This will be one of:
    ///     16,384 for 16k/48k Spectrum
    ///     32,768 for Spectrum 128/+2
    ///     65,536 for Spectrum +2A/+3
    ///     32,768 for Pentagon 128
    ///     65,536 for ZS Scorpion
    ///     16,384 for Timex Sinclair TS/TC2048
    ///     24,576 for Timex Sinclair TS2068
    ///     32,768 for Spectrum SE
    /// </summary>
    public DWord UncompressedSize { get; private init; }

    /// <summary>
    /// Gets the compressed or uncompressed ROM image depending on the Flags.
    /// </summary>
    public byte[] Data { get; private set; } = [];

    private CustomRomBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the CustomRomBlock class.
    /// </summary>
    /// <param name="data">The ROM data.</param>
    /// <param name="compressionLevel">Specifies the default compression level if data compression is on.</param>
    public CustomRomBlock(byte[] data, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        var compress = compressionLevel != CompressionLevel.NoCompression;

        Flags = (Word)(compress ? FlagsCompressed : 0);
        UncompressedSize = (DWord)data.Length;
        Data = compress ? ZLibHelper.Compress(data, compressionLevel) : data;
    }

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.CustomRom, 6 + Data.Length);
        header.Write(writer);

        writer.WriteWord(Flags);
        writer.WriteDWord(UncompressedSize);
        writer.WriteBytes(Data);
    }

    internal static CustomRomBlock Read(ByteStreamReader reader, int size)
    {
        var customRom = new CustomRomBlock
        {
            Flags = reader.ReadWord(),
            UncompressedSize = reader.ReadDWord()
        };

        var data = reader.ReadBytes(size - 6);
        customRom.Data = customRom.Flags == FlagsCompressed ? ZLibHelper.Decompress(data) : data;

        return customRom;
    }
}