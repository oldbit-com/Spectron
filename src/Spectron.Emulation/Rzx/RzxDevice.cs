using OldBit.Spectron.Emulation.Devices;

namespace OldBit.Spectron.Emulation.Rzx;

/// <summary>
/// Virtual RZX playback device. It runs before all other devices, so it answers every
/// port read with the value taken from the recording.
/// </summary>
/// <param name="inPort">Returns the next recorded port read value.</param>
internal class RzxDevice(Func<byte> inPort) : IDevice
{
    public byte? ReadPort(Word address) => inPort();

    public int Priority => int.MinValue;
}
