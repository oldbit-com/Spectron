using OldBit.Spectral.Emulator.Hardware;
using OldBit.Spectral.Emulator.Rom;

namespace OldBit.Spectral.Emulator.Computers;

public interface ISpectrum
{
    void Start();

    void Pause();

    void Resume();

    Keyboard Keyboard { get; }

    void LoadFile(string fileName);
}