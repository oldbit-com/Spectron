using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Rzx.Blocks;

internal sealed class BlockHeader(byte blockId, DWord size)
{
    internal byte BlockId { get; } = blockId;
    internal DWord Size { get; } = size;

    internal static BlockHeader? Read(ByteStreamReader reader)
    {
        if (reader.TryReadByte(out var blockId) && reader.TryReadDWord(out var size))
        {
            return new BlockHeader(blockId, size);
        }

        return null;
    }
}