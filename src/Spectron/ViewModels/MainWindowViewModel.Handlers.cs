using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Keyboard;
using OldBit.Spectron.Models;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task HandleLoadFileAsync() => await HandleLoadFileAsync(null);

    private async Task HandleLoadFileAsync(string? filePath)
    {
        Stream? stream = null;
        var shouldResume = !IsPaused;

        try
        {
            Pause();

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
                Title = $"{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
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

    private async Task HandleSaveFileAsync()
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            var file = await FileDialogs.SaveSnapshotFileAsync();

            if (file != null && Emulator != null)
            {
                SnapshotManager.Save(file.Path.LocalPath, Emulator);
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

    private void HandleQuickSave()
    {
        if (Emulator == null)
        {
            return;
        }

        _quickSaveService.RequestQuickSave();
    }

    private void HandleQuickLoad()
    {
        var snapshot = _quickSaveService.QuickLoad();

        if (snapshot != null)
        {
            CreateEmulator(snapshot);
        }
    }

    private RecorderOptions GetRecorderOptions() => new()
    {
        AudioChannels = Emulator?.AudioManager.StereoMode == StereoMode.Mono ? 1 : 2,
        BorderLeft = BorderSizes.GetBorder(_preferences.RecordingSettings.BorderSize).Left,
        BorderRight = BorderSizes.GetBorder(_preferences.RecordingSettings.BorderSize).Right,
        BorderTop = BorderSizes.GetBorder(_preferences.RecordingSettings.BorderSize).Top,
        BorderBottom = BorderSizes.GetBorder(_preferences.RecordingSettings.BorderSize).Bottom,
        ScalingFactor = _preferences.RecordingSettings.ScalingFactor,
        ScalingAlgorithm = _preferences.RecordingSettings.ScalingAlgorithm,
        FFmpegPath = _preferences.RecordingSettings.FFmpegPath,
    };

    private async Task HandleStartAudioRecordingAsync()
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            var file = await FileDialogs.SaveAudioFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    RecorderMode.Audio,
                    file.Path.LocalPath,
                    GetRecorderOptions(),
                    _logger);

                _mediaRecorder.Start();

                RecordingStatus = RecordingStatus.Recording;
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

    private async Task HandleStartVideoRecordingAsync()
    {
        var shouldResume = !IsPaused;

        if (!MediaRecorder.VerifyDependencies())
        {
            await MessageDialogs.Error("Video recording is not available. It requires FFmpeg to be available.\nPlease check the documentation for more information.");

            return;
        }

        try
        {
            Pause();

            var file = await FileDialogs.SaveVideoFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    RecorderMode.AudioVideo,
                    file.Path.LocalPath,
                    GetRecorderOptions(),
                    _logger);

                _mediaRecorder.Start();

                RecordingStatus = RecordingStatus.Recording;
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

    private void HandleStopRecording()
    {
        if (_mediaRecorder == null)
        {
            RecordingStatus = RecordingStatus.None;
            return;
        }

        if (RecordingStatus != RecordingStatus.Recording)
        {
            return;
        }

        RecordingStatus = RecordingStatus.Processing;

        _mediaRecorder.StartProcess(result =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                RecordingStatus = RecordingStatus.None;

                NotificationManager.Show(new Notification(
                    result.ISucccess ? "Done!" : "Error!",
                    result.ISucccess ? "Recording has been successfully completed" : $"Recording has failed: {result.Error?.Message}",
                    result.ISucccess ? NotificationType.Information : NotificationType.Error)
                {
                    Expiration = TimeSpan.FromSeconds(10)
                });
            });

            _mediaRecorder.Dispose();
            _mediaRecorder = null;

            if (!result.ISucccess)
            {
                _logger.LogError(result.Error, "Failed to process recording");
            }
        });
    }

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.ScreenBitmap;
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
        UpdateWindowTitle();
    }

    private void HandleMachineHardReset()
    {
        CreateEmulator(_preferences.ComputerType, _preferences.RomType);

        RecentFilesViewModel.CurrentFileName = string.Empty;
        UpdateWindowTitle();
    }

    private void HandleTapeStateChanged(TapeStateEventArgs args)
    {
        if (args.Action == TapeAction.TapeEjected)
        {
            RecentFilesViewModel.CurrentFileName = string.Empty;
        }

        UpdateWindowTitle();
    }

    private void Pause()
    {
        if (IsPaused)
        {
            return;
        }

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

        StatusBarViewModel.Speed = emulationSpeed;
        Emulator?.SetEmulationSpeed(emulationSpeedValue);
    }

    private void HandleToggleFullScreen() =>
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

    private void HandleSetTapeLoadingSpeed(TapeSpeed tapeSpeed) => TapeLoadSpeed = tapeSpeed;

    private void HandleKeyUp(KeyEventArgs e)
    {
        if (MainWindow?.IsActive != true)
        {
            return;
        }

        if (IsPaused)
        {
            return;
        }

        var joystickInput = JoystickInput.None;

        if (IsKeyboardJoystickEmulationEnabled)
        {
            joystickInput = KeyMappings.ToJoystickAction(e.PhysicalKey, _preferences.Joystick.FireKey);

            if (joystickInput != JoystickInput.None)
            {
                Emulator?.JoystickManager.Released(joystickInput);
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e, joystickInput);

        Emulator?.KeyboardState.KeyUp(keys);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (MainWindow?.IsActive != true)
        {
            return;
        }

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

        var joystickInput = JoystickInput.None;

        if (IsKeyboardJoystickEmulationEnabled)
        {
            joystickInput = KeyMappings.ToJoystickAction(e.PhysicalKey, _preferences.Joystick.FireKey);

            if (joystickInput != JoystickInput.None)
            {
                Emulator?.JoystickManager.Pressed(joystickInput);
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e, joystickInput);

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