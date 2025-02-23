using Avalonia.Controls;
using Avalonia.Media.Imaging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
using OldBit.Spectron.Screen;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private BorderSize _borderSize = BorderSize.Medium;
    public BorderSize BorderSize
    {
        get => _borderSize;
        set => this.RaiseAndSetIfChanged(ref _borderSize, value);
    }

    private RomType _romType = RomType.Original;
    public RomType RomType
    {
        get => _romType;
        set => this.RaiseAndSetIfChanged(ref _romType, value);
    }

    private ComputerType _computerType = ComputerType.Spectrum48K;
    public ComputerType ComputerType
    {
        get => _computerType;
        set => this.RaiseAndSetIfChanged(ref _computerType, value);
    }

    private JoystickType _joystickType = JoystickType.None;
    public JoystickType JoystickType
    {
        get => _joystickType;
        set => this.RaiseAndSetIfChanged(ref _joystickType, value);
    }

    private bool _isUlaPlusEnabled;
    public bool IsUlaPlusEnabled
    {
        get => _isUlaPlusEnabled;
        set => this.RaiseAndSetIfChanged(ref _isUlaPlusEnabled, value);
    }

    private WriteableBitmap? _spectrumScreen;
    public WriteableBitmap? SpectrumScreen
    {
        get => _spectrumScreen;
        set => this.RaiseAndSetIfChanged(ref _spectrumScreen, value);
    }

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    private bool _isTimeMachineCountdownVisible;
    public bool IsTimeMachineCountdownVisible
    {
        get => _isTimeMachineCountdownVisible;
        set => this.RaiseAndSetIfChanged(ref _isTimeMachineCountdownVisible, value);
    }

    private string _emulationSpeed = "100";
    public string EmulationSpeed
    {
        get => _emulationSpeed;
        set => this.RaiseAndSetIfChanged(ref _emulationSpeed, value);
    }

    private WindowState _windowState = WindowState.Normal;
    public WindowState WindowState
    {
        get => _windowState;
        set => this.RaiseAndSetIfChanged(ref _windowState, value);
    }

    private TapeSpeed _tapeLoadSpeed = TapeSpeed.Normal;
    public TapeSpeed TapeLoadSpeed
    {
        get => _tapeLoadSpeed;
        set => this.RaiseAndSetIfChanged(ref _tapeLoadSpeed, value);
    }

    private bool _isMuted;
    public bool IsMuted
    {
        get => _isMuted;
        set => this.RaiseAndSetIfChanged(ref _isMuted, value);
    }

    private bool _isTimeMachineEnabled;
    private bool IsTimeMachineEnabled
    {
        get => _isTimeMachineEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTimeMachineEnabled, value);
    }

    private string _title = DefaultTitle;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private RecordingStatus _recordingStatus = RecordingStatus.None;
    public RecordingStatus RecordingStatus
    {
        get => _recordingStatus;
        set => this.RaiseAndSetIfChanged(ref _recordingStatus, value);
    }
}