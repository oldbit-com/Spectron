using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Emulator;

public interface ISpectrum
{
    void Start();

    Keyboard Keyboard { get; }

    void LoadFile(string fileName);
}