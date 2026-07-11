using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Files.Z80;

/// <summary>
/// Represents a .z80 file. Provides methods to load and save Z80 files.
/// </summary>
public sealed class Z80File
{
    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    [FileData(Order = 0)]
    public Z80Header Header { get; }

    /// <summary>
    /// Gets or sets the 48K RAM data.
    /// </summary>
    [FileData(Order = 1)]
    public byte[] Memory { get; set; } = [];

    /// <summary>
    /// Gets or sets the memory blocks.
    /// </summary>
    [FileData(Order = 2)]
    public List<MemoryBlock> MemoryBlocks { get; } = [];

    internal Z80File(Z80Header header)
    {
        Header = header;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Z80File"/> class with the specified header and memory.
    /// </summary>
    /// <param name="header">The Z80 file header.</param>
    /// <param name="memory">The memory data. It should either be 16k or 48 long</param>
    public Z80File(Z80Header header, byte[] memory) : this(header)
    {
        switch (memory.Length)
        {
            case 0x4000:
            {
                var empty = new byte[0x4000];

                MemoryBlocks.Add(new MemoryBlock(empty, 4));
                MemoryBlocks.Add(new MemoryBlock(empty, 5));
                MemoryBlocks.Add(new MemoryBlock(memory, 8));
                break;
            }
            case 0xC000:
                MemoryBlocks.Add(new MemoryBlock(memory[0x4000..0x8000], 4));
                MemoryBlocks.Add(new MemoryBlock(memory[0x8000..0xC000], 5));
                MemoryBlocks.Add(new MemoryBlock(memory[0x0000..0x4000], 8));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(memory), "Memory must be 16k or 48k long");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Z80File"/> class with the specified header and memory blocks.
    /// </summary>
    /// <param name="header">The Z80 file header.</param>
    /// <param name="blocks">The collection of memory blocks.</param>
    public Z80File(Z80Header header, IEnumerable<byte[]> blocks) : this(header)
    {
        foreach (var block in blocks)
        {
            MemoryBlocks.Add(new MemoryBlock(block, (byte)(MemoryBlocks.Count + 3)));
        }
    }

    /// <summary>
    /// Loads a Z80 file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the Z80 data.</param>
    /// <returns>The loaded Z80File object.</returns>
    public static Z80File Load(Stream stream) => Parser.Parse(stream);

    /// <summary>
    /// Loads a Z80 file from the given file.
    /// </summary>
    /// <param name="fileName">The file containing the Z80 data.</param>
    /// <returns>The loaded Z80File object.</returns>
    public static Z80File Load(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return Load(stream);
    }

    /// <summary>
    /// Saves the Z80 data to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the Z80 data to.</param>
    public void Save(Stream stream)
    {
        stream.Write(Header.Data, 0, Header.Data.Length);

        if (Memory.Length > 0)
        {
            var data = Header.Flags1.IsDataCompressed ? DataCompressor.Compress(Memory, true).ToArray() : Memory;
            stream.Write(data, 0, data.Length);
        }

        foreach (var block in MemoryBlocks)
        {
            var data = block.Serialize();
            stream.Write(data, 0, data.Length);
        }
    }

    /// <summary>
    /// Saves the Z80 data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the Z80 data to.</param>
    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        Save(stream);
    }
}