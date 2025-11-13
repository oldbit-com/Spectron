using System.Collections.Generic;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Extensions;

public static class EmulatorExtensions
{
    extension(Emulator? emulator)
    {
        public void SetTapeSettings(TapeSettings tapeSettings)
        {
            emulator?.TapeManager.IsTapeSaveEnabled = tapeSettings.IsSaveEnabled;
            emulator?.TapeManager.IsCustomLoaderDetectionEnabled = tapeSettings.IsCustomLoaderDetectionEnabled;
            emulator?.TapeManager.TapeSaveSpeed = tapeSettings.SaveSpeed;
            emulator?.TapeManager.TapeLoadSpeed = tapeSettings.LoadSpeed;
        }

        public void SetAudioSettings(AudioSettings audioSettings)
        {
            emulator?.AudioManager.IsBeeperEnabled = audioSettings.IsBeeperEnabled;
            emulator?.AudioManager.IsAyEnabled = audioSettings.IsAyAudioEnabled;
            emulator?.AudioManager.IsAySupportedStandardSpectrum = audioSettings.IsAySupportedStandardSpectrum;
            emulator?.AudioManager.StereoMode = audioSettings.StereoMode;
        }

        public void SetFloatingBusSupport(bool isFloatingBusEnabled)
        {
            emulator?.IsFloatingBusEnabled = isFloatingBusEnabled;
        }

        public void SetGamepad(JoystickSettings joystickSettings) =>
            emulator?.GamepadManager.Setup(
                new GamepadPreferences(
                    joystickSettings.GamepadControllerId,
                    joystickSettings.JoystickType,
                    joystickSettings.GamepadSettings.Mappings.GetValueOrDefault(joystickSettings.GamepadControllerId, [])));

        public void SetDivMMc(DivMmcSettings divMmcSettings)
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
    }
}