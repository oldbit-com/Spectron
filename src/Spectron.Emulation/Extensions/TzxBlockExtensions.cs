using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Extensions;

internal static class TzxBlockExtensions
{
    internal static IEnumerable<StandardSpeedDataBlock> GetStandardSpeedDataBlocks(this IEnumerable<IBlock> blocks) =>
        blocks.Where(b => b is StandardSpeedDataBlock).Cast<StandardSpeedDataBlock>();
}