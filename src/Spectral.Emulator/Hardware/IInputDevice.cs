namespace OldBit.Spectral.Emulator.Hardware;

public interface IInputDevice
{
    byte? Read(Word address);
}