using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Rzx;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Input;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

partial class MainViewModel
{
    private void CreateEmulator(ComputerType computerType, RomType romType, int clockMultiplier, byte[]? customRom = null, bool hardReset = false)
    {
        var emulator = _emulatorFactory.Create(computerType, romType, clockMultiplier, customRom);

        ApplyEmulatorDefaults(emulator, hardReset);

        Initialize(emulator);
    }

    private void ApplyEmulatorDefaults(Emulator emulator, bool hardReset = false, FavoriteProgram? favorite = null)
    {
        emulator.IsUlaPlusEnabled = hardReset ? _preferences.IsUlaPlusEnabled : favorite?.IsUlaPlusEnabled ?? IsUlaPlusEnabled;
        emulator.IsFloatingBusEnabled = _preferences.IsFloatingBusEnabled;
        emulator.JoystickManager.Configure(hardReset ? _preferences.Joystick.JoystickType : favorite?.JoystickType ?? JoystickType);
        emulator.Printer.IsEnabled = _preferences.Printer.IsZxPrinterEnabled;
        emulator.MouseManager.Configure(hardReset ? _preferences.Mouse.MouseType : favorite?.MouseType ?? MouseType);
        emulator.TapeManager.TapeLoadSpeed = hardReset ? _preferences.Tape.LoadSpeed : favorite?.TapeLoadSpeed ?? TapeLoadSpeed;
        emulator.TapeManager.TapeSaveSpeed = _preferences.Tape.SaveSpeed;

        if (favorite?.IsDivMmcEnabled ?? _preferences.DivMmc.IsEnabled)
        {
            emulator.DivMmc.Enable();
            emulator.ConfigureDivMMc(_preferences.DivMmc);
        }

        if (favorite?.IsBeta128Enabled ?? _preferences.Beta128.IsEnabled)
        {
            emulator.Beta128.Enable();
        }

        if (favorite?.IsInterface1Enabled ?? _preferences.Interface1.IsEnabled)
        {
            emulator.Interface1.Enable();
        }

        emulator.ConfigureAudio(_preferences.Audio, favorite);

       _mouseInputHandler = new MouseInputHandler(emulator.MouseManager);
    }

    private bool CreateEmulator(StateSnapshot snapshot, bool shouldResume = true)
    {
        Emulator?.Reset();

        try
        {
            var emulator = _stateSnapshotManager.CreateEmulator(snapshot);
            _mouseInputHandler = new MouseInputHandler(emulator.MouseManager);

            Initialize(emulator, shouldResume);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore emulator state");
        }

        return false;
    }

    private bool CreateEmulator(Stream stream, FileType fileType, FavoriteProgram? favorite)
    {
        Emulator? emulator = null;

        if (fileType.IsSnapshot())
        {
            emulator = _snapshotManager.Load(stream, fileType);
            emulator.ConfigureAudio(_preferences.Audio);
        }
        else if (fileType.IsTape())
        {
            emulator = _loader.EnterLoadCommand(favorite?.ComputerType ?? ComputerType);
            emulator.TapeManager.InsertTape(stream, fileType,
                _preferences.Tape.IsAutoPlayEnabled && (favorite?.TapeLoadSpeed ?? TapeLoadSpeed) != TapeSpeed.Instant);

            ApplyEmulatorDefaults(emulator, favorite: favorite);
        }
        else if (fileType == FileType.Rzx)
        {
            _rzxController = new RzxController(_snapshotManager, stream);
            _rzxController.PlaybackProgressChanged += (_, e) => StatusBarViewModel.RzxPlayProgress = $"{e.Progress:P2}";
            _rzxController.PlaybackCompleted += (_, _) => StatusBarViewModel.RzxPlayProgress = "Completed";

            emulator = _rzxController.Emulator;
            emulator.ConfigureAudio(_preferences.Audio);
        }

        if (emulator != null)
        {
            Initialize(emulator);
        }

        return emulator != null;
    }

    private void Initialize(Emulator emulator, bool shouldResume = true)
    {
        ShutdownEmulator();

        Emulator = emulator;

        if (IsPaused)
        {
            Emulator.Pause();
        }

        ComputerType = Emulator.ComputerType;
        RomType = Emulator.RomType;
        JoystickType = Emulator.JoystickManager.JoystickType;
        MouseType = Emulator.MouseManager.MouseType;
        IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;

        Emulator.FrameCompleted += EmulatorFrameCompleted;

        Emulator.ConfigureGamepad(new JoystickSettings
        {
            GamepadControllerId = _preferences.Joystick.GamepadControllerId,
            JoystickType = JoystickType,
            GamepadSettings = _preferences.Joystick.GamepadSettings
        });

        if (Emulator.DivMmc.IsEnabled)
        {
            Emulator.ConfigureDivMMc(_preferences.DivMmc);
        }

        ConfigureMouseCursor();
        ConfigureDebugging(Emulator);

        if (IsAudioMuted)
        {
            Emulator.AudioManager.Mute();
        }

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.CommandManager.CommandReceived += CommandManagerOnCommandReceived;

        _debuggerViewModel?.ConfigureEmulator(Emulator);

        InitializeFrameBuffer();

        emulator.Clock.Multiplier = ClockMultiplier;
        emulator.EmulationSpeed = EmulationSpeed;

        Emulator.Start();

        if (shouldResume)
        {
            Resume();
        }

        RefreshControls();
    }

