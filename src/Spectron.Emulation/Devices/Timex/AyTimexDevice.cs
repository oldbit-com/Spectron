using OldBit.Spectron.Emulation.Devices.Audio.AY;

namespace OldBit.Spectron.Emulation.Devices.Timex;

internal sealed class AyTimexDevice : AyDevice
{
    protected override bool IsDataPort(Word address) => address == 0xFFF5;

    protected override bool IsRegisterPort(Word address) => address == 0xFFF6;
}