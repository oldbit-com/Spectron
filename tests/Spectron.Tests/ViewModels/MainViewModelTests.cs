using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Models;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        services.AddEmulation();
        services.AddDebugging();
        services.AddLogging();
        services.AddSingleton<ILogStore, InMemoryLogStore>();

        var applicationDataService = NSubstitute.Substitute.For<IApplicationDataService>();
        services.AddSingleton(applicationDataService);

        var provider = services.BuildServiceProvider();

        _viewModel = provider.GetRequiredService<MainViewModel>();
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
    public async Task Test()
    {
        await _viewModel.ChangeRomCommand.ExecuteAsync(RomType.Harston);

        _viewModel.SpectrumScreen.ShouldNotBeNull();
        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);
    }
}
