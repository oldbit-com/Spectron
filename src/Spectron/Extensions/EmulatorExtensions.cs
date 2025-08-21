using System.Collections.Generic;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Gamepad;
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

        emulator.TapeManager.TapeLoadSpeed = tapeSpeed;
    }

    public static void SetTapeSettings(this Emulator? emulator, TapeSettings tapeSettings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.TapeManager.IsTapeSaveEnabled = tapeSettings.IsSaveEnabled;
        emulator.TapeManager.TapeSaveSpeed = tapeSettings.SaveSpeed;
        emulator.TapeManager.TapeLoadSpeed = tapeSettings.LoadSpeed;
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
            if (!string.IsNullOrWhiteSpace(divMmcSettings.Card0FileName))
            {
                emulator.DivMmc.InsertCard(divMmcSettings.Card0FileName, slotNumber: 0);
            }

            if (!string.IsNullOrWhiteSpace(divMmcSettings.Card1FileName))
            {
                emulator.DivMmc.InsertCard(divMmcSettings.Card1FileName, slotNumber: 1);
            }

            emulator.DivMmc.Enable();
        }
        else
        {
            emulator.DivMmc.Disable();
        }

        emulator.DivMmc.Memory.IsEepromWriteEnabled = divMmcSettings.IsEepromWriteEnabled;
        emulator.DivMmc.IsDriveWriteEnabled = divMmcSettings.IsDriveWriteEnabled;
    }

    public static void SetInterface1(this Emulator? emulator, Interface1Settings interface1Settings)
    {
        if (emulator == null)
        {
            return;
        }

        emulator.Interface1.ShadowRom.Version = interface1Settings.RomVersion;

        if (interface1Settings.IsEnabled)
        {
            emulator.Interface1.Enable();
        }
        else
        {
            emulator.Interface1.Disable();
        }
    }

}