using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Input;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void CreateEmulator(ComputerType computerType, RomType romType, byte[]? customRom = null)
    {
        var emulator = _emulatorFactory.Create(computerType, romType, customRom);

        emulator.IsUlaPlusEnabled = _preferences.IsUlaPlusEnabled;
        IsUlaPlusEnabled = _preferences.IsUlaPlusEnabled;

        emulator.MouseManager.SetupMouse(_preferences.Mouse.MouseType);
        _mouseHelper = new MouseHelper(emulator.MouseManager);
        emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);

        InitializeEmulator(emulator);
    }

    private bool CreateEmulator(StateSnapshot snapshot, bool shouldResume = true)
    {
        Emulator?.Reset();

        try
        {
            var emulator = _stateManager.CreateEmulator(snapshot);
            _mouseHelper = new MouseHelper(emulator.MouseManager);

            InitializeEmulator(emulator, shouldResume);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore emulator state");
        }

        return false;
    }

    private bool CreateEmulator(Stream stream, FileType fileType)
    {
        Emulator? emulator = null;

        if (fileType.IsSnapshot())
        {
            emulator = _snapshotManager.Load(stream, fileType);
        }
        else if (fileType.IsTape())
        {
            emulator = _loader.EnterLoadCommand(ComputerType);
            emulator.TapeManager.InsertTape(stream, fileType,
                _preferences.Tape.IsAutoPlayEnabled && TapeLoadSpeed != TapeSpeed.Instant);

            emulator.MouseManager.SetupMouse(_preferences.Mouse.MouseType);
            _mouseHelper = new MouseHelper(emulator.MouseManager);
            emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);
        }

        if (emulator != null)
        {
            InitializeEmulator(emulator);
        }

        return emulator != null;
    }

    private void InitializeEmulator(Emulator emulator, bool shouldResume = true)
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

        Emulator.TapeManager.TapeLoadSpeed = TapeLoadSpeed;
        Emulator.FrameCompleted += EmulatorFrameCompleted;

        RefreshUlaPlusState(Emulator.IsUlaPlusEnabled);

        ConfigureEmulator();
        ConfigureDebugging(Emulator);

        if (IsAudioMuted)
        {
            Emulator.AudioManager.Mute();
        }

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.CommandManager.CommandReceived += CommandManagerOnCommandReceived;

        _debuggerViewModel?.ConfigureEmulator(Emulator);

        Emulator.Start();

        if (shouldResume)
        {
            Resume();
        }
    }

    private void ConfigureEmulator()
    {
        Emulator.SetFloatingBusSupport(_preferences.IsFloatingBusEnabled);
        Emulator.SetAudioSettings(_preferences.Audio);
        Emulator.SetGamepad(_preferences.Joystick);
        Emulator.SetDivMMc(_preferences.DivMmc);

        RefreshAyState(Emulator?.AudioManager.IsAyEnabled, Emulator?.AudioManager.StereoMode);
        RefreshPrinterState(_preferences.Printer.IsZxPrinterEnabled);
        RefreshInterface1State(_preferences.Interface1.IsEnabled, _preferences.Interface1.RomVersion);
        RefreshBeta128State(_preferences.Beta128.IsEnabled);

        RefreshStatusBar();
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
            CreateEmulator(_preferences.ComputerType, _preferences.RomType);
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

    partial void OnIsPausedChanged(bool value) => _debuggerViewModel?.HandlePause(value, _breakpointHitEventArgs);

    partial void OnComputerTypeChanged(ComputerType value) => StatusBarViewModel.ComputerType = value;

    partial void OnIsUlaPlusEnabledChanged(bool value) => RefreshUlaPlusState(value);

    private void RefreshUlaPlusState(bool? isEnabled)
    {
        if (isEnabled == null || Emulator == null)
        {
            return;
        }

        Emulator.IsUlaPlusEnabled = isEnabled.Value;
        StatusBarViewModel.IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;
    }

    private void RefreshPrinterState(bool? isEnabled)
    {
        if (isEnabled == null || Emulator == null)
        {
            return;
        }

        Emulator.Printer.IsEnabled = isEnabled.Value;
        StatusBarViewModel.IsPrinterEnabled = Emulator.Printer.IsEnabled;
    }

    private void RefreshInterface1State(bool? isEnabled, Interface1RomVersion? romVersion = null)
    {
        if (isEnabled == null)
        {
            return;
        }

        if (isEnabled.Value)
        {
            Emulator?.Interface1.Enable();
        }
        else
        {
            Emulator?.Interface1.Disable();
        }

        if (romVersion != null && Emulator != null)
        {
            Emulator.Interface1.ShadowRom.Version = romVersion.Value;
        }

        IsInterface1Enabled = isEnabled == true;
        NumberOfMicrodrives = _preferences.Interface1.NumberOfDrives;
    }

    private void RefreshBeta128State(bool? isEnabled)
    {
        if (isEnabled == null)
        {
            return;
        }

        if (isEnabled.Value)
        {
            Emulator?.Beta128.Enable();
        }
        else
        {
            Emulator?.Beta128.Disable();
        }

        IsBeta128Enabled = isEnabled == true;
        NumberOfBeta128Drives = _preferences.Beta128.NumberOfDrives;
    }

    private void RefreshAyState(bool? isEnabled, StereoMode? stereoMode)
    {
        if (isEnabled == null || Emulator == null)
        {
            return;
        }

        Emulator.AudioManager.IsAyEnabled = isEnabled.Value;

        if (stereoMode != null)
        {
            Emulator.AudioManager.StereoMode = stereoMode.Value;
        }
    }

    private void RefreshStatusBar()
    {
        if (Emulator == null)
        {
            return;
        }

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

    partial void OnTapeLoadSpeedChanged(TapeSpeed value)
    {
        if (Emulator != null)
        {
            Emulator.TapeManager.TapeLoadSpeed = TapeLoadSpeed;
        }
    }

    partial void OnIsTimeMachineEnabledChanged(bool value)
    {
        _timeMachine.IsEnabled = value;
        NotifyCanExecuteChanged(nameof(ShowTimeMachineViewCommand));
    }
}