namespace OldBit.ZXSpectrum.Emulator.Hardware;

public interface IInputDevice
{
    byte Read(Word address);
}