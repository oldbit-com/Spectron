using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulation.Devices.Memory;

internal sealed class ContentionProvider : IContentionProvider
{
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];
    private readonly int[] _contentionTable = BuildContentionTable();

    public int GetMemoryContention(int ticks, Word address)
    {
        if (address is < 0x4000 or > 0x7fff)
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
        if (port is < 0x4000 or > 0x7fff)
        {
            return 0;
        }

        return ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;
    }

    private static int[] BuildContentionTable()
    {
        var contentionTable = new int[DefaultTimings.FirstPixelTick + ScreenSize.ContentHeight * DefaultTimings.LineTicks];

        for (var line = 0; line < ScreenSize.ContentHeight; line++)
        {
            var startLineState = DefaultTimings.FirstPixelTick + line * DefaultTimings.LineTicks;

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