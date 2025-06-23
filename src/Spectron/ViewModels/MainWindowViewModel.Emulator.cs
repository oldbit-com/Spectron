using System;
using System.IO;
using System.Threading.Tasks;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
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

        emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
        emulator.MouseManager.SetupMouse(_preferences.Mouse.MouseType);
        _mouseHelper = new MouseHelper(emulator.MouseManager);
        emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);

        InitializeEmulator(emulator);
    }

    private void CreateEmulator(StateSnapshot stateSnapshot)
    {
        Emulator?.Reset();

        var emulator = _stateManager.CreateEmulator(stateSnapshot);
        _mouseHelper = new MouseHelper(emulator.MouseManager);

        InitializeEmulator(emulator);
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

            emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
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

    private void InitializeEmulator(Emulator emulator)
    {
        ShutdownEmulator();

        Emulator = emulator;
        IsPaused = false;

        ComputerType = Emulator.ComputerType;
        RomType = Emulator.RomType;
        JoystickType = Emulator.JoystickManager.JoystickType;
        MouseType = Emulator.MouseManager.MouseType;
        IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;

        Emulator.TapeLoadSpeed = TapeLoadSpeed;
        Emulator.FrameCompleted += EmulatorFrameCompleted;

        ConfigureEmulatorSettings();
        ConfigureDebugging(Emulator);

        if (IsMuted)
        {
            Emulator.AudioManager.Mute();
        }

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.CommandManager.CommandReceived += CommandManagerOnCommandReceived;

        _debuggerViewModel?.ConfigureEmulator(Emulator);

        Emulator.Start();
    }

    private void ConfigureEmulatorSettings()
    {
        Emulator.SetFloatingBusSupport(_preferences.IsFloatingBusEnabled);
        Emulator.SetAudioSettings(_preferences.Audio);
        Emulator.SetGamepad(_preferences.Joystick);
        Emulator.SetDivMMc(_preferences.DivMmc);
        Emulator.SetPrinter(_preferences.Printer);

        StatusBarViewModel.IsDivMmcEnabled = _preferences.DivMmc.IsEnabled;
        StatusBarViewModel.IsPrinterEnabled = _preferences.Printer.IsZxPrinterEnabled;
        StatusBarViewModel.IsUlaPlusEnabled = IsUlaPlusEnabled;
        StatusBarViewModel.IsTapeLoaded = Emulator!.TapeManager.IsTapeLoaded;
        StatusBarViewModel.TapeLoadProgress = string.Empty;
    }

    private void ConfigureDebugging(Emulator emulator)
    {
        if (_breakpointHandler == null)
        {
            _breakpointHandler = new BreakpointHandler(emulator.Cpu, emulator.Memory);
        }
        else
        {
            _breakpointHandler.Update(emulator.Cpu);
        }

        // TODO: Add breakpoint handler and open debugger window
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

    private void HandleMachineReset(bool hardReset = false)
    {
        _pokeFile = null;

        if (hardReset)
        {
            CreateEmulator(_preferences.ComputerType, _preferences.RomType);
        }
        else
        {
            Emulator?.Reset();
            IsPaused = Emulator?.IsPaused ?? false;
        }

        RecentFilesViewModel.CurrentFileName = string.Empty;
        UpdateWindowTitle();
    }

    private void HandleTimeMachineResumeEmulator()
    {
        IsPauseOverlayVisible = false;
        IsTimeMachineCountdownVisible = false;

        Emulator?.Resume();
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

    partial void OnIsPausedChanged(bool value) =>
        _debuggerViewModel?.HandlePause(value);

    partial void OnComputerTypeChanged(ComputerType value) =>
        StatusBarViewModel.ComputerType = value;

    partial void OnIsUlaPlusEnabledChanged(bool value)
    {
        StatusBarViewModel.IsUlaPlusEnabled = IsUlaPlusEnabled;

        if (Emulator != null)
        {
            Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        }
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

    partial void OnTapeLoadSpeedChanged(TapeSpeed value) =>
        Emulator?.SetTapeLoadingSpeed(TapeLoadSpeed);

    partial void OnIsTimeMachineEnabledChanged(bool value)
    {
        _timeMachine.IsEnabled = value;
        NotifyCanExecuteChanged(nameof(ShowTimeMachineViewCommand));
    }
}