using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

internal record PulseSettings(
    Pulse PilotHeaderPulse,
    Pulse PilotDataPulse,
    Pulse FirstSyncPulse,
    Pulse SecondSyncPulse,
    Pulse PausePulse,
    IEnumerable<Pulse> DataPulses,
    int UsedBitsInLastByte);

internal static class PulseFactory
{
    private static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    internal static PulseSettings? Create(IBlock block, HardwareSettings hardware)
    {
        return block switch
        {
            StandardSpeedDataBlock standardSpeedDataBlock => Create(standardSpeedDataBlock, hardware),
            TurboSpeedDataBlock turboSpeedDataBlock => Create(turboSpeedDataBlock, hardware),
            _ => null
        };
    }

    private static PulseSettings Create(StandardSpeedDataBlock block, HardwareSettings hardware)
    {
        const int pilotHeaderPulseCount = 8063;         // Before each header block is a sequence of 8063 pulses
        const int pilotDataPulseCount = 3223;           // Before each data block is a sequence of 3223 pulses
        const int pilotPulseLength = 2168;              // Pilot tone length is 2168 T-states
        const int firstSyncPulseLength = 667;           // The pilot tone is followed by two sync pulses of 667
        const int secondSyncPulseLength = 735;          // and 735 T-states each
        const int zeroBitPulseLength = 855;             // '0' bit is encoded as 2 pulses of 855 T-states each
        const int oneBitPulseLength = 1710;             // '1' bit is encoded as 2 pulses of 1710 T-states each

        var pauseMilliseconds = block.PauseDuration == 0 ? 500 : block.PauseDuration;
        var duration = (int)(pauseMilliseconds / 1000f * hardware.TicksPerFrame * hardware.InterruptFrequency);

        var zeroBitPulse = new Pulse(RepeatCount: 2, zeroBitPulseLength);
        var oneBitPulse = new Pulse(RepeatCount: 2, oneBitPulseLength);

        return new PulseSettings(
            PilotHeaderPulse: new Pulse(pilotHeaderPulseCount, pilotPulseLength),
            PilotDataPulse: new Pulse(pilotDataPulseCount, pilotPulseLength),
            FirstSyncPulse: new Pulse(RepeatCount: 1, firstSyncPulseLength),
            SecondSyncPulse: new Pulse(RepeatCount: 1, secondSyncPulseLength),
            PausePulse: new Pulse(RepeatCount: 1, Duration: duration, IsSilence: true),
            DataPulses: block.Data
                .SelectMany(item => BitMasks.Select(mask => (item & mask) == 0 ?
                    zeroBitPulse :
                    oneBitPulse)),
            UsedBitsInLastByte: 8);
    }

    private static PulseSettings Create(TurboSpeedDataBlock block, HardwareSettings hardware)
    {
        var pilotHeaderPulseCount = 8063;
        var pilotDataPulseCount = 3223;

        if (TapData.TryParse(block.Data, out var tap))
        {
            if (tap.Flag < 128)
            {
                pilotHeaderPulseCount = block.PilotToneLength;
            }
            else
            {
                pilotDataPulseCount = block.PilotToneLength;
            }
        }

        var pauseMilliseconds = block.PauseDuration == 0 ? 500 : block.PauseDuration;
        var duration = (int)(pauseMilliseconds / 1000f * hardware.TicksPerFrame * hardware.InterruptFrequency);

        var zeroBitPulse = new Pulse(RepeatCount: 2, block.ZeroBitPulseLength);
        var oneBitPulse = new Pulse(RepeatCount: 2, block.OneBitPulseLength);

        return new PulseSettings(
            PilotHeaderPulse: new Pulse(pilotHeaderPulseCount, block.PilotPulseLength),
            PilotDataPulse: new Pulse(pilotDataPulseCount, block.PilotPulseLength),
            FirstSyncPulse: new Pulse(RepeatCount: 1, block.FirstSyncPulseLength),
            SecondSyncPulse: new Pulse(RepeatCount: 1, block.SecondSyncPulseLength),
            PausePulse: new Pulse(RepeatCount: 1, Duration: duration, IsSilence: true),
            DataPulses: block.Data
                .SelectMany(item => BitMasks.Select(mask => (item & mask) == 0 ?
                    zeroBitPulse :
                    oneBitPulse)),
            UsedBitsInLastByte: block.UsedBitsInLastByte);
    }
}