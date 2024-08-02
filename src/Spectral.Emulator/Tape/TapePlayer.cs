using OldBit.Z80Cpu;
using OldBit.ZXTape.Tap;

namespace OldBit.Spectral.Emulator.Tape;

internal class TapePlayer(Clock clock) : IDisposable
{
    private IEnumerator<Pulse>? _pulses;
    private int _runningPulseDuration;
    private int _runningPulseCount;

    internal bool EarBit { get; private set; }
    internal bool IsPlaying { get; private set; }

    internal void Start()
    {
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

    internal void LoadTape(TapFile tapFile)
    {
        var tapePulseProvider = new PulseProvider(tapFile);

        _pulses = tapePulseProvider.GetPulses().GetEnumerator();
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

       EarBit = !EarBit;
    }

    public void Dispose()
    {
        _pulses?.Dispose();
    }
}
