namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class Noise
{
    private int _counter;
    private byte _periodRegister;
    private int _rng = 1;

    internal int Period { get; private set; } = 1;
    internal bool Tone { get; set; }

    internal void SetPeriod(byte period) => _periodRegister = (byte)(period & 0x1F);

    internal void Update()
    {
        _counter += 1;

        if (_counter < Period)
        {
            return;
        }

        _counter = 0;

        Period = _periodRegister;
        if (Period == 0)
        {
            Period = 1;
        }
        Period <<= 1;

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
        Period = 1;
        _counter = 0;
        Tone = false;
        _periodRegister = 0;
        _rng = 1;
    }
}
