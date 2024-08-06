namespace OldBit.Spectral.Emulation.Screen;

public record struct BorderTick(int StartTick, int EndTick, int StartPixel);

internal class Border(FrameBuffer frameBuffer)
{
    private readonly List<BorderTick> _borderTickRanges = BuildBorderTickRanges();

    private int _lastRangeIndex;
    private int _offset;
    private Color _lastColor = Colors.White;

    internal void Update(Color color) => _lastColor = color;

    /// <summary>
    /// Fill the border with the specified color uo to the current tick.
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

                    _offset = frameTicks - tickRange.StartTick + 1;
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

    internal void NewFrame()
    {
        _lastRangeIndex = 0;
        _offset = 0;
    }

    internal void Reset()
    {
        _lastColor = Colors.White;
        NewFrame();
    }

    /// <summary>
    /// Builds a lookup table for border ticks so that we can quickly determine the border range and
    /// pixel position for a given tick.
    /// </summary>
    /// <returns>A list of border ticks.</returns>
    internal static List<BorderTick> BuildBorderTickRanges()
    {
        var ticksTable = new List<BorderTick>();

        var startTick = 0;
        var endTick = DefaultTimings.ContentLineTicks + DefaultTimings.LeftBorderTicks;
        var startPixel = ScreenSize.BorderLeft;

        for (var line = 0; line < ScreenSize.TotalLines; line++)
        {
            switch (line)
            {
                case < ScreenSize.BorderTop:
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + DefaultTimings.RetraceTicks;
                    endTick = startTick + DefaultTimings.LeftBorderTicks + DefaultTimings.ContentLineTicks + DefaultTimings.RightBorderTicks;
                    startPixel += line == 0 ?
                        ScreenSize.ContentWidth + ScreenSize.BorderRight :
                        ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

                    break;

                case >= ScreenSize.BorderTop + ScreenSize.ContentHeight:
                    endTick = startTick + DefaultTimings.LeftBorderTicks + DefaultTimings.ContentLineTicks + DefaultTimings.RightBorderTicks;

                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + DefaultTimings.RetraceTicks;
                    startPixel += ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

                    break;

                default:
                    // Left border
                    endTick = startTick + DefaultTimings.LeftBorderTicks;
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    // Skip content area
                    startTick = endTick + DefaultTimings.ContentLineTicks;
                    endTick = startTick + DefaultTimings.RightBorderTicks;
                    startPixel += ScreenSize.BorderLeft + ScreenSize.ContentWidth;

                    // Right border
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));
                    startTick = endTick + DefaultTimings.RetraceTicks;
                    startPixel += ScreenSize.BorderRight;

                    break;
            }
        }

        return ticksTable;
    }
}