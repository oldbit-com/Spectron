using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Extensions;

public static class EmulatorExtensions
{
    public static void SetTapeLoadingSpeed(this Emulator? emulator, TapeSpeed tapeSpeed)
    {
        if (emulator != null)
        {
            emulator.TapeLoadSpeed = tapeSpeed;
        }
    }

    public static void SetTapeSavingSettings(this Emulator? emulator, TapeSavingSettings tapeSavingSettings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.TapeManager.IsTapeSaveEnabled = tapeSavingSettings.IsEnabled;
        emulator.TapeManager.SaveTapeSpeed = tapeSavingSettings.Speed;
    }
}