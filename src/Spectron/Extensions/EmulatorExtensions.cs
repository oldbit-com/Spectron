using System.Collections.Generic;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
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
        emulator.AudioManager.IsAyEnabled = audioSettings.IsAyAudioEnabled;
        emulator.AudioManager.IsAySupportedStandardSpectrum = audioSettings.IsAySupportedStandardSpectrum;
        emulator.AudioManager.StereoMode = audioSettings.StereoMode;
    }

    public static void SetUlaPlus(this Emulator? emulator, bool isUlaPlusEnabled)
    {
        if (emulator != null)
        {
            emulator.IsUlaPlusEnabled = isUlaPlusEnabled;
        }
    }

    public static void SetGamepad(this Emulator? emulator, JoystickSettings joystickSettings) =>
        emulator?.GamepadManager.SetupGamepad(
            new GamepadPreferences(
                joystickSettings.JoystickGamepad,
                joystickSettings.JoystickType,
                joystickSettings.GamepadSettings.MappingsByController.GetValueOrDefault(joystickSettings.JoystickGamepad, [])));
}