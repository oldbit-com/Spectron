using System.Collections.ObjectModel;
using OldBit.Spectral.Emulator.Screen;

namespace OldBit.Spectral.Emulator.Tape;

internal record TapePulse(int PulseCount, int PulseLength);

internal class TapePulseCollection
{
    private const int PilotHeaderPulseCount = 8063;         // Before each header block is a sequence of 8063 pulses
    private const int PilotDataPulseCount = 3223;           // Before each data block is a sequence of 3223 pulses
    private const int PilotPulseLength = 2168;              // Pilot tone length is 2168 T-states
    private const int FirstSyncPulseLength = 667;           // The pilot tone is followed by two sync pulses of 667
    private const int SecondSyncPulseLength = 735;          // and 735 T-states each
    private const int ZeroBitPulseLength = 855;             // '0' bit is encoded as 2 pulses of 855 T-states each
    private const int OneBitPulseLength = 1710;             // '1' bit is encoded as 2 pulses of 1710 T-states each

    private readonly List<TapePulse> _pulses = [];

    private readonly TapePulse _zeroBitPulse = new(PulseCount: 2, ZeroBitPulseLength);
    private readonly TapePulse _oneBitPulse = new(PulseCount: 2, OneBitPulseLength);

    internal ReadOnlyCollection<TapePulse> Pulses => new(_pulses);

    internal void AddPilotHeaderPulses() =>
        _pulses.Add(new TapePulse(PilotHeaderPulseCount, PilotPulseLength));

    internal void AddSyncPulses()
    {
        _pulses.Add(new TapePulse(PulseCount: 1, FirstSyncPulseLength));
        _pulses.Add(new TapePulse(PulseCount: 1, SecondSyncPulseLength));
    }

    internal void AddPilotDataPulses() =>
        _pulses.Add(new TapePulse(PilotDataPulseCount, PilotPulseLength));

    internal void AddDataPulses(byte data) => AddDataPulses([data]);

    internal void AddDataPulses(IEnumerable<byte> bytes)
    {
        foreach (var data in bytes)
        {
            foreach (var bit in GetBits(data))
            {
                _pulses.Add(bit == 0 ? _zeroBitPulse : _oneBitPulse);
            }
        }
    }

    internal void Clear() => _pulses.Clear();

    private static IEnumerable<int> GetBits(byte value)
    {
        return FastLookup.BitMasks.Select(mask => value & mask);
    }
}