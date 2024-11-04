namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class Noise
{
    private int _period  = 1;
    private int _counter;
    private byte _periodRegister;
    private int _rng = 1;

    internal bool Tone { get; private set; }

    internal void SetPeriod(byte period) => _periodRegister = (byte)(period & 0x1F);

    internal void Update()
    {
        _counter += 1;

        if (_counter < _period)
        {
            return;
        }

        _counter = 0;

        _period = _periodRegister;
        if (_period == 0)
        {
            _period = 1;
        }
        _period <<= 1;

        if (((_rng + 1) & 0x02) != 0)
        {
            Tone = !Tone;
        }

        if ((_rng & 0x01) != 0)
        {
            _rng ^= 0x24000;
        }
        _rng >>>= 1;
    }

    internal void Reset()
    {
        _period = 1;
        _periodRegister = 0;
        _counter = 0;
        _rng = 1;
        Tone = false;
    }
}
