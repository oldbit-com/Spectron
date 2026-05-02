using OldBit.Spectron.Emulation.Devices;

namespace OldBit.Spectron.Emulation.Rzx;

internal class RzxDevice(Func<byte> inPort) : IDevice
{
    public byte? ReadPort(Word address) => inPort();

    public int Priority => int.MinValue;
}