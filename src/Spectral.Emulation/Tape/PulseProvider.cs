using OldBit.Spectral.Emulation.Extensions;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulation.Tape;

/// <summary>
/// A pulse represents a change in the signal level (lo->hi or hi->lo, e.g. toggle).
/// </summary>
/// <param name="RepeatCount">How many times pulse change should be repeated.</param>
/// <param name="Duration">The pulse duration in T-states.</param>
/// <param name="IsSilence">Used to add a delay between blocks.</param>
internal sealed record Pulse(int RepeatCount, int Duration, bool IsSilence = false);

/// <summary>
/// Memory efficient tape pulse provider. Only supports Standard Speed Data Blocks.
/// Normally there are many thousands of pulses in a file, so we don't want to store them all in memory.
/// </summary>
internal sealed class PulseProvider(TzxFile tzxFile, HardwareSettings hardware)
{
    private const int PilotHeaderPulseCount = 8063;         // Before each header block is a sequence of 8063 pulses
    private const int PilotDataPulseCount = 3223;           // Before each data block is a sequence of 3223 pulses
    private const int PilotPulseLength = 2168;              // Pilot tone length is 2168 T-states
    private const int FirstSyncPulseLength = 667;           // The pilot tone is followed by two sync pulses of 667
    private const int SecondSyncPulseLength = 735;          // and 735 T-states each
    private const int ZeroBitPulseLength = 855;             // '0' bit is encoded as 2 pulses of 855 T-states each
    private const int OneBitPulseLength = 1710;             // '1' bit is encoded as 2 pulses of 1710 T-states each

    private static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    private static readonly Pulse PilotHeaderPulse = new(PilotHeaderPulseCount, PilotPulseLength);
    private static readonly Pulse PilotDataPulse = new(PilotDataPulseCount, PilotPulseLength);
    private static readonly Pulse FirstSyncPulse = new(RepeatCount: 1, FirstSyncPulseLength);
    private static readonly Pulse SecondSyncPulse = new(RepeatCount: 1, SecondSyncPulseLength);
    private static readonly Pulse ZeroBitPulse = new(RepeatCount: 2, ZeroBitPulseLength);
    private static readonly Pulse OneBitPulse = new(RepeatCount: 2, OneBitPulseLength);

    internal IEnumerable<Pulse> GetAll()
    {
        foreach (var block in tzxFile.Blocks.GetStandardSpeedDataBlocks())
        {
            if (!TapData.TryParse(block.Data, out var tapData))
            {
                continue;
            }

            if (tapData.IsHeader)
            {
                yield return PilotHeaderPulse;
            }
            else
            {
                yield return PilotDataPulse;
            }

            yield return FirstSyncPulse;
            yield return SecondSyncPulse;

            var pulses = block.Data
                .SelectMany(item => BitMasks.Select(mask => (item & mask) == 0 ? ZeroBitPulse : OneBitPulse));

            foreach (var pulse in pulses)
            {
                yield return pulse;
            }

            var pauseMilliseconds = block.PauseDuration == 0 ? 500 : block.PauseDuration;
            var duration = (int)(pauseMilliseconds / 1000f * hardware.TicksPerFrame * hardware.InterruptFrequency);

            yield return new Pulse(RepeatCount: 1, Duration: duration, IsSilence: true);
        }
    }
}