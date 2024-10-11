using OldBit.Beep;

namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class Beeper
{
    private const float Alpha = 0.6f;

    private const int Multiplier = 1000;         // Used to avoid floating point arithmetic and rounding errors
    private const int FramesPerSecond = 50;

    private byte _lastEarMic;
    private bool _isMuted;
    private short _previousSample;

    private readonly int _statesPerSample;
    private readonly BeeperStates _beeperStates = new();
    private readonly BeeperSamplesPool _beeperSamplesPool;

    private const int Volume = 24000;
    private readonly short[] _volumeLevels =
    [
        -Volume,
        (short)(0.66f / 3.70f * Volume),
        (short)(3.56f / 3.70f * Volume),
        Volume
    ];

    internal bool IsEnabled { get; set; }

    internal Beeper(HardwareSettings hardware, int playerSampleRate, int bufferCount)
    {
        var samplesPerFrame = playerSampleRate / FramesPerSecond;
        var ticksPerFrame = hardware.TicksPerFrame;

        _beeperSamplesPool = new BeeperSamplesPool(bufferCount);
        _statesPerSample = Multiplier * ticksPerFrame / samplesPerFrame;
    }

    internal BeeperSamples? EndFrame(int frameTicks)
    {
        if (!IsEnabled || _isMuted)
        {
            return null;
        }

        var runningTicks = 0;
        var remainingTicks = 0;

        var ticks = _beeperStates.Count == 0 ? frameTicks : _beeperStates[0].Ticks;
        var duration = ticks * Multiplier;

        var samplesBuffer = _beeperSamplesPool.GetBuffer();

        for (var i = 0; i <= _beeperStates.Count; i++)
        {
            runningTicks += duration;
            duration += remainingTicks;

            while (duration >= _statesPerSample)
            {
                duration -= _statesPerSample;

                // Get sample and apply simple low-pass filter to make edges smoother
                var sample = i < _beeperStates.Count ? _volumeLevels[_beeperStates[i].EarMic] : _volumeLevels[_lastEarMic];
                sample = (short)(Alpha * sample + (1 - Alpha) * _previousSample);
                _previousSample = sample;

                samplesBuffer.Add(sample);
            }

            remainingTicks = duration;

            if (i == _beeperStates.Count)
            {
                break;
            }

            ticks = i == _beeperStates.Count - 1 ? frameTicks : _beeperStates[i + 1].Ticks;
            duration = ticks * Multiplier - runningTicks;
        }

        _beeperStates.Reset();

        return samplesBuffer;
    }

    internal void Update(int frameTicks, byte value)
    {
        if (!IsEnabled || _isMuted)
        {
            return;
        }

        var earMic = (value >> 3) & 0x03;

        if (_lastEarMic != earMic)
        {
            _beeperStates.Add(frameTicks, _lastEarMic);
        }

        _lastEarMic = (byte)earMic;
    }

    internal void Reset() => _beeperStates.Reset();

    internal void Stop() => _beeperStates.Reset();

    internal void Mute()
    {
        _isMuted = true;
        _beeperStates.Reset();
    }

    internal void UnMute() => _isMuted = false;
}
