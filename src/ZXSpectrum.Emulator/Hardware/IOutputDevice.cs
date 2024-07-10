namespace OldBit.ZXSpectrum.Emulator.Hardware;

public interface IOutputDevice
{
    void Write(Word address, byte data);
}