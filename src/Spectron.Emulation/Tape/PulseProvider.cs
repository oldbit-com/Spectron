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

            var pulseSettings = PulseFactory.Create(block, hardware);

            if (pulseSettings == null)
            {
                continue;
            }

            TapData? tapData = null;
            switch (block)
            {
                case StandardSpeedDataBlock standardSpeedDataBlock when !TapData.TryParse(standardSpeedDataBlock.Data, out tapData):
                case TurboSpeedDataBlock turboSpeedDataBlock when !TapData.TryParse(turboSpeedDataBlock.Data, out tapData):
                    continue;
            }

            if (tapData == null)
            {
                continue;
            }

            if (tapData.IsHeader)
            {
                yield return pulseSettings.PilotHeaderPulse;
            }
            else
            {
                yield return pulseSettings.PilotDataPulse;
            }

            yield return pulseSettings.FirstSyncPulse;
            yield return pulseSettings.SecondSyncPulse;

            foreach (var pulse in pulseSettings.DataPulses)
            {
                yield return pulse;
            }

            yield return pulseSettings.PausePulse;
        }
    }
}