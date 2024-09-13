using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Extensions;

public static class EmulatorExtensions
{
    public static void SetTapeLoadingSpeed(this Emulator? emulator, TapeLoadingSpeed tapeLoadingSpeed)
    {
        if (emulator != null)
        {
            emulator.TapeLoadingSpeed = tapeLoadingSpeed;
        }
    }
}