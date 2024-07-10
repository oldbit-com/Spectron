using OldBit.Z80Cpu.Contention;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class ContendedMemoryProvider : IContentionProvider
{
    private const int FirstState = 14335;           // T-state for the 1st visible pixel
    private const int StatesPerLine = 224;          // T-states per line pixels
    private const int LineCount = 24 * 8;           // 192 lines
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];
    private readonly int[] _contentionTable = BuildContentionTable();

    private static int[] BuildContentionTable()
    {
        var contentionTable = new int[FirstState + LineCount * StatesPerLine];

        for (var line = 0; line < LineCount; line++)
        {
            var startLineState = FirstState + line * StatesPerLine;

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

    public int GetMemoryContention(int currentStates, Word address)
    {
        if (address is < 0x4000 or > 0x7fff)
        {
            return 0;
        }

        if (currentStates < _contentionTable.Length && currentStates >= FirstState)
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

        if (currentStates < _contentionTable.Length)
        {
            return _contentionTable[currentStates];
        }

        return 0;
    }
}