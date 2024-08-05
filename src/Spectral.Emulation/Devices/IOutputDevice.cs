namespace OldBit.Spectral.Emulation.Devices;

public interface IOutputDevice
{
    void Write(Word address, byte value);
}