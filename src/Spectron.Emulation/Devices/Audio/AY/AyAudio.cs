using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

/// <summary>
/// AY audion chip emulation.
/// Based on JSpeccy implementation: https://github.com/jsanchezv/JSpeccy/blob/master/src/main/java/machine/AY8912.java
/// </summary>
internal sealed class AyAudio
{
    private const int Multiplier = 100_000; // used to avoid floating point calculations and rounding errors
    private const int AyCycles = 16;
    private const int ClockStep = Multiplier * AyCycles;

    private readonly long _statesPerSample;
    private readonly double _sampleRate;

    private int _ayTicks;
    private long _clockStepCounter;
    private readonly Clock _clock;
    private readonly AyDevice _ay;

    public AyAudio(Clock clock, AyDevice ay, double statesPerSample)
    {
        _clock = clock;
        _ay = ay;

        _statesPerSample = (long)(Multiplier * statesPerSample);
        _sampleRate = statesPerSample / AyCycles;

        _ay.OnUpdateAudio = () => Update(clock.FrameTicks);
    }

    internal void EndFrame()
    {
        Update(_clock.FrameTicks);

        _ay.ChannelA.Samples.Clear();
        _ay.ChannelB.Samples.Clear();
        _ay.ChannelC.Samples.Clear();

        _ayTicks -= _clock.FrameTicks;
    }

    internal void Reset() => _ay.Reset();

    private void Update(int ticks)
    {
        while (_ayTicks < ticks)
        {
            _ayTicks += 16;

            _ay.ChannelA.Update();
            _ay.ChannelB.Update();
            _ay.ChannelC.Update();
            _ay.Noise.Update();
            _ay.Envelope.Update();

            if (_ay.ChannelA.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _ay.SetAmplitudeUsingEnvelope(_ay.ChannelA);
            }

            if (_ay.ChannelB.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _ay.SetAmplitudeUsingEnvelope(_ay.ChannelB);
            }

            if (_ay.ChannelC.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _ay.SetAmplitudeUsingEnvelope(_ay.ChannelC);
            }

            var remaining = _statesPerSample - _clockStepCounter;
            _clockStepCounter += ClockStep;

            if (remaining > ClockStep)
            {
                if ((_ay.ChannelA.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelA.NoiseDisabled))
                {
                    _ay.ChannelA.Volume += _ay.ChannelA.Amplitude;
                }

                if ((_ay.ChannelB.Tone || _ay.ChannelB.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelB.NoiseDisabled))
                {
                    _ay.ChannelB.Volume += _ay.ChannelB.Amplitude;
                }

                if ((_ay.ChannelC.Tone || _ay.ChannelC.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelC.NoiseDisabled))
                {
                    _ay.ChannelC.Volume += _ay.ChannelC.Amplitude;
                }
            }
            else
            {
                var percent = remaining / (double)ClockStep;
                int lastA = 0, lastB = 0, lastC = 0;

                if ((_ay.ChannelA.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelA.NoiseDisabled))
                {
                    lastA = (int)(_ay.ChannelA.Amplitude * percent);
                }

                if ((_ay.ChannelB.Tone || _ay.ChannelB.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelB.NoiseDisabled))
                {
                    lastB = (int)(_ay.ChannelB.Amplitude * percent);
                }

                if ((_ay.ChannelC.Tone || _ay.ChannelC.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelC.NoiseDisabled))
                {
                    lastC = (int)(_ay.ChannelC.Amplitude * percent);
                }

                _clockStepCounter -= _statesPerSample;

                _ay.ChannelA.Samples.Add((int)((_ay.ChannelA.Volume + lastA) / _sampleRate));
                _ay.ChannelB.Samples.Add((int)((_ay.ChannelB.Volume + lastB) / _sampleRate));
                _ay.ChannelC.Samples.Add((int)((_ay.ChannelC.Volume + lastC) / _sampleRate));

                _ay.ChannelA.Volume = lastA > 0 ? _ay.ChannelA.Amplitude - lastA : 0;
                _ay.ChannelB.Volume = lastB > 0 ? _ay.ChannelB.Amplitude - lastB : 0;
                _ay.ChannelC.Volume = lastC > 0 ? _ay.ChannelC.Amplitude - lastC : 0;
            }
        }
    }
}