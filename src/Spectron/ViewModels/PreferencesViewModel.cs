using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Theming;
using SharpHook.Data;

namespace OldBit.Spectron.ViewModels;

public partial class PreferencesViewModel : ObservableValidator, IDisposable
{
    private readonly GamepadManager _gamepadManager;
    private readonly GamepadSettings _gamepadSettings;
    private Guid _previousGamepadControllerId = Guid.Empty;

    public GamepadMappingViewModel GamepadMappingViewModel { get; }

    public PreferencesViewModel(Preferences preferences, GamepadManager gamepadManager)
    {
        _gamepadManager = gamepadManager;
        _gamepadSettings = preferences.Joystick.GamepadSettings;

        GamepadControllers = new ObservableCollection<GamepadController>(_gamepadManager.Controllers);
        GamepadMappingViewModel = new GamepadMappingViewModel(_gamepadManager);

        _gamepadManager.ControllerChanged += GamepadManagerOnControllerChanged;

        Theme = preferences.Theme;

        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        IsFloatingBusEnabled = preferences.IsFloatingBusEnabled;
        RomType = preferences.RomType;
        IsAutoLoadPokeFilesEnabled = preferences.IsAutoLoadPokeFilesEnabled;

        JoystickType = preferences.Joystick.JoystickType;
        EmulateUsingKeyboard = preferences.Joystick.EmulateUsingKeyboard;
        GamepadControllerId = _gamepadManager.Controllers.FirstOrDefault(
            controller => controller.ControllerId == preferences.Joystick.GamepadControllerId)?.ControllerId ?? GamepadController.None.ControllerId;
        _previousGamepadControllerId = GamepadControllerId;
        FireKey = preferences.Joystick.FireKey;

        MouseType = preferences.Mouse.MouseType;
        IsStandardMousePointerHidden = preferences.Mouse.IsStandardMousePointerHidden;

        IsResumeEnabled = preferences.Resume.IsResumeEnabled;
        ShouldIncludeTapeInResume = preferences.Resume.ShouldIncludeTape;
        ShouldIncludeTimeMachineInResume = preferences.Resume.ShouldIncludeTimeMachine;

        IsBeeperEnabled = preferences.Audio.IsBeeperEnabled;
        IsAyEnabled = preferences.Audio.IsAyAudioEnabled;
        IsAySupportedStandardSpectrum = preferences.Audio.IsAySupportedStandardSpectrum;
        StereoMode = preferences.Audio.StereoMode;

        IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
        SnapshotInterval = preferences.TimeMachine.SnapshotInterval.TotalSeconds;
        MaxDuration = preferences.TimeMachine.MaxDuration.TotalSeconds;
        TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

        IsAutoPlayEnabled = preferences.Tape.IsAutoPlayEnabled;
        IsTapeSaveEnabled = preferences.Tape.IsSaveEnabled;
        TapeSaveSpeed = preferences.Tape.SaveSpeed;
        TapeLoadSpeed = preferences.Tape.LoadSpeed;

        RecordingBorderSize = preferences.Recording.BorderSize;
        ScalingFactor = preferences.Recording.ScalingFactor;
        ScalingAlgorithm = preferences.Recording.ScalingAlgorithm;
        FfmpegPath = preferences.Recording.FFmpegPath;

        DebuggerPreferredNumberFormat = preferences.Debugger.PreferredNumberFormat;

        IsDivMmcEnabled = preferences.DivMmc.IsEnabled;
        IsDivMmcWriteEnabled = preferences.DivMmc.IsEepromWriteEnabled;
        DivMmcCard0FileName = preferences.DivMmc.Card0FileName;
        DivMmcCard1FileName = preferences.DivMmc.Card1FileName;
        IsDivMmcDriveWriteEnabled = preferences.DivMmc.IsDriveWriteEnabled;

        IsZxPrinterEnabled = preferences.Printer.IsZxPrinterEnabled;
    }

    partial void OnThemeChanged(Theme value) => ThemeManager.SelectTheme(value);

    partial void OnGamepadControllerIdChanged(Guid value)
    {
        if (_previousGamepadControllerId != Guid.Empty)
        {
            _gamepadSettings.Mappings[_previousGamepadControllerId] = GamepadMappingViewModel.GetConfiguredMappings();
        }

        GamepadMappingViewModel.UpdateView(value, _gamepadSettings);

        _previousGamepadControllerId = value;
    }

