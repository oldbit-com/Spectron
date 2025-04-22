using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Theming;
using ReactiveUI;
using SharpHook.Native;

namespace OldBit.Spectron.ViewModels;

public class PreferencesViewModel : ReactiveObject, IDisposable
{
    private readonly GamepadManager _gamepadManager;
    private readonly GamepadSettings _gamepadSettings;

    public ReactiveCommand<Unit, Preferences> UpdatePreferencesCommand { get; }
    public ReactiveCommand<Unit, Unit> ProbeFFmpegCommand { get; }
    public ReactiveCommand<string, Task> SelectSdCardImageFile { get; }

    public Interaction<GamepadMappingViewModel, List<GamepadMapping>?> ShowGamepadMappingView { get; }

    public GamepadMappingViewModel GamepadMappingViewModel { get; }

    public PreferencesViewModel(Preferences preferences, GamepadManager gamepadManager)
    {
        _gamepadManager = gamepadManager;
        _gamepadSettings = preferences.Joystick.GamepadSettings;

        GamepadControllers = new ObservableCollection<GamepadController>(_gamepadManager.Controllers);
        GamepadMappingViewModel = new GamepadMappingViewModel(_gamepadManager);

        _gamepadManager.ControllerChanged += GamepadManagerOnControllerChanged;

        this.WhenAnyValue(x => x.GamepadControllerId)
            .Buffer(2, 1)
            .Select(b => (Previous: b[0], Current: b[1]))
            .Subscribe(value =>
            {
                if (value.Previous != Guid.Empty)
                {
                    _gamepadSettings.Mappings[value.Previous] = GamepadMappingViewModel.GetConfiguredMappings();
                }

                GamepadMappingViewModel.UpdateView(value.Current, _gamepadSettings);
            });

        this.WhenAnyValue(x => x.Theme).Subscribe(ThemeManager.SelectTheme);

        Theme = preferences.Theme;

        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        IsFloatingBusEnabled = preferences.IsFloatingBusEnabled;
        RomType = preferences.RomType;

        JoystickType = preferences.Joystick.JoystickType;
        EmulateUsingKeyboard = preferences.Joystick.EmulateUsingKeyboard;
        GamepadControllerId = _gamepadManager.Controllers.FirstOrDefault(
            controller => controller.ControllerId == preferences.Joystick.GamepadControllerId)?.ControllerId ?? GamepadController.None.ControllerId;
        FireKey = preferences.Joystick.FireKey;

        IsKempstonMouseEnabled = preferences.Mouse.IsKempstonMouseEnabled;
        IsStandardMousePointerHidden = preferences.Mouse.IsStandardMousePointerHidden;

        IsResumeEnabled = preferences.ResumeSettings.IsResumeEnabled;
        ShouldIncludeTapeInResume = preferences.ResumeSettings.ShouldIncludeTape;
        ShouldIncludeTimeMachineInResume = preferences.ResumeSettings.ShouldIncludeTimeMachine;

        IsBeeperEnabled = preferences.AudioSettings.IsBeeperEnabled;
        IsAyEnabled = preferences.AudioSettings.IsAyAudioEnabled;
        IsAySupportedStandardSpectrum = preferences.AudioSettings.IsAySupportedStandardSpectrum;
        StereoMode = preferences.AudioSettings.StereoMode;

        IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
        SnapshotInterval = preferences.TimeMachine.SnapshotInterval.TotalSeconds;
        MaxDuration = preferences.TimeMachine.MaxDuration.TotalSeconds;
        TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

        IsAutoPlayEnabled = preferences.TapeSettings.IsAutoPlayEnabled;
        IsTapeSaveEnabled = preferences.TapeSettings.IsSaveEnabled;
        TapeSaveSpeed = preferences.TapeSettings.SaveSpeed;
        TapeLoadSpeed = preferences.TapeSettings.LoadSpeed;

        RecordingBorderSize = preferences.RecordingSettings.BorderSize;
        ScalingFactor = preferences.RecordingSettings.ScalingFactor;
        ScalingAlgorithm = preferences.RecordingSettings.ScalingAlgorithm;
        FFmpegPath = preferences.RecordingSettings.FFmpegPath;

        UpdatePreferencesCommand = ReactiveCommand.Create(UpdatePreferences);
        ProbeFFmpegCommand = ReactiveCommand.Create(ProbeFFmpeg);
        SelectSdCardImageFile = ReactiveCommand.Create<string, Task>(HandleOpenSdCardImageFile);

        DebuggerPreferredNumberFormat = preferences.DebuggerSettings.PreferredNumberFormat;

        IsDivMmcEnabled = preferences.DivMmcSettings.IsEnabled;
        IsDivMmcWriteEnabled = preferences.DivMmcSettings.IsEepromWriteEnabled;
        DivMmcCard0FileName = preferences.DivMmcSettings.Card0FileName;
        DivMmcCard1FileName = preferences.DivMmcSettings.Card1FileName;
        IsDivMmcDriveWriteEnabled = preferences.DivMmcSettings.IsDriveWriteEnabled;

        ShowGamepadMappingView = new Interaction<GamepadMappingViewModel, List<GamepadMapping>?>();
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

    private Preferences UpdatePreferences()
    {
        if (GamepadControllerId != GamepadController.None.ControllerId)
        {
            _gamepadSettings.Mappings[GamepadControllerId] = GamepadMappingViewModel.GetConfiguredMappings();
        }

        return new Preferences
        {
            Theme = Theme,
            ComputerType = ComputerType,
            IsUlaPlusEnabled = IsUlaPlusEnabled,
            IsFloatingBusEnabled = IsFloatingBusEnabled,
            RomType = RomType,
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
                IsKempstonMouseEnabled = IsKempstonMouseEnabled,
                IsStandardMousePointerHidden = IsStandardMousePointerHidden,
            },

            ResumeSettings = new ResumeSettings
            {
                IsResumeEnabled = IsResumeEnabled,
                ShouldIncludeTape = ShouldIncludeTapeInResume,
                ShouldIncludeTimeMachine = ShouldIncludeTimeMachineInResume
            },

            AudioSettings = new AudioSettings
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

            TapeSettings = new TapeSettings
            {
                IsAutoPlayEnabled = IsAutoPlayEnabled,
                IsSaveEnabled = IsTapeSaveEnabled,
                SaveSpeed = TapeSaveSpeed,
                LoadSpeed = TapeLoadSpeed
            },

            RecordingSettings = new RecordingSettings
            {
                BorderSize = RecordingBorderSize,
                ScalingFactor = ScalingFactor,
                ScalingAlgorithm = ScalingAlgorithm,
                FFmpegPath = FFmpegPath
            },

            DebuggerSettings = new DebuggerSettings
            {
                PreferredNumberFormat = DebuggerPreferredNumberFormat,
            },

            DivMmcSettings = new DivMmcSettings
            {
                IsEnabled = IsDivMmcEnabled,
                IsEepromWriteEnabled = IsDivMmcWriteEnabled,
                Card0FileName = DivMmcCard0FileName,
                Card1FileName = DivMmcCard1FileName,
                IsDriveWriteEnabled = IsDivMmcDriveWriteEnabled,
            }
        };
    }

