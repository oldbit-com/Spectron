using OldBit.Spectral.Emulator.Screen;
using OldBit.ZXTape.Tap;

namespace OldBit.Spectral.Emulator.Tape;

/// <summary>
/// Memory efficient tape pulse provider. Only supports TAP files at the moment.
/// </summary>
internal class PulseProvider(TapFile tapFile)
{
    private const int PilotHeaderPulseCount = 8063;         // Before each header block is a sequence of 8063 pulses
    private const int PilotDataPulseCount = 3223;           // Before each data block is a sequence of 3223 pulses
    private const int PilotPulseLength = 2168;              // Pilot tone length is 2168 T-states
    private const int FirstSyncPulseLength = 667;           // The pilot tone is followed by two sync pulses of 667
    private const int SecondSyncPulseLength = 735;          // and 735 T-states each
    private const int ZeroBitPulseLength = 855;             // '0' bit is encoded as 2 pulses of 855 T-states each
    private const int OneBitPulseLength = 1710;             // '1' bit is encoded as 2 pulses of 1710 T-states each

    private static readonly Pulse PilotHeaderPulse = new(PilotHeaderPulseCount, PilotPulseLength);
    private static readonly Pulse PilotDataPulse = new(PilotDataPulseCount, PilotPulseLength);
    private static readonly Pulse FirstSyncPulse = new(RepeatCount: 1, FirstSyncPulseLength);
    private static readonly Pulse SecondSyncPulse = new(RepeatCount: 1, SecondSyncPulseLength);
    private static readonly Pulse ZeroBitPulse = new(RepeatCount: 2, ZeroBitPulseLength);
    private static readonly Pulse OneBitPulse = new(RepeatCount: 2, OneBitPulseLength);

    internal IEnumerable<Pulse> GetPulses() => PrivateGetPulses().SelectMany(pulse => pulse);

    private IEnumerable<IEnumerable<Pulse>> PrivateGetPulses()
    {
        foreach (var block in tapFile.Blocks)
        {
            if (block.IsHeader)
            {
                yield return PilotHeaderPulse.AsEnumerable();
            }
            else
            {
                yield return PilotDataPulse.AsEnumerable();
            }

            yield return FirstSyncPulse.AsEnumerable();
            yield return SecondSyncPulse.AsEnumerable();

            yield return ToEnumerable(block.Flag);
            yield return ToEnumerable(block.Data);
            yield return ToEnumerable(block.Checksum);
        }
    }

    private static IEnumerable<Pulse> ToEnumerable(byte value) => ToEnumerable([value]);

    private static IEnumerable<Pulse> ToEnumerable(IEnumerable<byte> values) =>
        values.SelectMany(value => GetBitValue(value).Select(bit => bit == 0 ? ZeroBitPulse : OneBitPulse));

    private static IEnumerable<int> GetBitValue(byte value) => FastLookup.BitMasks.Select(mask => value & mask);
}