using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal sealed class BeeperAudio
{
    private const float Alpha = 0.6f;
    private const int Multiplier = 1000;  // Used to avoid floating point arithmetic and rounding errors

    private byte _lastEarMic;
    private short _previousSample;
    private int _remainingTicks;

    private readonly Clock _clock;
    private readonly int _statesPerSample;
    private readonly BeeperStates _beeperStates = new();

    private const int Volume = 24000;
    private readonly short[] _volumeLevels =
    [
        0,         // -Volume,
        0,         // (short)(0.66f / 3.70f * Volume),
        -(short)(3.56f / 3.70f * Volume),
        -Volume
    ];

    internal AudioSamples Samples { get; } = new();

    internal BeeperAudio(Clock clock, double statesPerSample)
    {
        _clock = clock;
        _statesPerSample = (int)(Multiplier * statesPerSample);
    }

    internal void NewFrame() => Samples.Clear();

    internal void EndFrame()
    {
        var runningTicks = 0;

        var ticks = _beeperStates.Count == 0 ? _clock.CurrentFrameTicks : _beeperStates[0].Ticks;
        var duration = ticks * Multiplier;

        for (var i = 0; i <= _beeperStates.Count; i++)
        {
            runningTicks += duration;
            duration += _remainingTicks;

            while (duration >= _statesPerSample)
            {
                duration -= _statesPerSample;

                // Get sample and apply simple low-pass filter to make edges smoother
                var sample = i < _beeperStates.Count ? _volumeLevels[_beeperStates[i].EarMic] : _volumeLevels[_lastEarMic];
                sample = (short)(Alpha * sample + (1 - Alpha) * _previousSample);
                _previousSample = sample;

                Samples.Add(sample);
            }

            _remainingTicks = duration;

            if (i == _beeperStates.Count)
            {
                break;
            }

            ticks = i == _beeperStates.Count - 1 ? _clock.CurrentFrameTicks : _beeperStates[i + 1].Ticks;
            duration = ticks * Multiplier - runningTicks;
        }

        _beeperStates.Reset();
    }

    internal void Update(byte value)
    {
        var earMic = (value >> 3) & 0x03;

        if (_lastEarMic != earMic)
        {
            _beeperStates.Add(_clock.CurrentFrameTicks, _lastEarMic);
        }

        _lastEarMic = (byte)earMic;
    }

    internal void Reset()
    {
        _beeperStates.Reset();
        _remainingTicks = 0;
        _lastEarMic = 0;
        _previousSample = 0;
    }
}
