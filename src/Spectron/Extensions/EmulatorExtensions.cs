using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Settings;

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

    public static void SetTapeSavingSettings(this Emulator? emulator, TapeSavingSettings tapeSavingSettings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.TapeManager.IsTapeSaveEnabled = tapeSavingSettings.IsEnabled;
        emulator.TapeManager.IsFastTapeSaveEnabled = tapeSavingSettings.IsFastSaveEnabled;
    }
}