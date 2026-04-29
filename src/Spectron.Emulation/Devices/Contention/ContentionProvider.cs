using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.Devices.Contention;

internal static class ContentionProvider
{
    private static readonly int[] ContentionPattern = [6, 5, 4, 3, 2, 1, 0, 0];

    internal static int[] BuildContentionTable(int firstPixelTick, int ticksPerLine)
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