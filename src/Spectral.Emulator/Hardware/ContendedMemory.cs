using OldBit.Spectral.Emulator.Screen;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulator.Hardware;

public class ContendedMemory : IContentionProvider
{
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];
    private readonly int[] _contentionTable = BuildContentionTable();

    public int GetMemoryContention(int currentStates, Word address)
    {
        if (address is < 0x4000 or > 0x7fff)
        {
            return 0;
        }

        if (currentStates < _contentionTable.Length && currentStates >= DefaultTimings.FirstPixelTick)
        {
            return _contentionTable[currentStates];
        }

        return 0;
    }

    public int GetPortContention(int currentStates, Word port)
    {
        if (port is < 0x4000 or > 0x7fff)
        {
            return 0;
        }

        return currentStates < _contentionTable.Length ? _contentionTable[currentStates] : 0;
    }

    private static int[] BuildContentionTable()
    {
        var contentionTable = new int[DefaultTimings.FirstPixelTick + DefaultSizes.ContentHeight * DefaultTimings.LineTicks];

        for (var line = 0; line < DefaultSizes.ContentHeight; line++)
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