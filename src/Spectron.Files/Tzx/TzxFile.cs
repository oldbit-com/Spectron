using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tzx;

/// <summary>
/// Represents a .tzx file.
/// </summary>
public sealed class TzxFile
{
    /// <summary>
    /// Gets the TZX file header.
    /// </summary>
    public HeaderBlock Header { get; private set; } = new();

    /// <summary>
    /// Gets a list of TZX file blocks.
    /// </summary>
    public List<IBlock> Blocks { get; } = [];

    /// <summary>
    /// Loads a TZX file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the TZX data.</param>
    /// <returns>The loaded TzxFile object.</returns>
    public static TzxFile Load(Stream stream)
    {
        var reader = new BlockReader(stream);

        var tzx = new TzxFile();
        while (true)
        {
            var block = reader.ReadNextBlock();
            if (block == null)
            {
                break;
            }

            if (block is HeaderBlock headerBlock)
            {
                if (!headerBlock.IsValid)
                {
                    throw new InvalidDataException("Invalid TZX header.");
                }
                tzx.Header = headerBlock;
            }
            else
            {
                tzx.Blocks.Add(block);
            }
        }

        return tzx;
    }

    /// <summary>
    /// Loads a TZX file from the given file.
    /// </summary>
    /// <param name="fileName">The file containing the TZX data.</param>
    /// <returns>The loaded TapFile object.</returns>
    public static TzxFile Load(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return Load(stream);
    }


    /// <summary>
    /// Saves the TZX data to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the TZX data to.</param>
    public void Save(Stream stream)
    {
        var writer = new DataWriter(stream);

        writer.Write(Header);
        foreach (var block in Blocks)
        {
            writer.Write(block);
        }
    }

    /// <summary>
    /// Saves the TZX data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the TZX data to.</param>
    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);
        Save(stream);
    }
}