    [RelayCommand]
    private void UpdatePreferences()
    {
        if (GamepadControllerId != GamepadController.None.ControllerId)
        {
            _gamepadSettings.Mappings[GamepadControllerId] = GamepadMappingViewModel.GetConfiguredMappings();
        }

        var preferences = new Preferences
        {
            Theme = Theme,
            ComputerType = ComputerType,
            IsUlaPlusEnabled = IsUlaPlusEnabled,
            IsFloatingBusEnabled = IsFloatingBusEnabled,
            RomType = RomType,
            IsAutoLoadPokeFilesEnabled = IsAutoLoadPokeFilesEnabled,
            Joystick = new JoystickSettings
            {
                JoystickType = JoystickType,
                EmulateUsingKeyboard = EmulateUsingKeyboard,
                GamepadControllerId = GamepadControllerId,
                GamepadSettings = _gamepadSettings,
                FireKey = FireKey
            },

            Mouse = new MouseSettings
            {
                MouseType = MouseType,
                IsStandardMousePointerHidden = IsStandardMousePointerHidden,
            },

            Resume = new ResumeSettings
            {
                IsResumeEnabled = IsResumeEnabled,
                ShouldIncludeTape = ShouldIncludeTapeInResume,
                ShouldIncludeTimeMachine = ShouldIncludeTimeMachineInResume
            },

            Audio = new AudioSettings
            {
                IsBeeperEnabled = IsBeeperEnabled,
                IsAyAudioEnabled = IsAyEnabled,
                IsAySupportedStandardSpectrum = IsAySupportedStandardSpectrum,
                StereoMode = StereoMode,
            },

            TimeMachine = new TimeMachineSettings
            {
                IsEnabled = IsTimeMachineEnabled,
                SnapshotInterval = TimeSpan.FromSeconds(SnapshotInterval),
                MaxDuration = TimeSpan.FromSeconds(MaxDuration),
                CountdownSeconds = TimeMachineCountdownSeconds,
            },

            Tape = new TapeSettings
            {
                IsAutoPlayEnabled = IsAutoPlayEnabled,
                IsSaveEnabled = IsTapeSaveEnabled,
                SaveSpeed = TapeSaveSpeed,
                LoadSpeed = TapeLoadSpeed
            },

            Recording = new RecordingSettings
            {
                BorderSize = RecordingBorderSize,
                ScalingFactor = ScalingFactor,
                ScalingAlgorithm = ScalingAlgorithm,
                FFmpegPath = FfmpegPath
            },

            Debugger = new DebuggerSettings
            {
                PreferredNumberFormat = DebuggerPreferredNumberFormat,
            },

            DivMmc = new DivMmcSettings
            {
                IsEnabled = IsDivMmcEnabled,
                IsEepromWriteEnabled = IsDivMmcWriteEnabled,
                Card0FileName = DivMmcCard0FileName,
                Card1FileName = DivMmcCard1FileName,
                IsDriveWriteEnabled = IsDivMmcDriveWriteEnabled,
            },

            Printer = new PrinterSettings
            {
                IsZxPrinterEnabled = IsZxPrinterEnabled,
            }
        };

        WeakReferenceMessenger.Default.Send(new UpdatePreferencesMessage(preferences));
    }

    [RelayCommand]
    private void ProbeFFmpeg()
    {
        FfmpegMessage = MediaRecorder.VerifyDependencies(FfmpegPath) ?
            "Success. FFmpeg found" :
            "Failure. FFmpeg not found";
    }

    [RelayCommand]
    private async Task SelectSdCardImageFile(string cardId)
    {
        var file = await FileDialogs.OpenDiskImageFileAsync();

        if (file.Count == 0)
        {
            return;
        }

        switch (cardId)
        {
            case "0":
                DivMmcCard0FileName = file[0].Path.LocalPath;
                break;

            case "1":
                DivMmcCard1FileName = file[0].Path.LocalPath;
                break;
        }
    }

    private void GamepadManagerOnControllerChanged(object? sender, ControllerChangedEventArgs e)
    {
        switch (e.Action)
        {
            case ControllerChangedAction.Added:
                Dispatcher.UIThread.Post(() =>
                {
                    GamepadControllers.Add(e.Controller);
                    GamepadControllerId = e.Controller.ControllerId;
                });

                break;

            case ControllerChangedAction.Removed:
                Dispatcher.UIThread.Post(() =>
                {
                    GamepadControllers.Remove(e.Controller);
                    GamepadControllerId = GamepadController.None.ControllerId;
                });

                break;
        }
    }

