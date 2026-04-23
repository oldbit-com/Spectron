using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation.Devices.Memory.Contention;

internal sealed class ContentionProvider128K(int firstPixelTick, int ticksPerLine) : IContentionProvider
{
    private readonly int[] _contentionTable = ContentionProvider.BuildContentionTable(firstPixelTick, ticksPerLine);

    internal int ActiveRamBankId { get; set; }

    public int GetMemoryContention(int ticks, Word address) =>
        ticks >= 0 && ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;

    public int GetPortContention(int ticks, Word port) =>
        ticks >= 0 && ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;

    public bool IsAddressContended(Word address) =>
        address is >= 0x4000 and <= 0x7FFF ||
        address >= 0xC000 && IsRamBankContended;

    public bool IsPortContended(Word port) =>
        port is >= 0x4000 and <= 0x7FFF ||
        port >= 0xC000 && IsRamBankContended;

    public bool IsPortLatched(Word address) => (address & 0x8002) == 0;

    private bool IsRamBankContended => (ActiveRamBankId & 0x01) == 0x01;  // Bank 1, 3, 5 or 7
}