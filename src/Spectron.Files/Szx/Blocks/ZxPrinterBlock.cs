using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('Z','X','P','R') contains the status of the ZX Printer.
/// </summary>
public sealed class ZxPrinterBlock
{
    /// <summary>
    /// ZX printer emulation is enabled.
    /// <remarks>ZXSTPRF_ENABLED</remarks>
    /// </summary>
    public const Word FlagsEnabled = 1;

    /// <summary>
    /// Gets or sets flags that specify the state of the ZX Printer.
    /// <remarks>
    /// If the ZXSTPRF_ENABLED bit set, this specifies ZX Printer emulation is enabled. Otherwise it should be disabled.
    /// </remarks>
    /// </summary>
    public Word Flags { get; set; }

    private static int Size => 2;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.ZxPrinter, Size);
        header.Write(writer);

        writer.WriteWord(Flags);
    }

    internal static ZxPrinterBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid ZxPrinter block size.");
        }

        return new ZxPrinterBlock
        {
            Flags = reader.ReadWord()
        };
    }
}