using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Emulator.Computers;

public interface ISpectrum
{
    void Start();

    void Pause();

    void Resume();

    Keyboard Keyboard { get; }

    void LoadFile(string fileName);
}