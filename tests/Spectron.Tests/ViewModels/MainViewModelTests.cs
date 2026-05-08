using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
using OldBit.Spectron.Screen;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly MainViewModel _viewModel;
    private StateSnapshot? _stateSnapshot;

    public MainViewModelTests()
    {
        var builder = new MainViewModelBuilder()
            .WithFile("test.spectron")
            .WithSaveFilePicker()
            .WithStateSnapshotStore(snapshot => _stateSnapshot = snapshot);

        _viewModel = builder.Build();
    }

    [Fact]
    public void ViewModel_ShouldInitialize()
    {
        _viewModel.ShouldNotBeNull();
        _viewModel.RomType.ShouldBe(RomType.Original);
        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.JoystickType.ShouldBe(JoystickType.None);
        _viewModel.BorderSize.ShouldBe(BorderSize.Medium);
        _viewModel.TapeLoadSpeed.ShouldBe(TapeSpeed.Normal);
        _viewModel.IsAudioMuted.ShouldBeFalse();
        _viewModel.ScreenEffect.ShouldBe(ScreenEffect.None);
        _viewModel.IsMenuVisible.ShouldBeTrue();
        _viewModel.IsNativeMenuEnabled.ShouldBeFalse();
        _viewModel.IsUlaPlusEnabled.ShouldBeFalse();
        _viewModel.IsPaused.ShouldBeFalse();
        _viewModel.IsAudioMuted.ShouldBeFalse();
        _viewModel.IsTimeMachineEnabled.ShouldBeFalse();
        _viewModel.IsTimeMachineCountdownVisible.ShouldBeFalse();
        _viewModel.EmulationSpeed.ShouldBe("100");
        _viewModel.Title.ShouldBe("Spectron - ZX Spectrum Emulator");
        _viewModel.WindowState.ShouldBe(WindowState.Normal);
        _viewModel.IsFullScreen.ShouldBeFalse();
        _viewModel.RecordingStatus.ShouldBe(RecordingStatus.None);
        _viewModel.CanStartRecording.ShouldBeTrue();
        _viewModel.CanStopRecording.ShouldBeFalse();
        _viewModel.MouseCursor.ShouldBe(MouseCursors.Default);
        _viewModel.IsInterface1Enabled.ShouldBeFalse();
        _viewModel.IsBeta128Enabled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ShouldChangeRom()
    {
        await _viewModel.ChangeRomCommand.ExecuteAsync(RomType.Harston);

        //_viewModel.SpectrumScreen.ShouldNotBeNull();
        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _stateSnapshot?.CustomRom?.RomType.ShouldBe(RomType.Harston);
    }

    [AvaloniaTheory]
    [MemberData(nameof(AllComputerTypes))]
    public async Task ShouldChangeComputerType(ComputerType computerType)
    {
        _viewModel.ChangeComputerTypeCommand.Execute(computerType);
        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(computerType);
        _stateSnapshot?.ComputerType.ShouldBe(computerType);
    }

    [AvaloniaTheory]
    [MemberData(nameof(AllJoystickTypes))]
    public async Task ShouldChangeJoystickType(JoystickType joystickType)
    {
        _viewModel.ChangeJoystickTypeCommand.Execute(joystickType);
        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _stateSnapshot?.Joystick.JoystickType.ShouldBe(joystickType);
        _viewModel.StatusBarViewModel.JoystickType.ShouldBe(joystickType);
    }

    [AvaloniaFact]
    public async Task ShouldChangeMouseType()
    {
        _viewModel.ChangeMouseTypeCommand.Execute(MouseType.Kempston);
        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _stateSnapshot?.Mouse.MouseType.ShouldBe(MouseType.Kempston);
        _viewModel.StatusBarViewModel.IsMouseEnabled.ShouldBeTrue();
    }

    public static TheoryData<ComputerType> AllComputerTypes => new(Enum.GetValues<ComputerType>());
    public static TheoryData<JoystickType> AllJoystickTypes => new(Enum.GetValues<JoystickType>());
}
