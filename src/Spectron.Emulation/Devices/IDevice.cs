namespace OldBit.Spectron.Emulation.Devices;

/// <summary>
/// Represents a device that can be connected to the bus.
/// </summary>
public interface IDevice
{
    void WritePort(Word address, byte value) { }

    byte? ReadPort(Word address) => null;

    int Priority => 0;
}