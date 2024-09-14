using OldBit.ZX.Files.Tap;

namespace OldBit.Spectron.Emulation.Tape;

internal interface ITapDataProvider
{
    internal TapData? GetNextTapData();
}