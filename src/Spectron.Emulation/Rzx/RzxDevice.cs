using OldBit.Spectron.Emulation.Devices;

namespace OldBit.Spectron.Emulation.Rzx;

internal class RzxDevice(RzxHandler rzxHandler) : IDevice
{
    public byte? ReadPort(Word address) => rzxHandler.ReadPort();

    public int Priority => int.MinValue;
}