    private void ShutdownEmulator()
    {
        if (Emulator == null)
        {
            return;
        }

        Emulator.Shutdown();
        Emulator.FrameCompleted -= EmulatorFrameCompleted;
        Emulator.CommandManager.CommandReceived -= CommandManagerOnCommandReceived;

        Emulator = null;
    }

    private async Task<bool> ResumeEmulatorSession()
    {
        if (CommandLineArgs?.IsResumeEnabled == false ||
            (!_preferences.Resume.IsResumeEnabled && CommandLineArgs?.IsResumeEnabled == null) ||
            CommandLineArgs?.FilePath != null)
        {
            return false;
        }

        var snapshot = await _sessionService.LoadAsync();

        if (snapshot == null || !CreateEmulator(snapshot))
        {
            return false;
        }

        UpdateWindowTitle();

        return true;
    }

    private void HandleMachineReset(bool hardReset = false)
    {
        _pokeFile = null;

        if (hardReset)
        {
            CreateEmulator(_preferences.ComputerType, _preferences.RomType, ClockMultiplier, hardReset: true);
        }
        else if (Emulator != null)
        {
            Emulator.Reset();
            Resume();

            ConfigureDebugging(Emulator);
        }

        RecentFilesViewModel.CurrentFileName = string.Empty;
        UpdateWindowTitle();
    }

    private void HandleSetEmulationSpeed(int emulationSpeed)
    {
        EmulationSpeed = emulationSpeed;
        StatusBarViewModel.Speed = emulationSpeed == -1 ? "Max" : $"{emulationSpeed}%";
        Emulator?.EmulationSpeed = emulationSpeed;
    }

    private void HandleSetClockMultiplier(int clockMultiplier)
    {
        ClockMultiplier = clockMultiplier;
        StatusBarViewModel.Clock = clockMultiplier == 1 ? "" : $"{3.5 * clockMultiplier} MHz";
        Emulator?.Clock.Multiplier = clockMultiplier;
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
                var files = await _fileDialogs.OpenCustomRomFileAsync();

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
            await _messageDialogs.Error(ex.Message);
        }

        CreateEmulator(ComputerType, RomType, ClockMultiplier, customRom);
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        ComputerType = computerType;

        if (RomType == RomType.Custom)
        {
            RomType = RomType.Original;
        }

        CreateEmulator(ComputerType, RomType, ClockMultiplier);
    }

    partial void OnIsPausedChanged(bool value) => _debuggerViewModel?.HandlePause(value, _breakpointHitEventArgs);

    partial void OnComputerTypeChanged(ComputerType value) => StatusBarViewModel.ComputerType = value;

    partial void OnIsUlaPlusEnabledChanged(bool value)
    {
        IsUlaPlusEnabled = value;
        Emulator?.IsUlaPlusEnabled = IsUlaPlusEnabled;
        StatusBarViewModel.IsUlaPlusEnabled = Emulator?.IsUlaPlusEnabled ?? false;
    }

    private void RefreshControls()
    {
        if (Emulator == null)
        {
            return;
        }

        StatusBarViewModel.IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;

        StatusBarViewModel.IsDivMmcEnabled = Emulator.DivMmc.IsEnabled;
        StatusBarViewModel.IsTapeInserted = Emulator.TapeManager.IsTapeLoaded;
        StatusBarViewModel.TapeLoadProgress = string.Empty;

        StatusBarViewModel.IsMicroDriveCartridgeInserted =
            Emulator.Interface1.IsEnabled &&
            Emulator.MicrodriveManager.Microdrives.Values.Any(drive => drive.IsCartridgeInserted);

        StatusBarViewModel.IsFloppyDiskInserted =
            Emulator.Beta128.IsEnabled &&
            Emulator.DiskDriveManager.Drives.Values.Any(drive => drive.IsDiskInserted);

        StatusBarViewModel.IsAyEnabled = Emulator.AudioManager.IsAyEnabled;
        StatusBarViewModel.StereoMode = Emulator.AudioManager.StereoMode;

        IsBeta128Enabled = Emulator.Beta128.IsEnabled;
        NumberOfBeta128Drives = _preferences.Beta128.NumberOfDrives;

        IsInterface1Enabled = Emulator.Interface1.IsEnabled;
        NumberOfMicrodrives = _preferences.Interface1.NumberOfDrives;

        StatusBarViewModel.IsRzxPlaying = _rzxController != null;
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

    private void Resume(bool isDebuggerResume = false)
    {
        Emulator?.Resume(isDebuggerResume);
        IsPaused = false;
        IsPauseOverlayVisible = false;
        IsTimeMachineCountdownVisible = false;
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

    private static async Task<byte[]> ReadCustomRom(string[] fileNames)
    {
        byte[] result = [];

        foreach (var fileName in fileNames)
        {
            var data = await File.ReadAllBytesAsync(fileName);

            result = result.Concatenate(data);
        }

        return result;
    }

    partial void OnTapeLoadSpeedChanged(TapeSpeed value) =>
        Emulator?.TapeManager.TapeLoadSpeed = TapeLoadSpeed;

    partial void OnIsTimeMachineEnabledChanged(bool value) =>
        _timeMachine.IsEnabled = value;
}