namespace OldBit.ZXSpectrum.Emulator.Screen;

public record struct BorderTick(int StartTick, int EndTick, int StartPixel);

public class BorderRenderer(ScreenBuffer screenBuffer)
{
    private readonly List<BorderTick> _borderTickRanges = BuildBorderTickRanges();

    private int _lastRangeIndex;
    private int _offset;
    private Color _lastColor = Colors.White;

    public void Update(Color color) => _lastColor = color;

    /// <summary>
    /// Fill the border with the specified color uo to the current tick.
    /// </summary>
    /// <param name="color">The new color.</param>
    /// <param name="currentTicks">The current tick when border color is changing.</param>
    public void Update(Color color, int currentTicks)
    {
        for (var rangeIndex = _lastRangeIndex; rangeIndex < _borderTickRanges.Count; rangeIndex++)
        {
            var tickRange = _borderTickRanges[rangeIndex];
            if (currentTicks >= tickRange.StartTick)
            {
                if (currentTicks < tickRange.EndTick)
                {
                    var startPixel = tickRange.StartPixel + 2 * _offset;
                    var count = 2 * (currentTicks - (tickRange.StartTick + _offset) + 1);

                    screenBuffer.Fill(startPixel, count, _lastColor);

                    _offset = currentTicks - tickRange.StartTick + 1;
                    _lastRangeIndex = rangeIndex;

                    break;
                }
                else
                {
                    var startPixel = tickRange.StartPixel + 2 * _offset;
                    var count = 2 * (tickRange.EndTick - (tickRange.StartTick + _offset) + 1);

                    screenBuffer.Fill(startPixel, count, _lastColor);

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

    public void NewFrame()
    {
        _lastRangeIndex = 0;
        _offset = 0;
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
        var endTick = Constants.ContentTicks + Constants.LeftBorderTicks;
        var startPixel = Constants.BorderLeft;

        for (var line = 0; line < Constants.TotalLines; line++)
        {
            switch (line)
            {
                case < Constants.BorderTop:
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + Constants.RetraceTicks;
                    endTick = startTick + Constants.LeftBorderTicks + Constants.ContentTicks + Constants.RightBorderTicks;
                    startPixel += line == 0 ?
                        Constants.ContentWidth + Constants.BorderRight :
                        Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight;

                    break;

                case >= Constants.BorderTop + Constants.ContentHeight:
                    endTick = startTick + Constants.LeftBorderTicks + Constants.ContentTicks + Constants.RightBorderTicks;

                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    startTick = endTick + Constants.RetraceTicks;
                    startPixel += Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight;

                    break;

                default:
                    // Left border
                    endTick = startTick + Constants.LeftBorderTicks;
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));

                    // Skip content area
                    startTick = endTick + Constants.ContentTicks;
                    endTick = startTick + Constants.RightBorderTicks;
                    startPixel += Constants.BorderLeft + Constants.ContentWidth;

                    // Right border
                    ticksTable.Add(new BorderTick(startTick, endTick - 1, startPixel));
                    startTick = endTick + Constants.RetraceTicks;
                    startPixel += Constants.BorderRight;

                    break;
            }
        }

        return ticksTable;
    }
}