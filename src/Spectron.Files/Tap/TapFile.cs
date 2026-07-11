using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Tap;

/// <summary>
/// Represents a .tap file.
/// </summary>
public sealed class TapFile
{
    /// <summary>
    /// Gets a list of TAP file blocks.
    /// </summary>
    public List<TapData> Blocks { get; } = [];

    /// <summary>
    /// Loads a TAP file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the TAP data.</param>
    /// <returns>The loaded TapFile object.</returns>
    public static TapFile Load(Stream stream)
    {
        var tapFile = new TapFile();
        var reader = new ByteStreamReader(stream);

        while (reader.TryReadWord(out var length))
        {
            if (!TapData.TryParse(reader.ReadBytes(length), out var tapData))
            {
                throw new InvalidDataException("Invalid TAP data.");
            }

            tapFile.Blocks.Add(tapData);
        }

        return tapFile;
    }

    /// <summary>
    /// Loads a TAP file from the given file.
    /// </summary>
    /// <param name="fileName">The file containing the TAP data.</param>
    /// <returns>The loaded TapFile object.</returns>
    public static TapFile Load(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return Load(stream);
    }

    /// <summary>
    /// Saves the TAP data to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the TAP data to.</param>
    public void Save(Stream stream)
    {
        var writer = new DataWriter(stream);

        foreach (var block in Blocks)
        {
            writer.WriteWord(block.Length);
            writer.Write(block);
        }
    }

    /// <summary>
    /// Saves the TAP data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the TAP data to.</param>
    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        Save(stream);
    }
}