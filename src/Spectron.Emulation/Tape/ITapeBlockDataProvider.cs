using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

internal interface ITapeBlockDataProvider
{
    IBlock? GetNextBlock();
}