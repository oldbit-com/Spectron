using OldBit.Spectron.Emulation.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// A pulse represents a change in the signal level (lo->hi or hi->lo, e.g. toggle).
/// </summary>
/// <param name="RepeatCount">How many times pulse change should be repeated.</param>
/// <param name="Duration">The pulse duration in T-states.</param>
/// <param name="IsSilence">Used to add a delay between blocks.</param>
internal sealed record Pulse(int RepeatCount, int Duration, bool IsSilence = false);

/// <summary>
/// Memory efficient tape pulse provider. Normally there are many thousands of pulses in a file,
/// so we don't want to store them all in memory, just generate them on the fly.
/// </summary>
internal sealed class PulseProvider(ITapeBlockDataProvider blockDataProvider, HardwareSettings hardware)
{
    internal IEnumerable<Pulse> GetAllPulses()
    {
        while (true)
        {
            var block = blockDataProvider.GetNextBlock();

            if (block == null)
            {
                yield break;
            }

            IEnumerable<Pulse> pulses = [];

            switch (block)
            {
                case PauseBlock pauseBlock:
                    var pausePulse = PulseFactory.CreatePausePulse(pauseBlock.Duration, hardware);
                    if (pausePulse != null)
                    {
                        yield return pausePulse;
                    }
                    continue;

                case PureToneBlock pureToneBlock:
                    yield return PulseFactory.Create(pureToneBlock);
                    continue;

                case PulseSequenceBlock pulseSequenceBlock:
                    pulses = PulseFactory.Create(pulseSequenceBlock);
                    break;

                case PureDataBlock pureDataBlock:
                    pulses = GetPureDataBlockPulses(pureDataBlock, hardware);
                    break;

                case StandardSpeedDataBlock standardSpeedDataBlock:
                    pulses = GetStandardSpeedDataBlockPulses(standardSpeedDataBlock, hardware);
                    break;

                case TurboSpeedDataBlock turboSpeedDataBlock:
                    pulses = GetTurboSpeedDataBlockPulses(turboSpeedDataBlock, hardware);
                    break;
            }

            foreach (var pulse in pulses)
            {
                yield return pulse;
            }
        }
    }

    private static IEnumerable<Pulse> GetStandardSpeedDataBlockPulses(StandardSpeedDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return !TapData.TryParse(block.Data, out var tapData) ? [] : GetTapeDataPulses(tapData, pulseSettings);
    }

    private static IEnumerable<Pulse> GetTurboSpeedDataBlockPulses(TurboSpeedDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return !TapData.TryParse(block.Data, out var tapData) ? [] : GetTapeDataPulses(tapData, pulseSettings);
    }

    private static IEnumerable<Pulse> GetPureDataBlockPulses(PureDataBlock block, HardwareSettings hardware)
    {
        var pulseSettings = PulseFactory.Create(block, hardware);

        return GetTapeDataPulses(null, pulseSettings);
    }

    private static IEnumerable<Pulse> GetTapeDataPulses(TapData? tapData, BlockPulses blockPulses)
    {
        if (tapData?.IsHeader == true)
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

        foreach (var pulse in blockPulses.DataPulses)
        {
            yield return pulse;
        }

        if (blockPulses.PausePulse != null)
        {
            yield return blockPulses.PausePulse;
        }
    }
}