using OldBit.ZXTape.Tzx.Blocks;

namespace OldBit.Spectral.Emulation.Extensions;

internal static class TzxBlockExtensions
{
    internal static IEnumerable<StandardSpeedDataBlock> GetStandardSpeedDataBlocks(this IEnumerable<IBlock> blocks) =>
        blocks.Where(b => b is StandardSpeedDataBlock).Cast<StandardSpeedDataBlock>();
}