    public static ValidationResult? ValidateCardImageFile(string fileName, ValidationContext context)
    {
        if (context.ObjectInstance is PreferencesViewModel { IsDivMmcEnabled: false })
        {
            return ValidationResult.Success;
        }

        if (string.IsNullOrWhiteSpace(fileName) || DiskImage.Validate(fileName, out var errorMessage))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(errorMessage, [context.MemberName ?? string.Empty]);
    }

    public List<NameValuePair<ComputerType>> ComputerTypes { get; } =
    [
        new("ZX Spectrum 16k", ComputerType.Spectrum16K),
        new("ZX Spectrum 48k", ComputerType.Spectrum48K),
        new("ZX Spectrum 128k", ComputerType.Spectrum128K),
    ];

    public List<NameValuePair<RomType>> RomTypes { get; } =
    [
        new("Original", RomType.Original),
        new("Gosh Wonderful", RomType.GoshWonderful),
        new("Busy Soft v1.40", RomType.BusySoft),
        new("J.G. Harston v0.77", RomType.Harston),
        new("Diagnostic: Retroleum v1.71", RomType.Retroleum),
        new("Diagnostic: B. Alford v1.37", RomType.BrendanAlford),
    ];

    public List<NameValuePair<JoystickType>> JoystickTypes { get; } =
    [
        new("None", JoystickType.None),
        new("Kempston", JoystickType.Kempston),
        new("Sinclair 1", JoystickType.Sinclair1),
        new("Sinclair 2", JoystickType.Sinclair2),
        new("Cursor", JoystickType.Cursor),
        new("Fuller", JoystickType.Fuller),
    ];

    public List<NameValuePair<MouseType>> MouseTypes { get; } =
    [
        new("None", MouseType.None),
        new("Kempston", MouseType.Kempston),
    ];

    public List<NameValuePair<StereoMode>> StereoModes { get; } =
    [
        new("Mono", StereoMode.Mono),
        new("Stereo ABC", StereoMode.StereoAbc),
        new("Stereo ACB", StereoMode.StereoAcb),
    ];

    public List<NameValuePair<KeyCode>> FireKeys { get; } =
    [
        new("Space", KeyCode.VcSpace),
        new("Tab", KeyCode.VcTab),
        new("Enter", KeyCode.VcEnter),
        new("A", KeyCode.VcA),
        new("B", KeyCode.VcB),
        new("C", KeyCode.VcC),
        new("D", KeyCode.VcD),
        new("E", KeyCode.VcE),
        new("F", KeyCode.VcF),
        new("G", KeyCode.VcG),
        new("H", KeyCode.VcH),
        new("I", KeyCode.VcI),
        new("J", KeyCode.VcJ),
        new("K", KeyCode.VcK),
        new("L", KeyCode.VcL),
        new("M", KeyCode.VcM),
        new("N", KeyCode.VcN),
        new("O", KeyCode.VcO),
        new("P", KeyCode.VcP),
        new("Q", KeyCode.VcQ),
        new("R", KeyCode.VcR),
        new("S", KeyCode.VcS),
        new("T", KeyCode.VcT),
        new("U", KeyCode.VcU),
        new("V", KeyCode.VcV),
        new("W", KeyCode.VcW),
        new("X", KeyCode.VcX),
        new("Y", KeyCode.VcY),
        new("Z", KeyCode.VcZ),
        new("0", KeyCode.Vc0),
        new("1", KeyCode.Vc1),
        new("2", KeyCode.Vc2),
        new("3", KeyCode.Vc3),
        new("4", KeyCode.Vc4),
        new("5", KeyCode.Vc5),
        new("6", KeyCode.Vc6),
        new("7", KeyCode.Vc7),
        new("8", KeyCode.Vc8),
        new("9", KeyCode.Vc9),
    ];

    public List<NameValuePair<BorderSize>> BorderSizes { get; } =
    [
        new("None", BorderSize.None),
        new("Small", BorderSize.Small),
        new("Medium", BorderSize.Medium),
        new("Large", BorderSize.Large),
        new("Full", BorderSize.Full),
    ];

    public List<NameValuePair<int>> ScalingFactors { get; } =
    [
        new("1x", 1),
        new("2x", 2),
        new("3x", 3),
        new("4x", 4),
        new("5x", 5),
    ];

