using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
using OldBit.Spectron.Screen;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelTests : IDisposable
{
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        var builder = new MainViewModelBuilder();

        _viewModel = builder.Build();

        EnsureEmulatorIsCreated();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _viewModel.Emulator?.Shutdown();
    }

    [AvaloniaFact]
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

        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);

        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.RomType.ShouldBe(RomType.Harston);
    }

    [AvaloniaTheory]
    [MemberData(nameof(AllComputerTypes))]
    public void ShouldChangeComputerType(ComputerType computerType)
    {
        _viewModel.ChangeComputerTypeCommand.Execute(computerType);

        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(computerType);

        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(computerType);
    }

    [AvaloniaTheory]
    [MemberData(nameof(AllJoystickTypes))]
    public void ShouldChangeJoystickType(JoystickType joystickType)
    {
        _viewModel.ChangeJoystickTypeCommand.Execute(joystickType);

        _viewModel.StatusBarViewModel.JoystickType.ShouldBe(joystickType);

        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.JoystickManager.JoystickType.ShouldBe(joystickType);
    }

    [AvaloniaFact]
    public void ShouldChangeMouseType()
    {
        _viewModel.ChangeMouseTypeCommand.Execute(MouseType.Kempston);

        _viewModel.StatusBarViewModel.IsMouseEnabled.ShouldBeTrue();

        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.MouseManager.MouseType.ShouldBe(MouseType.Kempston);
    }

    public static TheoryData<ComputerType> AllComputerTypes => new(Enum.GetValues<ComputerType>());
    public static TheoryData<JoystickType> AllJoystickTypes => new(Enum.GetValues<JoystickType>());

    private void EnsureEmulatorIsCreated() => _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum48K);
}
