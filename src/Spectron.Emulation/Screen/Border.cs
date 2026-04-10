namespace OldBit.Spectron.Emulation.Screen;

public record struct BorderTick(int StartTick, int EndTick, int StartPixel, int Shift = 1);

internal sealed class Border(HardwareSettings hardwareSettings, FrameBuffer frameBuffer)
{
    private const int LeftBorderTicks = 24;     // Number of ticks for the left border (24T)
    private const int RightBorderTicks = 24;    // Number of ticks for the right border (24T)
    private const int ContentLineTicks = 128;   // Number of ticks for the screen line content (128T).

    private List<BorderTick> _borderTickRanges = BuildBorderTickRanges(
        hardwareSettings.RetraceTicks, hardwareSettings.BorderTop);

    private int _lastRangeIndex;
    private int _offset;
    private Color _lastColor = SpectrumPalette.White;
    private ScreenMode _screenMode = ScreenMode.Spectrum;

    internal void ChangeScreenMode(ScreenMode screenMode)
    {
        if (_screenMode == screenMode)
        {
            return;
        }

        _borderTickRanges = BuildBorderTickRanges(
            hardwareSettings.RetraceTicks,
            hardwareSettings.BorderTop,
            screenMode == ScreenMode.TimexHiRes ? 2 * ScreenSize.ContentWidth : ScreenSize.ContentWidth);

        _screenMode = screenMode;
    }

    internal void Update(Color color) => _lastColor = color;

    /// <summary>
    /// Fill the border with the specified color up to the current tick. This fills the frame buffer with the
    /// current border color up to the current tick.
    /// </summary>
    /// <param name="color">The new color.</param>
    /// <param name="frameTicks">The current tick when the border color is changing.</param>
    internal void Update(Color color, int frameTicks)
    {
        for (var rangeIndex = _lastRangeIndex; rangeIndex < _borderTickRanges.Count; rangeIndex++)
        {
            var tickRange = _borderTickRanges[rangeIndex];

            if (frameTicks >= tickRange.StartTick)
            {
                if (frameTicks < tickRange.EndTick)
                {
                    var startPixel = tickRange.StartPixel + (_offset << tickRange.Shift);
                    var count = (frameTicks - (tickRange.StartTick + _offset) + 1) << tickRange.Shift;

                    frameBuffer.Fill(startPixel, count, _lastColor);

                    _offset = frameTicks - tickRange.StartTick;
                    _lastRangeIndex = rangeIndex;

                    break;
                }
                else
                {
                    var startPixel = tickRange.StartPixel + (_offset << tickRange.Shift);
                    var count = (tickRange.EndTick - (tickRange.StartTick + _offset) + 1) << tickRange.Shift;

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
    internal static List<BorderTick> BuildBorderTickRanges(int retraceTicks, int borderTop, int contentWidth = ScreenSize.ContentWidth)
    {
        var ticksTable = new List<BorderTick>();

        var startTick = 0;
        var startPixel = ScreenSize.BorderLeft;
        var totalLines = borderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;

        for (var line = 0; line < totalLines; line++)
        {
            // Top border
            if (line < borderTop)
            {
                AddFullBorderLine(ticksTable, line, startTick, startPixel, contentWidth);
                startTick += line == 0
                    ? ContentLineTicks + RightBorderTicks + retraceTicks
                    : LeftBorderTicks + ContentLineTicks + RightBorderTicks + retraceTicks;

                startPixel += line == 0
                    ? contentWidth + ScreenSize.BorderRight
                    : ScreenSize.BorderLeft + contentWidth + ScreenSize.BorderRight;
            }
            // Bottom border
            else if (line >= borderTop + ScreenSize.ContentHeight)
            {
                AddFullBorderLine(ticksTable, line, startTick, startPixel, contentWidth);
                startTick += LeftBorderTicks + ContentLineTicks + RightBorderTicks + retraceTicks;
                startPixel += ScreenSize.BorderLeft + contentWidth + ScreenSize.BorderRight;
            }
            else
            {
                // Left border
                var endTick = startTick + LeftBorderTicks;
                ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                // Skip content area
                startTick = endTick + ContentLineTicks;
                endTick = startTick + RightBorderTicks;
                startPixel += ScreenSize.BorderLeft + contentWidth;

                // Right border
                ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));
                startTick = endTick + retraceTicks;
                startPixel += ScreenSize.BorderRight;
            }
        }

        return ticksTable;
    }

    private static void AddFullBorderLine(List<BorderTick> ticksTable, int line, int startTick, int startPixel, int contentWidth)
    {
        // 1 = 2 pixels per tick (standard), 2 = 4 pixels per tick (hi-res content area)
        var contentShift = contentWidth > ScreenSize.ContentWidth ? 2 : 1;

        if (line == 0)
        {
            if (contentShift == 1)
            {
                ticksTable.Add(new BorderTick(
                    startTick,
                    startTick + ContentLineTicks + RightBorderTicks - 1,
                    startPixel));

                return;
            }

            // Content line
            ticksTable.Add(new BorderTick(
                startTick,
                startTick + ContentLineTicks - 1,
                startPixel,
                contentShift));

            // Right border
            ticksTable.Add(new BorderTick(
                startTick + ContentLineTicks,
                startTick + ContentLineTicks + RightBorderTicks - 1,
                startPixel + contentWidth));

            return;
        }

        if (contentShift == 1)
        {
            ticksTable.Add(new BorderTick(
                startTick,
                startTick + LeftBorderTicks + ContentLineTicks + RightBorderTicks - 1,
                startPixel));

            return;
        }

        // Left border
        ticksTable.Add(new BorderTick(
            startTick,
            startTick + LeftBorderTicks - 1,
            startPixel));

        // Content line
        ticksTable.Add(new BorderTick(
            startTick + LeftBorderTicks,
            startTick + LeftBorderTicks + ContentLineTicks - 1,
            startPixel + ScreenSize.BorderLeft,
            contentShift));

        // Right border
        ticksTable.Add(new BorderTick(
            startTick + LeftBorderTicks + ContentLineTicks,
            startTick + LeftBorderTicks + ContentLineTicks + RightBorderTicks - 1,
            startPixel + ScreenSize.BorderLeft + contentWidth));
    }
}