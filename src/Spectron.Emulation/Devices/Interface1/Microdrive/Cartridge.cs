namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

/// <summary>
/// Represents Microdrive cartridge stored in an MDR data file.
/// MDR file consists of 15 bytes header block, followed by 528 bytes data block repeated 254 times.
/// </summary>
public sealed class Cartridge
{
    private const int HeaderSize = 15;
    private const int DataSize = 512;
    private const int BlockSize = HeaderSize + HeaderSize + DataSize + 1;
    private const int MaxBlocks = 254;
    private const int MaxMdrSizeInBytes = MaxBlocks * BlockSize + 1;

    internal bool IsWriteProtected { get; set; }

    internal int BlockCount { get; }
    internal List<byte[]> Blocks { get; }

    public string? FilePath { get; }

    internal Cartridge()
    {
        Blocks  = [];

        for (var i = MaxBlocks; i > 0; i--)
        {
            Blocks.AddRange(new byte[HeaderSize]);
            Blocks.AddRange(new byte[BlockSize - HeaderSize]);
        }
    }

    internal Cartridge(string filePath) : this(File.ReadAllBytes(filePath)) =>
        FilePath = filePath;

    internal Cartridge(byte[] data)
    {
        EnsureValidMdrDataSize(data);

        BlockCount = data.Length / BlockSize;

        if (data.Length % BlockSize == 1)
        {
            IsWriteProtected = data[^1] != 0;
        }

        Blocks = SplitToBlocks(data);
    }

    public async Task SaveAsync(string filePath)
    {
        await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        foreach (var block in Blocks)
        {
            await file.WriteAsync(block);
        }

        file.Write(IsWriteProtected ? [0x01] : [0x00]);

        file.Flush();
        file.Close();
    }

    private static List<byte[]> SplitToBlocks(byte[] data)
    {
        var result = new List<byte[]>();

        foreach (var chunk in data.Chunk(BlockSize))
        {
            if (chunk.Length != BlockSize)
            {
                break;
            }

            result.AddRange(chunk.Take(HeaderSize).ToArray());
            result.AddRange(chunk.Skip(HeaderSize).ToArray());
        }

        return result;
    }

    private static void EnsureValidMdrDataSize(byte[] data)
    {
        if (data.Length < 10 * BlockSize)
        {
            throw new InvalidDataException("MDR data length is too small.");
        }

        if ((data.Length % BlockSize) > 1)
        {
            throw new InvalidDataException("MDR data length is invalid.");
        }

        if (data.Length > MaxMdrSizeInBytes)
        {
            throw new InvalidDataException("MDR data length is too big.");
        }
    }
}