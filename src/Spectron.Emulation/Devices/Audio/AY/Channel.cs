namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class Channel
{
    private int _counter;
    private int _period  = 1;

    internal bool Tone { get; private set; }
    internal int Volume { get; set; }
    internal AudioSamples Samples { get; } = new();

    internal int Amplitude { get; set; }
    internal AmplitudeMode AmplitudeMode { get; set; }

    internal bool ToneDisabled { get; set; }
    internal bool NoiseDisabled { get; set; }

    internal void SetPeriod(byte fineTune, byte coarseTune) =>
        _period = ((coarseTune & 0x0F) << 8) | fineTune;

    internal void Update()
    {
        _counter += 1;

        if (_counter < _period)
        {
            return;
        }

        Tone = !Tone;
        _counter = 0;
    }

    internal void Reset()
    {
        _period = 1;
        _counter = 0;
        Tone = false;
        Volume = 0;
        Amplitude = 0;
        AmplitudeMode = AmplitudeMode.FixedLevel;
        ToneDisabled = false;
        NoiseDisabled = false;
    }
}