    private async Task HandleOpenSdCardImageFile(string cardId)
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

    private void ProbeFFmpeg()
    {
        FFmpegMessage = MediaRecorder.VerifyDependencies(FFmpegPath) ?
            "Success. FFmpeg found" :
            "Failure. FFmpeg not found";
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

    private Theme _theme;
    public Theme Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    private ComputerType _computerType;
    public ComputerType ComputerType
    {
        get => _computerType;
        set => this.RaiseAndSetIfChanged(ref _computerType, value);
    }

    private bool _isUlaPlusEnabled;
    public bool IsUlaPlusEnabled
    {
        get => _isUlaPlusEnabled;
        set => this.RaiseAndSetIfChanged(ref _isUlaPlusEnabled, value);
    }

    private RomType _romType;
    public RomType RomType
    {
        get => _romType;
        set => this.RaiseAndSetIfChanged(ref _romType, value);
    }

    private JoystickType _joystickType = JoystickType.None;
    public JoystickType JoystickType
    {
        get => _joystickType;
        set => this.RaiseAndSetIfChanged(ref _joystickType, value);
    }

    private bool _emulateUsingKeyboard;
    public bool EmulateUsingKeyboard
    {
        get => _emulateUsingKeyboard;
        set => this.RaiseAndSetIfChanged(ref _emulateUsingKeyboard, value);
    }

    private KeyCode _fireKey = KeyCode.VcSpace;
    public KeyCode FireKey
    {
        get => _fireKey;
        set => this.RaiseAndSetIfChanged(ref _fireKey, value);
    }

    private Guid _gamepadControllerId = GamepadController.None.ControllerId;
    public Guid GamepadControllerId
    {
        get => _gamepadControllerId;
        set => this.RaiseAndSetIfChanged(ref _gamepadControllerId, value);
    }

    private bool _isKempstonMouseEnabled;
    public bool IsKempstonMouseEnabled
    {
        get => _isKempstonMouseEnabled;
        set => this.RaiseAndSetIfChanged(ref _isKempstonMouseEnabled, value);
    }

    private bool _isStandardMousePointerHidden;
    public bool IsStandardMousePointerHidden
    {
        get => _isStandardMousePointerHidden;
        set => this.RaiseAndSetIfChanged(ref _isStandardMousePointerHidden, value);
    }

    private bool _isTimeMachineEnabled;
    public bool IsTimeMachineEnabled
    {
        get => _isTimeMachineEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTimeMachineEnabled, value);
    }

