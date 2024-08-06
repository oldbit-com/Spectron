namespace OldBit.Spectral.Emulation.Devices;

/// <summary>
/// Represents a device that can be connected to the bus.
/// </summary>
internal interface IDevice
{
    void WritePort(Word address, byte value) { }

    byte? ReadPort(Word address) => null;
}