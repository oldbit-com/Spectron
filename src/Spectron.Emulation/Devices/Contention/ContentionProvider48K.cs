using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation.Devices.Contention;

internal sealed class ContentionProvider48K(int firstPixelTick, int ticksPerLine) : IContentionProvider
{
    private readonly int[] _contentionTable = ContentionProvider.BuildContentionTable(firstPixelTick, ticksPerLine);

    public int GetMemoryContention(int ticks, Word address) =>
        ticks >= 0 && ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;

    public int GetPortContention(int ticks, Word port) =>
        ticks >= 0 && ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;

    public bool IsAddressContended(Word address) => address is >= 0x4000 and <= 0x7FFF;

    public bool IsPortContended(Word port) => port is >= 0x4000 and <= 0x7FFF;
}