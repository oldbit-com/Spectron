using OldBit.Spectral.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulation.Tape;

/// <summary>
/// Simulates a tape player that converts tape data into pulses that can be read by the Spectrum.
/// </summary>
internal sealed class TapePlayer(Clock clock, HardwareSettings hardware) : IDisposable
{
    private IEnumerator<Pulse>? _pulses;
    private IEnumerator<TapData>? _tapeBlocks;

    private int _runningPulseDuration;
    private int _runningPulseCount;

    internal TzxFile? TzxFile { get; private set; }
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

    internal void LoadTape(TzxFile tzxFile)
    {
        Close();

        TzxFile = tzxFile;
        var tapePulseProvider = new PulseProvider(tzxFile, hardware);

        _pulses = tapePulseProvider.GetAll().GetEnumerator();
        _pulses.MoveNext();

        _tapeBlocks = GetTapBlocs(tzxFile);

    }

    internal TapData? NextBlock()
    {
        if (_tapeBlocks == null || !_tapeBlocks.MoveNext())
        {
            return null;
        }

        return _tapeBlocks.Current;
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

    private static IEnumerator<TapData> GetTapBlocs(TzxFile tzxFile)
    {
        foreach (var block in tzxFile.Blocks.GetStandardSpeedDataBlocks())
        {
            if (TapData.TryParse(block.Data, out var tapData))
            {
                yield return tapData;
            }
        }
    }

    private void Close()
    {
        TzxFile = null;
        _runningPulseDuration = 0;
        _runningPulseCount = 0;

        _pulses?.Dispose();
        _tapeBlocks?.Dispose();
    }

    public void Dispose() => Close();
}
