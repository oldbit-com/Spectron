namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class Channel
{
    private int _counter;

    internal int Period { get; private set; } = 1;
    internal bool Tone { get; set; }
    internal int Amplitude { get; set; }
    internal AmplitudeMode AmplitudeMode { get; set; }
    internal bool IsToneDisabled { get; set; }
    internal bool IsNoiseDisabled { get; set; }

    internal void SetPeriod(byte fineTune, byte coarseTune) =>
        Period = ((coarseTune & 0x0F) << 8) | fineTune;

    internal void Update()
    {
        _counter += 1;

        if (_counter < Period)
        {
            return;
        }

        Tone = !Tone;
        _counter = 0;
    }

    internal void Reset()
    {
        Period = 1;
        _counter = 0;
        Tone = false;
        Amplitude = 0;
        AmplitudeMode = AmplitudeMode.FixedLevel;
        IsToneDisabled = false;
        IsNoiseDisabled = false;
    }
}