using System.IO.Compression;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('T','A','P','E') contains the state of the virtual cassette recorder and its contents.
/// </summary>
public sealed class TapeBlock
{
    /// <summary>
    /// There is a tape file embedded in this block.
    /// </summary>
    public const Word FlagsEmbedded = 1;

    /// <summary>
    /// The embedded tape file in this block has been compressed by the Zlib compression library.
    /// </summary>
    public const Word FlagsCompressed = 2;

    /// <summary>
    /// Gets the current block number (ie. the position of the virtual tape head) starting from 0.
    /// </summary>
    public Word CurrentBlockNo { get; set; }

    /// <summary>
    /// Gets the various flags indicating whether a tape file is embedded in this block or linked to on disk.
    /// </summary>
    public Word Flags { get; init; }

    /// <summary>
    /// Gets the uncompressed size in bytes of the tape file. This value is undefined if the tape file is not embedded.
    /// </summary>
    public DWord UncompressedSize { get; private init; }

    /// <summary>
    /// Gets the compressed data size.
    /// </summary>
    public DWord CompressedSize { get; private init; }

    /// <summary>
    /// Gets the file extension (case-insensitive) of an embedded tape file.
    /// This value is undefined if the tape file is not embedded.
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Depending on the Flags, gets the embedded or linked tape file name.
    /// </summary>
    public byte[] Data { get; private set; } = [];

    private TapeBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the TapeBlock class.
    /// </summary>
    /// <param name="data">The tape data.</param>
    /// <param name="compressionLevel">Specifies the default compression level if data compression is on.</param>
    public TapeBlock(byte[] data, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        var compress = compressionLevel != CompressionLevel.NoCompression;

        Flags = (Word)(compress ? FlagsCompressed + FlagsEmbedded : FlagsEmbedded);
        UncompressedSize = (DWord)data.Length;
        Data = compress ? ZLibHelper.Compress(data, compressionLevel) : data;
        CompressedSize = (DWord)Data.Length;
    }

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Tape, 28 + Data.Length );
        header.Write(writer);

        writer.WriteWord(CurrentBlockNo);
        writer.WriteWord(Flags);
        writer.WriteDWord(UncompressedSize);
        writer.WriteDWord(CompressedSize);
        writer.WriteChars(FileExtension, 16);
        writer.WriteBytes(Data);
    }

    internal static TapeBlock Read(ByteStreamReader reader, int size)
    {
        var tape = new TapeBlock
        {
            CurrentBlockNo = reader.ReadWord(),
            Flags = reader.ReadWord(),
            UncompressedSize = reader.ReadDWord(),
            CompressedSize = reader.ReadDWord(),
            FileExtension = reader.ReadChars(16)
        };

        var data = reader.ReadBytes(size - 28);
        tape.Data = tape.Flags >= FlagsCompressed ? ZLibHelper.Decompress(data) : data;

        return tape;
    }
}