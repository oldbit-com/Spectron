using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation.Devices.Contention;

internal sealed class ContentionProvider48K(int firstPixelTick, int ticksPerLine, int clockMultiplier = 1) : IContentionProvider
{
    private readonly int[] _contentionTable = ContentionProvider.BuildContentionTable(firstPixelTick, ticksPerLine);

    internal int ClockMultiplier { get; set; } = clockMultiplier;

    public int GetMemoryContention(int ticks, Word address)
    {
        var ulaTicks = ticks / ClockMultiplier;

        return ulaTicks >= 0 && ulaTicks < _contentionTable.Length ? _contentionTable[ulaTicks] * ClockMultiplier : 0;
    }

    public int GetPortContention(int ticks, Word port)
    {
        var ulaTicks = ticks / ClockMultiplier;

        return ulaTicks >= 0 && ulaTicks < _contentionTable.Length ? _contentionTable[ulaTicks] * ClockMultiplier : 0;
    }

    public bool IsAddressContended(Word address) => address is >= 0x4000 and <= 0x7FFF;

    public bool IsPortContended(Word port) => port is >= 0x4000 and <= 0x7FFF;
}