using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;


/// <summary>
/// AY audion chip emulation.
/// Implementation based on JSpeccy code, e.g. copied the logic how AY audio is generated.
/// https://github.com/jsanchezv/JSpeccy/blob/master/src/main/java/machine/AY8912.java
/// </summary>
internal sealed class AyAudio
{
    private const int Multiplier = 100_000;

    private readonly long _statesPerSample;
    private readonly double _sampleRate;

    private int _audioTicks;
    private long stepCounter;
    private readonly Clock _clock;
    private readonly AyDevice _ay;

    public AyAudio(Clock clock, AyDevice ay, double statesPerSample)
    {
        _clock = clock;
        _ay = ay;

        _statesPerSample = (long)(Multiplier * statesPerSample);
        _sampleRate = statesPerSample / 16.0;
        _ay.OnUpdateAudio = () => Update(clock.FrameTicks);
    }

    internal void EndFrame()
    {
        Update(_clock.FrameTicks);

        _ay.ChannelA.Samples.Clear();
        _ay.ChannelB.Samples.Clear();
        _ay.ChannelC.Samples.Clear();

        _audioTicks -= _clock.FrameTicks;
    }

    internal void Reset() => _ay.Reset();

    private void Update(int ticks)
    {
        while (_audioTicks < ticks)
        {
            _audioTicks += 16;

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

            long diff = _statesPerSample - stepCounter;
            stepCounter += Multiplier * 16;

            if (diff > Multiplier * 16)
            {
                if ((_ay.ChannelA.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelA.NoiseDisabled))
                {
                    _ay.ChannelA.Volume += _ay.ChannelA.Amplitude;
                }

                if ((_ay.ChannelB.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelB.NoiseDisabled))
                {
                    _ay.ChannelB.Volume += _ay.ChannelB.Amplitude;
                }

                if ((_ay.ChannelC.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelC.NoiseDisabled))
                {
                    _ay.ChannelC.Volume += _ay.ChannelB.Amplitude;
                }
            }
            else
            {
                var percent = diff / (double)Multiplier * 16;
                int lastA = 0, lastB = 0, lastC = 0;

                if ((_ay.ChannelA.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelA.NoiseDisabled))
                {
                    lastA = (int)(_ay.ChannelA.Amplitude * percent);
                }

                if ((_ay.ChannelB.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelB.NoiseDisabled))
                {
                    lastB = (int)(_ay.ChannelB.Amplitude * percent);
                }

                if ((_ay.ChannelC.Tone || _ay.ChannelA.ToneDisabled) && (_ay.Noise.Tone || _ay.ChannelC.NoiseDisabled))
                {
                    lastC = (int)(_ay.ChannelC.Amplitude * percent);
                }

                stepCounter -= _statesPerSample;

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