using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Sna;

/// <summary>
/// Represents a .sna file. Provides methods to load and save SNA files.
/// </summary>
public sealed class SnaFile
{
    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    [FileData(Order = 0)]
    public SnaHeader Header { get; set; } = new();

    /// <summary>
    /// Gets or sets the 48K RAM data.
    /// </summary>
    [FileData(Order = 1)]
    public byte[] Ram48 { get; set; } = [];

    /// <summary>
    /// Gets or sets the SNA header.
    /// </summary>
    [FileData(Order = 2)]
    public SnaHeader128? Header128 { get; set; }

    /// <summary>
    /// Gets or sets the remaining banks of the 128K RAM.
    /// </summary>
    [FileData(Order = 3)]
    public List<byte[]>? RamBanks { get; set; }

    /// <summary>
    /// Loads a SNA file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the SNA data.</param>
    /// <returns>The loaded SnaFile object.</returns>
    public static SnaFile Load(Stream stream) => Parser.Parse(stream);

    /// <summary>
    /// Loads a SNA file from the given file.
    /// </summary>
    /// <param name="fileName">The file containing the SNA data.</param>
    /// <returns>The loaded SnaFile object.</returns>
    public static SnaFile Load(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return Load(stream);
    }

    /// <summary>
    /// Saves the SNA file to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the SNA data to.</param>
    public void Save(Stream stream)
    {
        var data = FileDataSerializer.Serialize(this);

        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Saves the SNA data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the SNA data to.</param>
    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        Save(stream);
    }
}