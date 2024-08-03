using OldBit.Spectral.Emulator.Hardware;

namespace OldBit.Spectral.Emulator.Computers;

public interface IEmulator
{
    void Start();

    void Pause();

    void Resume();

    Keyboard Keyboard { get; }

    void LoadFile(string fileName);
}