    private double _snapshotInterval;
    public double SnapshotInterval
    {
        get => _snapshotInterval;
        set => this.RaiseAndSetIfChanged(ref _snapshotInterval, value);
    }

    private double _maxDuration;
    public double MaxDuration
    {
        get => _maxDuration;
        set => this.RaiseAndSetIfChanged(ref _maxDuration, value);
    }

    private bool _isResumeEnabled;
    public bool IsResumeEnabled
    {
        get => _isResumeEnabled;
        set => this.RaiseAndSetIfChanged(ref _isResumeEnabled, value);
    }

    private bool _shouldIncludeTapeInResume;
    public bool ShouldIncludeTapeInResume
    {
        get => _shouldIncludeTapeInResume;
        set => this.RaiseAndSetIfChanged(ref _shouldIncludeTapeInResume, value);
    }

    private bool _shouldIncludeTimeMachineInResume;
    public bool ShouldIncludeTimeMachineInResume
    {
        get => _shouldIncludeTimeMachineInResume;
        set => this.RaiseAndSetIfChanged(ref _shouldIncludeTimeMachineInResume, value);
    }

    private bool _isAutoPlayEnabled;
    public bool IsAutoPlayEnabled
    {
        get => _isAutoPlayEnabled;
        set => this.RaiseAndSetIfChanged(ref _isAutoPlayEnabled, value);
    }

