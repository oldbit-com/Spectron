using OldBit.Z80Cpu;
using OldBit.ZXTape.Tap;

namespace OldBit.Spectral.Emulator.Tape;

internal class TapePlayer(Clock clock)
{
    private readonly TapePulseCollection _tape = new();
    private int _currentPulseIndex;
    private int _runningPulseLength;
    private int _runningPulseCount;

    internal bool EarBit { get; set; }

    internal bool IsPlaying { get; set; }

    internal void Start()
    {
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
        _currentPulseIndex = 0;
        _runningPulseLength = 0;
        _runningPulseCount = 0;
        IsPlaying = false;
    }

    internal void LoadTape(TapFile tapFile)
    {
        _tape.Clear();

        foreach (var block in tapFile.Blocks)
        {
            if (block.IsHeader)
            {
                _tape.AddPilotHeaderPulses();
            }
            else
            {
                _tape.AddPilotDataPulses();
            }
            _tape.AddSyncPulses();

            _tape.AddDataPulses(block.Flag);
            _tape.AddDataPulses(block.Data);
            _tape.AddDataPulses(block.Checksum);
        }
    }

    private void ReadTape(int addedTicks, int previousFrameTicks, int currentFrameTicks)
    {
        if (!IsPlaying)
        {
            return;
        }

        var pulse = _tape.Pulses[_currentPulseIndex];
       _runningPulseLength += addedTicks;

       // If the pulse length is less than the current pulse length, then we need to wait for the next pulse
       if (_runningPulseLength < pulse.PulseLength)
       {
           return;
       }

       _runningPulseLength = 0;
       _runningPulseCount += 1;

       // If we have reached the pulse count, then move to the next pulse
       if (_runningPulseCount >= pulse.PulseCount)
       {
           _currentPulseIndex += 1;
           if (_currentPulseIndex >= _tape.Pulses.Count)
           {
               Stop();
               return;
           }

           _runningPulseCount = 0;
       }

       EarBit = !EarBit;
    }
}