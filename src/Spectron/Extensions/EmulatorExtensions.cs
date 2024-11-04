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
        emulator.TapeManager.TapeSaveSpeed = tapeSavingSettings.Speed;
    }

    public static void SetAudioSettings(this Emulator? emulator, AudioSettings audioSettings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.AudioManager.IsBeeperEnabled = audioSettings.IsBeeperEnabled;
        emulator.AudioManager.IsAyAudioEnabled = audioSettings.IsAyAudioEnabled;
        emulator.AudioManager.IsAyAudioEnabled48K = audioSettings.IsAyAudioEnabled48K;
    }
}