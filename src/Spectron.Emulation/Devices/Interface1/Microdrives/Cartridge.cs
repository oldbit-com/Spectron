namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

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
    internal List<Block> Blocks { get; }

    public string? FilePath { get; private set; }

    internal Cartridge()
    {
        Blocks  = [];

        for (var i = MaxBlocks; i > 0; i--)
        {
            Blocks.AddRange(new Block(Enumerable.Repeat((byte)0xFF, HeaderSize).ToArray()));
            Blocks.AddRange(new Block(Enumerable.Repeat((byte)0xFF, BlockSize - HeaderSize).ToArray()));
        }
    }

    internal Cartridge(string filePath) : this(File.ReadAllBytes(filePath)) =>
        FilePath = filePath;

    internal Cartridge(string? filePath, byte[] data): this(data) =>
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
            await file.WriteAsync(block.Data);
        }

        file.Write(IsWriteProtected ? [0x01] : [0x00]);

        FilePath = filePath;

        file.Flush();
        file.Close();
    }

    public byte[] GetData()
    {
        var totalLength = Blocks.Sum(block => block.Data.Length);

        var result = new byte[totalLength];
        var offset = 0;

        foreach (var block in Blocks)
        {
            Buffer.BlockCopy(block.Data, 0, result, offset, block.Data.Length);
            offset += block.Data.Length;
        }

        return result;
    }

    private static List<Block> SplitToBlocks(byte[] data)
    {
        var result = new List<Block>();

        foreach (var chunk in data.Chunk(BlockSize))
        {
            if (chunk.Length != BlockSize)
            {
                break;
            }

            result.AddRange(new Block(chunk.Take(HeaderSize).ToArray(), isPreambleValid: true));
            result.AddRange(new Block(chunk.Skip(HeaderSize).ToArray(), isPreambleValid: true));
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