using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

internal record DataPulses(IEnumerable<Pulse> Pulses, int TotalCount);

internal record BlockPulses(
    Pulse? PilotHeaderPulse,
    Pulse? PilotDataPulse,
    Pulse? FirstSyncPulse,
    Pulse? SecondSyncPulse,
    Pulse? PausePulse,
    DataPulses Data);

internal static class PulseFactory
{
    private static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    internal static BlockPulses Create(StandardSpeedDataBlock block, HardwareSettings hardware)
    {
        const int pilotHeaderPulseCount = 8063;     // Before each header block is a sequence of 8063 pulses
        const int pilotDataPulseCount = 3223;       // Before each data block is a sequence of 3223 pulses
        const int pilotPulseLength = 2168;          // Pilot tone length is 2168 T-states
        const int firstSyncPulseLength = 667;       // The pilot tone is followed by two sync pulses of 667
        const int secondSyncPulseLength = 735;      // and 735 T-states each
        const int zeroBitPulseLength = 855;         // '0' bit is encoded as 2 pulses of 855 T-states each
        const int oneBitPulseLength = 1710;         // '1' bit is encoded as 2 pulses of 1710 T-states each

        var zeroBitPulse = new Pulse(RepeatCount: 2, zeroBitPulseLength);
        var oneBitPulse = new Pulse(RepeatCount: 2, oneBitPulseLength);

        return new BlockPulses(
            PilotHeaderPulse: new Pulse(pilotHeaderPulseCount, pilotPulseLength),
            PilotDataPulse: new Pulse(pilotDataPulseCount, pilotPulseLength),
            FirstSyncPulse: new Pulse(RepeatCount: 1, firstSyncPulseLength),
            SecondSyncPulse: new Pulse(RepeatCount: 1, secondSyncPulseLength),
            PausePulse: CreatePausePulse(block.PauseDuration, hardware),
            Data: new DataPulses(
                block.Data.SelectMany(data => BitMasks.Select(mask => (data & mask) == 0 ? zeroBitPulse : oneBitPulse)),
                block.Data.Count * 8));
    }

    internal static BlockPulses Create(TurboSpeedDataBlock block, HardwareSettings hardware)
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

        var zeroBitPulse = new Pulse(RepeatCount: 2, block.ZeroBitPulseLength);
        var oneBitPulse = new Pulse(RepeatCount: 2, block.OneBitPulseLength);

        return new BlockPulses(
            PilotHeaderPulse: new Pulse(pilotHeaderPulseCount, block.PilotPulseLength),
            PilotDataPulse: new Pulse(pilotDataPulseCount, block.PilotPulseLength),
            FirstSyncPulse: new Pulse(RepeatCount: 1, block.FirstSyncPulseLength),
            SecondSyncPulse: new Pulse(RepeatCount: 1, block.SecondSyncPulseLength),
            PausePulse: CreatePausePulse(block.PauseDuration, hardware),
            Data: GetDataPulses(block.Data, zeroBitPulse, oneBitPulse, block.UsedBitsInLastByte));
    }

    private static DataPulses GetDataPulses(List<byte> data, Pulse zeroBitPulse, Pulse oneBitPulse, byte usedBitsInLastByte)
    {
        var dataPulses = data
            .SkipLast(1)
            .SelectMany(item => BitMasks.Select(mask => (item & mask) == 0 ? zeroBitPulse : oneBitPulse))
            .Concat(
                BitMasks.Take(usedBitsInLastByte).Select(mask => (data.Last() & mask) == 0 ? zeroBitPulse : oneBitPulse)
            );

        return new DataPulses(dataPulses, (data.Count - 1) * 8 + usedBitsInLastByte);
    }

    internal static BlockPulses Create(PureDataBlock block, HardwareSettings hardware)
    {
        var zeroBitPulse = new Pulse(RepeatCount: 2, block.ZeroBitPulseLength);
        var oneBitPulse = new Pulse(RepeatCount: 2, block.OneBitPulseLength);

        return new BlockPulses(
            PilotHeaderPulse: null,
            PilotDataPulse: null,
            FirstSyncPulse: null,
            SecondSyncPulse: null,
            PausePulse: CreatePausePulse(block.PauseDuration, hardware),
            Data: GetDataPulses(block.Data, zeroBitPulse, oneBitPulse, block.UsedBitsInLastByte));
    }

    internal static Pulse? CreatePausePulse(int pauseMilliseconds, HardwareSettings hardware)
    {
        if (pauseMilliseconds == 0)
        {
            return null;
        }

        var duration = (int)(pauseMilliseconds / 1000f * hardware.TicksPerFrame * hardware.InterruptFrequency);

        return new Pulse(RepeatCount: 1, Duration: duration, IsSilence: true);
    }

    internal static Pulse Create(PureToneBlock block) => new(block.PulseCount, block.PulseLength);

    internal static IEnumerable<Pulse> Create(PulseSequenceBlock block) =>
        block.PulseLengths.Select(pulseLength => new Pulse(RepeatCount: 1, Duration: pulseLength));
}