namespace OldBit.ZXSpectrum.Emulator.Screen;

public class Border
{
    private readonly List<BorderState> _borderStates = [];
    private BorderState _lastBorderState = new();

    // 0, 1234, 4593, 8374, 12345, 123456
    // Hash 0 - 1234
    // Hash 1234 - 4593
    // Hash 4593 - 8374


    public void ChangeBorderColor(byte color, int clockCycle)
    {
        var borderColor = Colors.BorderColors[(byte)(color & 0x07)];
        if (_lastBorderState.Color == borderColor)
        {
            return;
        }

        _lastBorderState.Color = borderColor;
        _lastBorderState.ClockCycle = clockCycle;

        _borderStates.Add(new BorderState
        {
            Color = borderColor,
            ClockCycle = clockCycle,
            Index = _borderStates.Count
        });
    }

    public Color GetBorderColor(int clockCycle)
    {
        if (_borderStates.Count == 0)
        {
            return _lastBorderState.Color;
        }

        // TODO: Binary search or something more efficient

        return _lastBorderState.Color;
    }

    public void Reset()
    {
        if (_borderStates.Count > 0)
        {
            _lastBorderState = _borderStates[^1];
        }

        _borderStates.Clear();
    }
}