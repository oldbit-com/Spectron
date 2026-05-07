using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
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
        var snapshotUri = new Uri("file:///path/file.spectron");

        var builder = new MainViewModelBuilder()
            .WithSaveFilePicker(snapshotUri)
            .WithStateSnapshotStore(snapshotUri, snapshot => _stateSnapshot = snapshot);

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

    [AvaloniaFact]
    public async Task ShouldChangeComputerType()
    {
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Timex2048);

        //_viewModel.SpectrumScreen.ShouldNotBeNull();
        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(ComputerType.Timex2048);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _stateSnapshot?.ComputerType.ShouldBe(ComputerType.Timex2048);
    }

    [AvaloniaTheory]
    [InlineData(JoystickType.None)]
    [InlineData(JoystickType.Cursor)]
    [InlineData(JoystickType.Kempston)]
    [InlineData(JoystickType.Sinclair1)]
    [InlineData(JoystickType.Sinclair2)]
    [InlineData(JoystickType.Fuller)]
    public async Task ShouldChangeJoystickType(JoystickType joystickType)
    {
        _viewModel.ChangeJoystickTypeCommand.Execute(joystickType);
        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        _stateSnapshot?.Joystick.JoystickType.ShouldBe(joystickType);
        _viewModel.StatusBarViewModel.JoystickType.ShouldBe(joystickType);
    }
}
