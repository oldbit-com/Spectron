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
        if (!IsAddressInContendedArea(address))
        {
            return 0;
        }

        if (ticks < _contentionTable.Length && ticks >= firstPixelTick)
        {
            return _contentionTable[ticks];
        }

        return 0;
    }

    public int GetPortContention(int ticks, Word port)
    {
        if (!IsPortInContendedArea(port))
        {
            return 0;
        }

        return ticks < _contentionTable.Length ? _contentionTable[ticks] : 0;
    }

    private bool IsAddressInContendedArea(Word address) =>
        address is >= 0x4000 and <= 0x7FFF ||
        MemoryBankId is 1 or 3 or 5 or 7 && address >= 0xC000;

    private static bool IsPortInContendedArea(Word port) =>
        Ula.IsUlaPort(port) ||
        port is >= 0x4000 and <= 0x7FFF && !Memory128K.IsPagingPortAddress(port);

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