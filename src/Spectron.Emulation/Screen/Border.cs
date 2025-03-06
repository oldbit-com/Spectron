namespace OldBit.Spectron.Emulation.Screen;

public record struct BorderTick(int StartTick, int EndTick, int StartPixel);

internal sealed class Border(HardwareSettings hardwareSettings, FrameBuffer frameBuffer)
{
    private const int LeftBorderTicks = 24;     // Number of ticks for the left border (24T)
    private const int RightBorderTicks = 24;    // Number of ticks for the right border (24T)
    private const int ContentLineTicks = 128;   // Number of ticks for the screen line content (128T).

    private readonly List<BorderTick> _borderTickRanges = BuildBorderTickRanges(hardwareSettings.RetraceTicks);

    private int _lastRangeIndex;
    private int _offset;
    private Color _lastColor = SpectrumPalette.White;

    internal void Update(Color color) => _lastColor = color;

    /// <summary>
    /// Fill the border with the specified color up to the current tick. This fills frame buffer with the
    /// current border color up to the current tick.
    /// </summary>
    /// <param name="color">The new color.</param>
    /// <param name="frameTicks">The current tick when border color is changing.</param>
    internal void Update(Color color, int frameTicks)
    {
        for (var rangeIndex = _lastRangeIndex; rangeIndex < _borderTickRanges.Count; rangeIndex++)
        {
            var tickRange = _borderTickRanges[rangeIndex];

            if (frameTicks >= tickRange.StartTick)
            {
                if (frameTicks < tickRange.EndTick)
                {
                    var startPixel = tickRange.StartPixel + 2 * _offset;
                    var count = 2 * (frameTicks - (tickRange.StartTick + _offset) + 1);

                    frameBuffer.Fill(startPixel, count, _lastColor);

                    _offset = frameTicks - tickRange.StartTick;
                    _lastRangeIndex = rangeIndex;

                    break;
                }
                else
                {
                    var startPixel = tickRange.StartPixel + 2 * _offset;
                    var count = 2 * (tickRange.EndTick - (tickRange.StartTick + _offset) + 1);

                    frameBuffer.Fill(startPixel, count, _lastColor);

                    _offset = 0;
                }
            }
            else
            {
                _lastRangeIndex = rangeIndex;
                break;
            }
        }

        _lastColor = color;
    }

    internal void NewFrame() => Invalidate();

    internal void Reset()
    {
        _lastColor = SpectrumPalette.White;
        Invalidate();
    }

    public void Invalidate()
    {
        _lastRangeIndex = 0;
        _offset = 0;
    }

    /// <summary>
    /// Builds a lookup table for border ticks so that we can quickly determine the border range and
    /// pixel position for a given tick.
    /// </summary>
    /// <returns>A list of border tick data.</returns>
    internal static List<BorderTick> BuildBorderTickRanges(int retraceTicks)
    {
        var ticksTable = new List<BorderTick>();

        var startTick = 0;
        var endTick = ContentLineTicks + LeftBorderTicks;
        var startPixel = ScreenSize.BorderLeft;

        for (var line = 0; line < ScreenSize.TotalLines; line++)
        {
            switch (line)
            {
                case < ScreenSize.BorderTop:
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + retraceTicks;
                    endTick = startTick + LeftBorderTicks + ContentLineTicks + RightBorderTicks;
                    startPixel += line == 0 ?
                        ScreenSize.ContentWidth + ScreenSize.BorderRight :
                        ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

                    break;

                case >= ScreenSize.BorderTop + ScreenSize.ContentHeight:
                    endTick = startTick + LeftBorderTicks + ContentLineTicks + RightBorderTicks;

                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + retraceTicks;
                    startPixel += ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

                    break;

                default:
                    // Left border
                    endTick = startTick + LeftBorderTicks;
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    // Skip content area
                    startTick = endTick + ContentLineTicks;
                    endTick = startTick + RightBorderTicks;
                    startPixel += ScreenSize.BorderLeft + ScreenSize.ContentWidth;

                    // Right border
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));
                    startTick = endTick + retraceTicks;
                    startPixel += ScreenSize.BorderRight;

                    break;
            }
        }

        return ticksTable;
    }
}