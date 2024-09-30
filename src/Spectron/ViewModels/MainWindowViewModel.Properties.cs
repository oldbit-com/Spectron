using Avalonia.Controls;
using Avalonia.Media.Imaging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
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

    private string _windowStateCommandName = string.Empty;
    public string WindowStateCommandName
    {
        get => _windowStateCommandName;
        set => this.RaiseAndSetIfChanged(ref _windowStateCommandName, value);
    }

    private TapeSpeed _tapeLoadSpeed = TapeSpeed.Instant;
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
}