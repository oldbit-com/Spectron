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
        if (emulator == null)
        {
            return;
        }

        emulator.TapeLoadSpeed = tapeSpeed;
    }

    public static void SetTapeSettings(this Emulator? emulator, TapeSettings tapeSettings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.TapeManager.IsTapeSaveEnabled = tapeSettings.IsSaveEnabled;
        emulator.TapeManager.TapeSaveSpeed = tapeSettings.SaveSpeed;
        emulator.TapeLoadSpeed = tapeSettings.LoadSpeed;
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

    public static void SetFloatingBusSupport(this Emulator? emulator, bool isFloatingBusEnabled)
    {
        if (emulator != null)
        {
            emulator.IsFloatingBusEnabled = isFloatingBusEnabled;
        }
    }

    public static void SetGamepad(this Emulator? emulator, JoystickSettings joystickSettings) =>
        emulator?.GamepadManager.Setup(
            new GamepadPreferences(
                joystickSettings.GamepadControllerId,
                joystickSettings.JoystickType,
                joystickSettings.GamepadSettings.Mappings.GetValueOrDefault(joystickSettings.GamepadControllerId, [])));

    public static void SetDivMMc(this Emulator? emulator, DivMmcSettings divMmcSettings)
    {
        if (emulator == null)
        {
            return;
        }

        if (divMmcSettings.IsEnabled)
        {
            emulator.DivMmc.InsertCard(divMmcSettings.Card0FileName);
            emulator.DivMmc.Enable();
        }
        else
        {
            emulator.DivMmc.Disable();
        }

        emulator.DivMmc.Memory.IsWriteEnabled = divMmcSettings.IsWriteEnabled;
    }
}