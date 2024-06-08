namespace OldBit.ZXSpectrum.Emulator.Screen;

public class Border
{
    private readonly List<BorderState> _borderStates = [];
    private BorderState _lastBorderState = new(Colors.White);
    private BorderState? _nextBorderState;

    public void ChangeBorderColor(byte color, int clockCycle)
    {
        var borderColor = Colors.BorderColors[(byte)(color & 0x07)];
        if (_lastBorderState.Color == borderColor)
        {
            return;
        }

        _lastBorderState = new BorderState
        {
            Color = borderColor,
            ClockCycle = clockCycle,
            Index = _borderStates.Count
        };

        _borderStates.Add(_lastBorderState);
    }

    public Color GetBorderColor(int clockCycle)
    {
        if (_borderStates.Count == 0)
        {
            return _lastBorderState.Color;
        }

        var start = 0;
        if (_nextBorderState != null)
        {
            if (clockCycle < _nextBorderState.Value.ClockCycle)
            {
                return _lastBorderState.Color;
            }

            start = _nextBorderState.Value.Index;
        }

        for (var i = start; i < _borderStates.Count; i++)
        {
            if (_borderStates[i].ClockCycle > clockCycle)
            {
                _nextBorderState = _borderStates[i];
                break;
            }

            _lastBorderState = _borderStates[i];
        }

        return _lastBorderState.Color;
    }

    public void Reset()
    {
        if (_borderStates.Count > 0)
        {
            _lastBorderState = _borderStates[^1];
        }

        _borderStates.Clear();
        _nextBorderState = null;
    }
}