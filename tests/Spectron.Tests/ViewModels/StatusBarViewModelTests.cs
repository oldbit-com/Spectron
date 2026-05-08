using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class StatusBarViewModelTests
{
    private readonly StatusBarViewModel _viewModel = new();

    [Theory]
    [InlineData(ComputerType.Spectrum16K, "16k")]
    [InlineData(ComputerType.Spectrum48K, "48k")]
    [InlineData(ComputerType.Spectrum128K, "128k")]
    [InlineData(ComputerType.Timex2048, "TC2048")]
    public void ComputerNameShouldBeSet(ComputerType computerType, string expectedComputerName)
    {
        _viewModel.ComputerType = computerType;
        _viewModel.ComputerName.ShouldBe(expectedComputerName);
    }

    [Theory]
    [InlineData(JoystickType.None, "None")]
    [InlineData(JoystickType.Kempston, "Kempston")]
    [InlineData(JoystickType.Sinclair1, "Sinclair 1")]
    [InlineData(JoystickType.Sinclair2, "Sinclair 2")]
    [InlineData(JoystickType.Cursor, "Cursor")]
    [InlineData(JoystickType.Fuller, "Fuller")]
    public void JoystickNameShouldBeSet(JoystickType joystickType, string expectedJoystickName)
    {
        _viewModel.JoystickType = joystickType;
        _viewModel.JoystickName.ShouldBe(expectedJoystickName);
    }

    [Theory]
    [InlineData(StereoMode.Mono, "AY Mono")]
    [InlineData(StereoMode.StereoABC, "AY Stereo ABC")]
    [InlineData(StereoMode.StereoACB, "AY Stereo ACB")]
    public void StereoModeShouldBeSet(StereoMode stereoMode, string expectedStereoModeName)
    {
        _viewModel.StereoMode = stereoMode;
        _viewModel.ToolTipAy.ShouldBe(expectedStereoModeName);
    }
}