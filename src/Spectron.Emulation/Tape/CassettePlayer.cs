using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Simulates the tape player that converts tape data into pulses
/// that can be read by the ZX Spectrum by reading the EAR port signals.
/// </summary>
internal sealed class CassettePlayer(Cassette cassette, Clock clock, HardwareSettings hardware) : IDisposable
{
    private PulseStream? _pulseStream;
    private Cassette _cassette = cassette;

    private int _runningPulseDuration;
    private int _runningPulseCount;

    internal bool EarBit { get; private set; }
    internal bool IsPlaying { get; private set; }
    internal double BlockReadProgressPercentage => _pulseStream?.BlockReadProgressPercentage ?? 0;

    internal void Play()
    {
        if (IsPlaying)
        {
            return;
        }

        clock.TicksAdded -= ReadTapePulses;
        clock.TicksAdded += ReadTapePulses;

        _pulseStream?.Start();

        IsPlaying = true;
    }

    internal void Stop()
    {
        clock.TicksAdded -= ReadTapePulses;

        IsPlaying = false;
    }

    internal void Rewind()
    {
        _cassette.Rewind();
        LoadTape(_cassette);
    }

    internal void LoadTape(Cassette cassette)
    {
        _cassette = cassette;

        Close();

        _pulseStream = new PulseStream(cassette, hardware);
    }

    private void ReadTapePulses(int addedTicks, int previousFrameTicks, int currentFrameTicks)
    {
        if (!IsPlaying || _pulseStream == null)
        {
            return;
        }

        var pulse = _pulseStream.Current;

        if (pulse == null)
        {
            Stop();
            return;
        }

        _runningPulseDuration += addedTicks;

        // If the pulse length is less than the current pulse length, then we need to wait for the next pulse
        if (_runningPulseDuration < pulse.Duration)
        {
            return;
        }

        _runningPulseDuration = 0;
        _runningPulseCount += 1;

        // If we have reached the pulse count, then move to the next pulse
        if (_runningPulseCount >= pulse.RepeatCount)
        {
            if (!_pulseStream.Next())
            {
                Stop();
                return;
            }

            _runningPulseCount = 0;
        }

        if (pulse.IsSilence)
        {
            EarBit = false;
            return;
        }

        EarBit = !EarBit;
    }

    private void Close()
    {
        IsPlaying = false;

        _runningPulseDuration = 0;
        _runningPulseCount = 0;

        _pulseStream?.Dispose();
    }

    public void Dispose() => Close();
}
