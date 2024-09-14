using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Simulates a tape player that converts tape data into pulses that can be read by the Spectrum.
/// </summary>
internal sealed class TapePlayer(Clock clock, HardwareSettings hardware) : IDisposable
{
    private IEnumerator<Pulse>? _pulses;

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

        clock.TicksAdded -= ReadTape;
        clock.TicksAdded += ReadTape;

        IsPlaying = true;
    }

    internal void Stop()
    {
        clock.TicksAdded -= ReadTape;

        IsPlaying = false;
    }

    internal void Rewind()
    {
        _runningPulseDuration = 0;
        _runningPulseCount = 0;

        IsPlaying = false;
    }

    internal void LoadTape(TapeFile tape)
    {
        Close();

        var pulseProvider = new PulseProvider(tape, hardware);

        _pulses = pulseProvider.GetAllPulses().GetEnumerator();
        _pulses.MoveNext();
    }

    private void ReadTape(int addedTicks, int previousFrameTicks, int currentFrameTicks)
    {
        if (!IsPlaying || _pulses == null)
        {
            return;
        }

        var pulse = _pulses.Current;
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
           if (!_pulses.MoveNext())
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

        _pulses?.Dispose();
    }

    public void Dispose() => Close();
}
