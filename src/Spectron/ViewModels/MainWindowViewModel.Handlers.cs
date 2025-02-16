using System;
using System.IO;
using System.Reactive.Linq;
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

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task HandleLoadFileAsync() => await HandleLoadFileAsync(null);

    private async Task HandleLoadFileAsync(string? filePath)
    {
        Stream? stream = null;
        var shouldResume = false;

        try
        {
            if (!IsPaused)
            {
                Pause();
                shouldResume = true;
            }

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
            if (fileType == FileType.Unsupported)
            {
                await MessageDialogs.Warning($"Unsupported file type: {fileType}.");

                return;
            }

            var fileResult = await LoadFileAsync(filePath, fileType);
            if (fileResult.Stream == null)
            {
                return;
            }

            if (CreateEmulator(fileResult.Stream, fileResult.FileType))
            {
                RecentFilesViewModel.Add(filePath);
                Title = $"S{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
            RecentFilesViewModel.Remove(filePath);
        }
        finally
        {
            stream?.Close();

            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private async Task<(Stream? Stream, FileType FileType)> LoadFileAsync(string filePath, FileType fileType)
    {
        Stream? stream = null;

        if (fileType.IsArchive())
        {
            var archive = new ZipFileReader(filePath);
            var files = archive.GetFiles();

            switch (files.Count)
            {
                case 0:
                    await MessageDialogs.Warning("No matching files found in the archive.");
                    return (null, fileType);

                case 1:
                    fileType = files[0].FileType;
                    stream = archive.GetFile(files[0].Name);
                    break;

                default:
                {
                    var selectedFile = await ShowSelectFileView.Handle(new SelectFileViewModel { FileNames = files });
                    if (selectedFile != null)
                    {
                        fileType = selectedFile.FileType;
                        stream = archive.GetFile(selectedFile.Name);
                    }

                    break;
                }
            }
        }
        else
        {
            stream = File.OpenRead(filePath);
        }

        return (stream, fileType);
    }

    private bool CreateEmulator(Stream stream, FileType fileType)
    {
        Emulator? emulator = null;

        if (fileType.IsSnapshot())
        {
            emulator = _snapshotLoader.Load(stream, fileType);
        }
        else if (fileType.IsTape())
        {
            emulator = _loader.EnterLoadCommand(ComputerType);
            emulator.TapeManager.InsertTape(stream, fileType,
                _preferences.TapeSettings.IsAutoPlayEnabled && TapeLoadSpeed != TapeSpeed.Instant);
        }

        if (emulator != null)
        {
            InitializeEmulator(emulator);
        }

        return emulator != null;
    }

    private async Task HandleSaveFileAsync()
    {
        var shouldResume = false;
        try
        {
            if (!IsPaused)
            {
                Pause();
                shouldResume = true;
            }

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
            if (shouldResume)
            {
                Resume();
            }
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
        CreateEmulator(ComputerType, RomType);
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        ComputerType = computerType;
        CreateEmulator(ComputerType, RomType);
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

        RecentFilesViewModel.CurrentFileName = string.Empty;
        SetTitle();
    }

    private void HandleMachineHardReset()
    {
        CreateEmulator(_preferences.ComputerType, _preferences.RomType);

        RecentFilesViewModel.CurrentFileName = string.Empty;
        SetTitle();
    }

    private void Pause()
    {
        Emulator?.Pause();
        IsPaused = true;
    }

    private void Resume()
    {
        Emulator?.Resume();
        IsPaused = false;
    }

    private void HandleTogglePause()
    {
        switch (Emulator?.IsPaused)
        {
            case true:
                Resume();
                break;

            case false:
                Pause();
                break;
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

    private void HandleKeyUp(KeyEventArgs e)
    {
        if (IsPaused)
        {
            return;
        }

        if (IsKeyboardJoystickEmulationEnabled)
        {
            var input = KeyMappings.ToJoystickAction(e);

            if (input != JoystickInput.None)
            {
                Emulator?.JoystickManager.Released(input);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardState.KeyUp(keys);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        switch (e)
        {
            case { Key: Key.Escape }:
                if (IsPaused)
                {
                    HandleTogglePause();
                    return;
                }
                break;

            case { Key: Key.F1, KeyModifiers: KeyModifiers.None }:
                _ = ShowKeyboardHelpWindow();
                return;

            case { Key: Key.F5, KeyModifiers: KeyModifiers.Control }:
                HandleMachineHardReset();
                return;
        }

        if (IsKeyboardJoystickEmulationEnabled)
        {
            var input = KeyMappings.ToJoystickAction(e);

            if (input != JoystickInput.None)
            {
                Emulator?.JoystickManager.Pressed(input);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardState.KeyDown(keys);
    }

    private bool IsKeyboardJoystickEmulationEnabled =>
        JoystickType != JoystickType.None && _preferences.Joystick.EmulateUsingKeyboard;

    private void HandleToggleMute()
    {
        IsMuted = !IsMuted;

        if (IsMuted)
        {
            Emulator?.AudioManager.Mute();
        }
        else
        {
            Emulator?.AudioManager.UnMute();
        }
    }

    private void HandleTimeMachineResumeEmulator()
    {
        IsTimeMachineCountdownVisible = false;

        Emulator?.Resume();
    }
}