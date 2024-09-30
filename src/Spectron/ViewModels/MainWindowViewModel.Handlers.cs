using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Controls;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Helpers;
using OldBit.Spectron.Models;
using OldBit.Spectron.Views;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task HandleLoadFileAsync() => await HandleLoadFileAsync(null);

    private async Task HandleLoadFileAsync(string? filePath)
    {
        try
        {
            Emulator?.Pause();

            if (filePath == null)
            {
                var files = await FileDialogs.OpenAnyFileAsync();
                if (files.Count <= 0)
                {
                    return;
                }

                filePath = files[0].Path.LocalPath;
            }

            var fileType = FileTypeHelper.GetFileType(filePath);
            if (fileType.IsSnapshot())
            {
                var emulator = _snapshotLoader.Load(filePath);
                InitializeEmulator(emulator);
            }
            else
            {
                var emulator = _loader.EnterLoadCommand(ComputerType);
                InitializeEmulator(emulator);

                emulator.TapeManager.InsertTape(
                    filePath,
                    autoPlay: TapeLoadSpeed != TapeSpeed.Instant);
            }

            RecentFilesViewModel.Add(filePath);
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
            RecentFilesViewModel.Remove(filePath);
        }
        finally
        {
            Emulator?.Resume();
        }
    }

    private async Task HandleSaveFileAsync()
    {
        try
        {
            Emulator?.Pause();

            var file = await FileDialogs.SaveSnapshotFileAsync();
            if (file != null && Emulator != null)
            {
                SnapshotLoader.Save(file.Path.LocalPath, Emulator);
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
        finally
        {
            Emulator?.Resume();
        }
    }

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    private void HandleChangeRom(RomType romType)
    {
        RomType = romType;

        CreateEmulator();
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        ComputerType = computerType;

        CreateEmulator();
    }

    private void HandleChangeJoystickType(JoystickType joystickType)
    {
        JoystickType = joystickType;
        Emulator?.JoystickManager.SetupJoystick(joystickType);
    }

    private void HandleToggleUlaPlus()
    {
        IsUlaPlusEnabled = !IsUlaPlusEnabled;

        if (Emulator != null)
        {
            Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        }
    }

    private void HandleMachineReset()
    {
        Emulator?.Reset();
        IsPaused = Emulator?.IsPaused ?? false;
    }

    private void HandleTogglePause()
    {
        switch (Emulator?.IsPaused)
        {
            case true:
                Emulator.Resume();
                break;

            case false:
                Emulator.Pause();
                break;
        }

        IsPaused = Emulator?.IsPaused ?? false;

        if (IsPaused)
        {
            TimeMachineViewModel.BeforeShow();
        }
    }

    private void HandleSetEmulationSpeed(string emulationSpeed)
    {
        int emulationSpeedValue;

        if (emulationSpeed.Equals("max", StringComparison.OrdinalIgnoreCase))
        {
            emulationSpeedValue = int.MaxValue;
        }
        else
        {
            if (!int.TryParse(emulationSpeed, out emulationSpeedValue))
            {
                return;
            }
        }

        Emulator?.SetEmulationSpeed(emulationSpeedValue);
    }

    private void HandleToggleFullScreen() =>
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

    private void HandleSetTapeLoadingSpeed(TapeSpeed tapeSpeed) => TapeLoadSpeed = tapeSpeed;

    private void HandleHelpKeyboardCommand()
    {
        if (_helpKeyboardView == null)
        {
            _helpKeyboardView = new HelpKeyboardView();
            _helpKeyboardView.Closed += (_, _) => _helpKeyboardView = null;

            if (MainWindow != null)
            {
                _helpKeyboardView.Show(MainWindow);
            }
            else
            {
                _helpKeyboardView.Show();
            }
        }
        else
        {
            _helpKeyboardView.Activate();
        }
    }

    private void HandleShowTimeMachineCommand()
    {
    }

    private void HandleKeyUp(KeyEventArgs e)
    {
        if (IsPaused)
        {
            return;
        }

        if (JoystickType != JoystickType.None)
        {
            var joystickKeys = KeyMappings.ToJoystickAction(e);
            if (joystickKeys != JoystickInput.None)
            {
                Emulator?.JoystickManager.HandleInput(joystickKeys, isOn: false);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardHandler.HandleKeyUp(keys);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (IsPaused)
        {
            if (e.Key == Key.Escape)
            {
                HandleTogglePause();
            }
            return;
        }

        if (JoystickType != JoystickType.None && _useCursorKeysAsJoystick)
        {
            var joystickKeys = KeyMappings.ToJoystickAction(e);
            if (joystickKeys != JoystickInput.None)
            {
                Emulator?.JoystickManager.HandleInput(joystickKeys, isOn: true);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardHandler.HandleKeyDown(keys);
    }

    private void HandleTimeTravel(TimeMachineEntry entry) => CreateEmulator(entry.Snapshot);

    private void HandleToggleMute()
    {
        IsMuted = !IsMuted;

        if (IsMuted)
        {
            Emulator?.Mute();
        }
        else
        {
            Emulator?.UnMute();
        }
    }
}