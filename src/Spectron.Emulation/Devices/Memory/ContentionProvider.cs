using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation.Devices.Memory;

internal sealed class ContentionProvider(int firstPixelTick, int ticksPerLine) : IContentionProvider
{
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];
    private readonly int[] _contentionTable = BuildContentionTable(firstPixelTick, ticksPerLine);

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

    private bool IsRamBankContended => (ActiveRamBankId & 0x01) == 0x01;  // Bank 1, 3, 5 or 7

    private static int[] BuildContentionTable(int firstPixelTick, int ticksPerLine)
    {
        var contentionTable = new int[firstPixelTick + ScreenSize.ContentHeight * ticksPerLine];

        for (var line = 0; line < ScreenSize.ContentHeight; line++)
        {
            var startLineState = firstPixelTick + line * ticksPerLine;

            for (var i = 0; i < 128; i += ContentionPattern.Length)
            {
                for (var delay = 0; delay < ContentionPattern.Length; delay++)
                {
                    contentionTable[startLineState + i + delay] = ContentionPattern[delay];
                }
            }
        }

        return contentionTable;
    }
}