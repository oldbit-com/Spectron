using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Input;
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
                var files = await FileDialogs.OpenEmulatorFileAsync();
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

            if (fileType == FileType.Pok)
            {
                await LoadPokeFile(fileResult.Stream);
                return;
            }
            else if (_preferences.IsAutoLoadPokeFilesEnabled)
            {
                TryAutoLoadPokeFile(filePath, fileResult.FileType);
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

    private async Task LoadPokeFile(Stream stream)
    {
        _pokeFile = PokeFile.Load(stream);

        await OpenTrainersWindow();
    }

    private void TryAutoLoadPokeFile(string filePath, FileType fileType)
    {
        try
        {
            var pokeFilePath = Path.ChangeExtension(filePath, ".pok");

            if (!File.Exists(pokeFilePath))
            {
                return;
            }

            _pokeFile = PokeFile.Load(pokeFilePath);
        }
        catch
        {
            // Ignore errors
        }
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
        BorderLeft = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Left,
        BorderRight = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Right,
        BorderTop = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Top,
        BorderBottom = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Bottom,
        ScalingFactor = _preferences.Recording.ScalingFactor,
        ScalingAlgorithm = _preferences.Recording.ScalingAlgorithm,
        FFmpegPath = _preferences.Recording.FFmpegPath,
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

    private async Task HandleChangeRomAsync(RomType romType)
    {
        var oldRomType = RomType;
        RomType = romType;

        byte[]? customRom = null;

        try
        {
            if (romType == RomType.Custom)
            {
                var files = await FileDialogs.OpenCustomRomFileAsync();

                if (files.Count <= 0)
                {
                    RomType = oldRomType;
                    return;
                }

                customRom = await File.ReadAllBytesAsync(files[0].Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }

        CreateEmulator(ComputerType, RomType, customRom);
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        ComputerType = computerType;

        if (RomType == RomType.Custom)
        {
            RomType = RomType.Original;
        }

        CreateEmulator(ComputerType, RomType);
    }

    private void HandleChangeJoystickType(JoystickType joystickType)
    {
        JoystickType = joystickType;
        Emulator?.JoystickManager.SetupJoystick(joystickType);
    }

    private void HandleChangeMouseType(MouseType mouseType)
    {
        MouseType = mouseType;
        Emulator?.MouseManager.SetupMouse(mouseType);
        _mouseHelper = new MouseHelper(Emulator!.MouseManager);
    }

    private void HandleToggleUlaPlus()
    {
        IsUlaPlusEnabled = !IsUlaPlusEnabled;

        if (Emulator != null)
        {
            Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        }
    }

    private void HandleTriggerNmi() => Emulator?.RequestNmi();

    private void HandleMachineReset()
    {
        _pokeFile = null;

        Emulator?.Reset();
        IsPaused = Emulator?.IsPaused ?? false;

        RecentFilesViewModel.CurrentFileName = string.Empty;
        UpdateWindowTitle();
    }

    private void HandleMachineHardReset()
    {
        _pokeFile = null;

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

    private void Pause(bool showOverlay = true)
    {
        if (IsPaused)
        {
            return;
        }

        Emulator?.Pause();
        IsPaused = true;

        if (showOverlay)
        {
            IsPauseOverlayVisible = true;
        }
    }

    private void Resume()
    {
        Emulator?.Resume();
        IsPaused = false;
        IsPauseOverlayVisible = false;
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
    }

    private void HandleSpectrumKeyPressed(object? sender, SpectrumKeyEventArgs e)
    {
        if (MainWindow?.IsActive != true)
        {
            return;
        }

        if (JoystickHandled(e))
        {
            return;
        }

        Emulator?.KeyboardState.KeyDown(e.Keys);
    }

    private void HandleSpectrumKeyReleased(object? sender, SpectrumKeyEventArgs e)
    {
        if (MainWindow?.IsActive != true || IsPaused)
        {
            return;
        }

        if (JoystickHandled(e))
        {
            return;
        }

        Emulator?.KeyboardState.KeyUp(e.Keys);
    }

    private bool JoystickHandled(SpectrumKeyEventArgs e)
    {
        if (!IsKeyboardJoystickEmulationEnabled)
        {
            return false;
        }

        var joystickInput = KeyboardHook.ToJoystickAction(e.KeyCode, _preferences.Joystick.FireKey);

        if (joystickInput == JoystickInput.None)
        {
            return false;
        }

        if (e.IsKeyPressed)
        {
            Emulator?.JoystickManager.Pressed(joystickInput);
        }
        else
        {
            Emulator?.JoystickManager.Released(joystickInput);
        }

        return true;

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
        IsPauseOverlayVisible = false;
        IsTimeMachineCountdownVisible = false;

        Emulator?.Resume();
    }

    private void HandleTakeScreenshot() => _screenshotViewModel.AddScreenshot(SpectrumScreen);

    public void HandleMouseMoved(Point position, Rect bounds) =>
        _mouseHelper?.MouseMoved(BorderSize, position, bounds);

    public void HandleMouseButtonStateChanged(PointerPoint point, Rect bounds) =>
        _mouseHelper?.ButtonsStateChanged(point);
}