namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal sealed class LowPassFilter
{
    private const double CutoffFreq = 5000.0;
    private const double Q = 0.1; // Butterworth response

    // Biquad filter state
    private float _z1, _z2;
    private readonly float _b0, _b1, _b2, _a1, _a2;

    public LowPassFilter(double statesPerSample, float clockFrequency)
    {
        var sampleRate = 1.0 / (statesPerSample / clockFrequency);

        var w0 = 2 * Math.PI * CutoffFreq / sampleRate;
        var cosW0 = Math.Cos(w0);
        var sinW0 = Math.Sin(w0);
        var alpha = sinW0 / (2.0 * Q);

        var a0 = 1.0 + alpha;
        _b0 = (float)((1.0 - cosW0) / (2.0 * a0));
        _b1 = (float)((1.0 - cosW0) / a0);
        _b2 = (float)((1.0 - cosW0) / (2.0 * a0));
        _a1 = (float)(-2.0 * cosW0 / a0);
        _a2 = (float)((1.0 - alpha) / a0);
    }

    internal void Apply(ref short sample)
    {
        var output = _b0 * sample + _z1;
        _z1 = _b1 * sample - _a1 * output + _z2;
        _z2 = _b2 * sample - _a2 * output;

        sample = (short)output;
    }

    internal void Reset()
    {
        _z1 = 0;
        _z2 = 0;
    }
}