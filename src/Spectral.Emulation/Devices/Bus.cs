using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices;

internal sealed class Bus : IBus
{
    private readonly List<IDevice> _devices = [];

    internal void AddDevice(IDevice device) => _devices.Add(device);

    public byte Read(Word address)
    {
        foreach (var device in _devices)
        {
            var result = device.ReadPort(address);
            if (result != null)
            {
                return result.Value;
            }
        }

        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        foreach(var device in _devices)
        {
            device.WritePort(address, data);
        }
    }
}