namespace OldBit.Spectral.Emulation.Devices;

public interface IInputDevice
{
    byte? Read(Word address);
}