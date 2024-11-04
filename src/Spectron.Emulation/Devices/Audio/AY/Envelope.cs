namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

/// <summary>
///     \_________    00xx
///     /|________    01xx
///     \|\|\|\|\|    1000
///     \_________    1001
///     \|‾‾‾‾‾‾‾‾    1011
///     /|/|/|/|/|    1100
///     /‾‾‾‾‾‾‾‾     1101
///     /\/\/\/\/\    1110
///     /|________    1111
/// </summary>
internal sealed class Envelope
{
    private int _counter;
    private bool _first;
    private int _period;

    private bool _hold;
    private bool _alternate;
    private bool _attack;
    private bool _continue;

    internal int Amplitude { get; private set; }

    internal void SetPeriod(byte fineTune, byte coarseTune)
    {
        _period = (coarseTune << 8) | fineTune;
        _period = _period == 0 ? 2 : _period << 1;
    }

    internal void SetShape(byte shape)
    {
        _hold = (shape & 0x01) != 0;
        _alternate = (shape & 0x02) != 0;
        _attack = (shape & 0x04) != 0;
        _continue = (shape & 0x08) != 0;

        Amplitude = _attack ? 0 : 15;
        _counter = 0;
        _first = true;
    }

    internal void Update()
    {
        _counter += 1;
        if (!_first || _counter < _period)
        {
            return;
        }

        _counter = 0;
        Amplitude = _attack ? Amplitude + 1 : Amplitude - 1;

        if ((Amplitude & 0x10) == 0)
        {
            return;
        }

        if (_continue)
        {
            if (_alternate)
            {
                _attack = !_attack;
            }

            if (_hold)
            {
                Amplitude = _attack ? 15 : 0;
                _first = false;
            }
            else
            {
                Amplitude = _attack ? 0 : 15;
            }
        }
        else
        {
            Amplitude = 0;
            _first = false;
        }
    }

    internal void Reset()
    {
        _first = false;
        _attack = false;
        _period = 0;
        Amplitude = 0;
    }
}

