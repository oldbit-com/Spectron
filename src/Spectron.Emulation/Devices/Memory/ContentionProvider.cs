using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation.Devices.Memory;

internal sealed class ContentionProvider(int firstPixelTick, int ticksPerLine) : IContentionProvider
{
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];
    private readonly int[] _contentionTable = BuildContentionTable(firstPixelTick, ticksPerLine);

    internal int MemoryBankId { get; set; }

    public int GetMemoryContention(int ticks, Word address)
    {
        if (!IsAddressContended(address))
        {
            return 0;
        }

        if (ticks < _contentionTable.Length && ticks >= DefaultTimings.FirstPixelTick)
        {
            return _contentionTable[ticks];
        }

        return 0;
    }

    public int GetPortContention(int ticks, Word port)
    {
        if (!IsAddressContended(port))
        {
            return 0;
        }

        return ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;
    }

    private bool IsAddressContended(Word address)
    {
        if (address < 0x4000)
        {
            return false;
        }

        switch (MemoryBankId)
        {
            case 0 when address > 0x7fff:
            case 1 or 3 or 5 or 7 when address > 0xBFFF:
                return false;
        }

        return true;
    }

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