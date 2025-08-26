namespace OldBit.Spectron.Emulation.Devices.Interface1;

/// <summary>
/// Represents Microdrive cartridge as stored in an MDR data file.
/// MDR file consists of 15 bytes header block, followed by 528 bytes data block repeated 254 times.
/// </summary>
internal sealed class Cartridge
{
    internal const int HeaderSize = 15;
    internal const int DataSize = 512;
    internal const int BlockSize = HeaderSize + HeaderSize + DataSize + 1;

    private const int MaxBlocks = 254;
    private const int MaxMdrSizeInBytes = MaxBlocks * BlockSize + 1;

    private readonly List<byte[]> _blocks;
    private int _currentBlock;

    public bool IsWriteProtected { get; private set; }

    internal int BlockCount { get; }
    internal int CurrentBlockLength => _blocks[_currentBlock].Length;

    public Cartridge(string filePath) : this(File.ReadAllBytes(filePath))
    {
    }

    public Cartridge(byte[] data)
    {
        EnsureValidMdrDataSize(data);

        BlockCount = data.Length / BlockSize;

        if (data.Length % BlockSize == 1)
        {
            IsWriteProtected = data[^1] != 0;
        }

        _blocks = SplitToBlocks(data);
    }

    internal byte Read(int position) => _blocks[_currentBlock][position];

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