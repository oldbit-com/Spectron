using OldBit.Z80Cpu;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Bus() : IBus
{
    private readonly List<IInputDevice> _inputDevices = [];
    private readonly List<IOutputDevice> _outputDevices = [];

    internal void AddInputDevice(IInputDevice device) => _inputDevices.Add(device);
    internal void AddOutputDevice(IOutputDevice device) => _outputDevices.Add(device);

    public byte Read(Word address)
    {
        foreach (var inputDevice in _inputDevices)
        {
            var result = inputDevice.Read(address);
            if (result != null)
            {
                return result.Value;
            }
        }

        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        foreach(var outputDevice in _outputDevices)
        {
            outputDevice.Write(address, data);
        }
    }

    internal static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}