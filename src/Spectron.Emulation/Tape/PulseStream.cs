using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// A pulse represents a change in the signal level (lo->hi or hi->lo, e.g. toggle).
/// </summary>
/// <param name="RepeatCount">How many times pulse change should be repeated.</param>
/// <param name="Duration">The pulse duration in T-states.</param>
/// <param name="IsSilence">Used to add a delay between blocks.</param>
internal sealed record Pulse(int RepeatCount, int Duration, bool IsSilence = false);

/// <summary>
/// Memory efficient tape pulse stream. Typically, a tape file is represented by thousands of pulses,
/// This class generates them on the fly, e.g. when the tape is played.
/// </summary>
internal class PulseStream : IDisposable
{
    private readonly Cassette _cassette;
    private readonly HardwareSettings _hardware;

    private IEnumerator<Pulse>? _pulseEnumerator;
    private int _loopCounter;

    internal int DataPulseCount { get; private set; }

    internal PulseStream(Cassette cassette, HardwareSettings hardware)
    {
        _cassette = cassette;
        _hardware = hardware;
    }

    internal void Start() => _pulseEnumerator = GetNextEnumerator();

    internal Pulse? Current => _pulseEnumerator?.Current;

    internal bool Next()
    {
        if (_pulseEnumerator == null)
        {
            return false;
        }

        if (_pulseEnumerator.MoveNext())
        {
            return true;
        }

        _pulseEnumerator.Dispose();
        _pulseEnumerator = GetNextEnumerator();

        return _pulseEnumerator != null;
    }

    private IEnumerator<Pulse>? GetNextEnumerator()
    {
        while (true)
        {
            var block = _cassette.GetNextBlock();

            switch (block)
            {
                case null:
                    return null;

                case LoopStartBlock loopStartBlock:
                    _loopCounter = loopStartBlock.Count;
                    _cassette.SetMarker();

                    break;

                case LoopEndBlock:
                {
                    _loopCounter -= 1;

                    if (_loopCounter > 0)
                    {
                        _cassette.GotoMarker();
                        continue;
                    }

                    break;
                }
            }

            DataPulseCount = 0;
            _pulseEnumerator = GetNextBlockPulses(block).GetEnumerator();

            if (!_pulseEnumerator.MoveNext())
            {
                // Skip empty blocks or blocks that don't produce pulses.
                continue;
            }

            return _pulseEnumerator;
        }
    }

    private IEnumerable<Pulse> GetNextBlockPulses(IBlock block)
    {
        switch (block)
        {
            case PauseBlock pauseBlock:
                var pausePulse = PulseFactory.CreatePausePulse(pauseBlock.Duration, _hardware);
                if (pausePulse != null)
                {
                    return [pausePulse];
                }
                break;

            case PureToneBlock pureToneBlock:
                return [PulseFactory.Create(pureToneBlock)];

            case PulseSequenceBlock pulseSequenceBlock:
                return PulseFactory.Create(pulseSequenceBlock);

            case PureDataBlock pureDataBlock:
                return GetPureDataBlockPulses(pureDataBlock, _hardware);

            case StandardSpeedDataBlock standardSpeedDataBlock:
                return GetStandardSpeedDataBlockPulses(standardSpeedDataBlock, _hardware);

            case TurboSpeedDataBlock turboSpeedDataBlock:
                return GetTurboSpeedDataBlockPulses(turboSpeedDataBlock, _hardware);
        }

        return [];
    }

    private IEnumerable<Pulse> GetStandardSpeedDataBlockPulses(StandardSpeedDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return !TapData.TryParse(block.Data, out var tapData) ? [] : GetTapeDataPulses(pulseSettings, tapData.IsHeader);
    }

    private IEnumerable<Pulse> GetTurboSpeedDataBlockPulses(TurboSpeedDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return !TapData.TryParse(block.Data, out var tapData) ? [] : GetTapeDataPulses(pulseSettings, tapData.IsHeader);
    }

    private IEnumerable<Pulse> GetPureDataBlockPulses(PureDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return GetTapeDataPulses(pulseSettings);
    }

    private IEnumerable<Pulse> GetTapeDataPulses(BlockPulses blockPulses, bool isHeader = false)
    {
        if (isHeader)
        {
            if (blockPulses.PilotHeaderPulse != null)
            {
                yield return blockPulses.PilotHeaderPulse;
            }
        }
        else
        {
            if (blockPulses.PilotDataPulse != null)
            {
                yield return blockPulses.PilotDataPulse;
            }
        }

        if (blockPulses.FirstSyncPulse != null)
        {
            yield return blockPulses.FirstSyncPulse;
        }

        if (blockPulses.SecondSyncPulse != null)
        {
            yield return blockPulses.SecondSyncPulse;
        }

        foreach (var pulse in blockPulses.Data.Pulses)
        {
            DataPulseCount += 1;
            yield return pulse;
        }

        if (blockPulses.PausePulse != null)
        {
            yield return blockPulses.PausePulse;
        }
    }

    public void Dispose()
    {
        _pulseEnumerator?.Dispose();
    }
}