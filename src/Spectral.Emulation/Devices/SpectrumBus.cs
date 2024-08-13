using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices;

internal sealed class SpectrumBus : IBus
{
    private readonly List<IDevice> _devices = [];

    internal void AddDevice(IDevice device)
    {
        var lastIndex = _devices.FindLastIndex(x => x.Priority <= device.Priority);

        if (lastIndex == -1)
        {
            _devices.Add(device);
        }
        else
        {
            _devices.Insert(lastIndex + 1, device);
        }
    }

    internal void RemoveDevice(IDevice? device)
    {
        if (device == null)
        {
            return;
        }

        _devices.Remove(device);
    }

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