    private bool _isTapeSaveEnabled;
    public bool IsTapeSaveEnabled
    {
        get => _isTapeSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTapeSaveEnabled, value);
    }

    private TapeSpeed _tapeSaveSpeed = TapeSpeed.Normal;
    public TapeSpeed TapeSaveSpeed
    {
        get => _tapeSaveSpeed;
        set => this.RaiseAndSetIfChanged(ref _tapeSaveSpeed, value);
    }

    private TapeSpeed _tapeLoadSpeed = TapeSpeed.Normal;
    public TapeSpeed TapeLoadSpeed
    {
        get => _tapeLoadSpeed;
        set => this.RaiseAndSetIfChanged(ref _tapeLoadSpeed, value);
    }

    private bool _isFloatingBusEnabled;
    public bool IsFloatingBusEnabled
    {
        get => _isFloatingBusEnabled;
        set => this.RaiseAndSetIfChanged(ref _isFloatingBusEnabled, value);
    }

    private bool _isBeeperEnabled;
    public bool IsBeeperEnabled
    {
        get => _isBeeperEnabled;
        set => this.RaiseAndSetIfChanged(ref _isBeeperEnabled, value);
    }

    private bool _isAyEnabled;
    public bool IsAyEnabled
    {
        get => _isAyEnabled;
        set => this.RaiseAndSetIfChanged(ref _isAyEnabled, value);
    }

    private bool _isAySupportedStandardSpectrum;
    public bool IsAySupportedStandardSpectrum
    {
        get => _isAySupportedStandardSpectrum;
        set => this.RaiseAndSetIfChanged(ref _isAySupportedStandardSpectrum, value);
    }

    private StereoMode _stereoMode;
    public StereoMode StereoMode
    {
        get => _stereoMode;
        set => this.RaiseAndSetIfChanged(ref _stereoMode, value);
    }

    private BorderSize _recordingBorderSize = BorderSize.Medium;
    public BorderSize RecordingBorderSize
    {
        get => _recordingBorderSize;
        set => this.RaiseAndSetIfChanged(ref _recordingBorderSize, value);
    }

    private string _scalingAlgorithm = "neighbor";
    public string ScalingAlgorithm
    {
        get => _scalingAlgorithm;
        set => this.RaiseAndSetIfChanged(ref _scalingAlgorithm, value);
    }

    private int _scalingFactor = 2;
    public int ScalingFactor
    {
        get => _scalingFactor;
        set => this.RaiseAndSetIfChanged(ref _scalingFactor, value);
    }

    private string _ffmpegPath = string.Empty;
    public string FFmpegPath
    {
        get => _ffmpegPath;
        set => this.RaiseAndSetIfChanged(ref _ffmpegPath, value);
    }

    private string _ffmpegMessage = string.Empty;
    public string FFmpegMessage
    {
        get => _ffmpegMessage;
        set => this.RaiseAndSetIfChanged(ref _ffmpegMessage, value);
    }

    private int _timeMachineCountdownSeconds = 3;
    public int TimeMachineCountdownSeconds
    {
        get => _timeMachineCountdownSeconds;
        set => this.RaiseAndSetIfChanged(ref _timeMachineCountdownSeconds, value);
    }

    private NumberFormat _debuggerPreferredNumberFormat = NumberFormat.HexPrefixDollar;
    public NumberFormat DebuggerPreferredNumberFormat
    {
        get => _debuggerPreferredNumberFormat;
        set => this.RaiseAndSetIfChanged(ref _debuggerPreferredNumberFormat, value);
    }

    private bool _isDivMmcEnabled;
    public bool IsDivMmcEnabled
    {
        get => _isDivMmcEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDivMmcEnabled, value);
    }

    private bool _isDivMmcWriteEnabled;
    public bool IsDivMmcWriteEnabled
    {
        get => _isDivMmcWriteEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDivMmcWriteEnabled, value);
    }

    private string _divMmcCard0FileName = string.Empty;
    [CustomValidation(typeof(PreferencesViewModel), nameof(ValidateCardImageFile))]
    public string DivMmcCard0FileName
    {
        get => _divMmcCard0FileName;
        set => this.RaiseAndSetIfChanged(ref _divMmcCard0FileName, value);
    }

    private string _divMmcCard1FileName = string.Empty;
    [CustomValidation(typeof(PreferencesViewModel), nameof(ValidateCardImageFile))]
    public string DivMmcCard1FileName
    {
        get => _divMmcCard1FileName;
        set => this.RaiseAndSetIfChanged(ref _divMmcCard1FileName, value);
    }

    private bool _isDivMmcDriveWriteEnabled;
    public bool IsDivMmcDriveWriteEnabled
    {
        get => _isDivMmcDriveWriteEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDivMmcDriveWriteEnabled, value);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _gamepadManager.ControllerChanged -= GamepadManagerOnControllerChanged;
        GamepadMappingViewModel.Dispose();
    }
}