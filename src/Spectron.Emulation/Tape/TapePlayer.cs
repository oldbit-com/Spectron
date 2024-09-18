using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Simulates the tape player that converts tape data into pulses
/// that can be read by the ZX Spectrum by reading the EAR port signals.
/// </summary>
internal sealed class TapePlayer(Clock clock, HardwareSettings hardware) : IDisposable
{
    private PulseStream? _pulseStream;

    private int _runningPulseDuration;
    private int _runningPulseCount;

    internal bool EarBit { get; private set; }
    internal bool IsPlaying { get; private set; }

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
        _runningPulseDuration = 0;
        _runningPulseCount = 0;

        IsPlaying = false;
    }

    internal void LoadTape(VirtualTape virtualTape)
    {
        Close();

        _pulseStream = new PulseStream(virtualTape, hardware);
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
        _runningPulseDuration = 0;
        _runningPulseCount = 0;

        _pulseStream?.Dispose();
    }

    public void Dispose() => Close();
}