    public List<NameValuePair<string>> ScalingAlgorithms { get; } =
    [
        new("Fast Bilinear","fast_bilinear"),
        new("Bilinear", "bilinear"),
        new("Bicubic", "bicubic"),
        new("Experimental", "experimental"),
        new("Nearest Neighbor", "neighbor"),
        new("Area", "area"),
        new("Bicublin", "bicublin"),
        new("Gauss", "gauss"),
        new("Sinc", "sinc"),
        new("Lanczos", "lanczos"),
        new("Spline", "spline"),
    ];

    public List<NameValuePair<int>> TimeMachineCountdownValues { get; } =
    [
        new("0 seconds", 0),
        new("1 second", 1),
        new("2 seconds", 2),
        new("3 seconds", 3),
        new("4 seconds", 4),
        new("5 seconds", 5),
    ];

    public List<NameValuePair<NumberFormat>> HexNumberFormats { get; } =
    [
        new("0A2F", NumberFormat.Hex),
        new("$0A2F", NumberFormat.HexPrefixDollar),
        new("0A2Fh", NumberFormat.HexSuffixH),
        new("0x0A2F", NumberFormat.HexPrefix0X),
        new("#0A2F", NumberFormat.HexPrefixHash),
    ];

    public ObservableCollection<GamepadController> GamepadControllers { get; }

    [ObservableProperty]
    private Theme _theme;

    [ObservableProperty]
    private ComputerType _computerType;

    [ObservableProperty]
    private bool _isUlaPlusEnabled;

    [ObservableProperty]
    private RomType _romType;

    [ObservableProperty]
    private JoystickType _joystickType = JoystickType.None;

    [ObservableProperty]
    private MouseType _mouseType = MouseType.None;

    [ObservableProperty]
    private bool _emulateUsingKeyboard;

    [ObservableProperty]
    private KeyCode _fireKey = KeyCode.VcSpace;

    [ObservableProperty]
    private Guid _gamepadControllerId = GamepadController.None.ControllerId;

    [ObservableProperty]
    private bool _isStandardMousePointerHidden;

    [ObservableProperty]
    private bool _isTimeMachineEnabled;

    [ObservableProperty]
    private double _snapshotInterval;

    [ObservableProperty]
    private double _maxDuration;

    [ObservableProperty]
    private bool _isResumeEnabled;

    [ObservableProperty]
    private bool _shouldIncludeTapeInResume;

    [ObservableProperty]
    private bool _shouldIncludeTimeMachineInResume;

    [ObservableProperty]
    private bool _isAutoPlayEnabled;

    [ObservableProperty]
    private bool _isTapeSaveEnabled;

    [ObservableProperty]
    private TapeSpeed _tapeSaveSpeed = TapeSpeed.Normal;

    [ObservableProperty]
    private TapeSpeed _tapeLoadSpeed = TapeSpeed.Normal;

    [ObservableProperty]
    private bool _isFloatingBusEnabled;

    [ObservableProperty]
    private bool _isBeeperEnabled;

    [ObservableProperty]
    private bool _isAyEnabled;

    [ObservableProperty]
    private bool _isAySupportedStandardSpectrum;

    [ObservableProperty]
    private StereoMode _stereoMode;

    [ObservableProperty]
    private BorderSize _recordingBorderSize = BorderSize.Medium;

    [ObservableProperty]
    private string _scalingAlgorithm = "neighbor";

    [ObservableProperty]
    private int _scalingFactor = 2;

    [ObservableProperty]
    private string _ffmpegPath = string.Empty;

    [ObservableProperty()]
    private string _ffmpegMessage = string.Empty;

    [ObservableProperty]
    private int _timeMachineCountdownSeconds = 3;

    [ObservableProperty]
    private NumberFormat _debuggerPreferredNumberFormat = NumberFormat.HexPrefixDollar;

    [ObservableProperty]
    private bool _isDivMmcEnabled;

    [ObservableProperty]
    private bool _isDivMmcWriteEnabled;

    [ObservableProperty]
    [CustomValidation(typeof(PreferencesViewModel), nameof(ValidateCardImageFile))]
    private string _divMmcCard0FileName = string.Empty;

    [ObservableProperty]
    [CustomValidation(typeof(PreferencesViewModel), nameof(ValidateCardImageFile))]
    private string _divMmcCard1FileName = string.Empty;

    [ObservableProperty]
    private bool _isDivMmcDriveWriteEnabled;

    [ObservableProperty]
    private bool _isZxPrinterEnabled;

    [ObservableProperty]
    private bool _isAutoLoadPokeFilesEnabled;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _gamepadManager.ControllerChanged -= GamepadManagerOnControllerChanged;
        GamepadMappingViewModel.Dispose();
    }
}