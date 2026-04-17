using OldBit.Spectron.CommandLine;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.Tests.CommandLine;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_WithNoArgs_ReturnsNull()
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, []);

        result.ShouldBe(0);
        args.ShouldBeNull();
    }

    [Theory]
    [InlineData("--divmmc", true)]
    [InlineData("--no-divmmc", false)]
    public void Parse_DivMmcEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/divmmc.image");
        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, "--divmmc-image", testFilePath]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with
        {
            IsDivMmcEnabled = expectedValue,
            DivMmcImageFile = testFilePath
        };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--divmmc-readonly", true)]
    [InlineData("--divmmc-writable", false)]
    public void Parse_DivMmcReadOnlyOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsDivMmcReadOnly = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-f")]
    [InlineData("--file")]
    public void Parse_FileOption_ShouldBeParsed(string option)
    {
        CommandLineArgs? args = null;

        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/Spectron.tap");
        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, testFilePath]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { FilePath = testFilePath };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--ay", true)]
    [InlineData("--no-ay", false)]
    public void Parse_AyEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsAyEnabled = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--ay-mode", "mono", StereoMode.Mono)]
    [InlineData("--ay-mode", "stereoABC", StereoMode.StereoABC)]
    [InlineData("--ay-mode", "StereoAcb", StereoMode.StereoACB)]
    public void Parse_AyStereoModeOption_ShouldBeParsed(string option, string value, StereoMode expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { AyStereoMode = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-b", "none", BorderSize.None)]
    [InlineData("-b", "medium", BorderSize.Medium)]
    [InlineData("--border", "Small", BorderSize.Small)]
    [InlineData("--border", "large", BorderSize.Large)]
    [InlineData("--border", "Full", BorderSize.Full)]
    public void Parse_BorderOption_ShouldBeParsed(string option, string value, BorderSize expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { BorderSize = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-c", "spectrum16k", ComputerType.Spectrum16K)]
    [InlineData("--computer", "Spectrum48k", ComputerType.Spectrum48K)]
    [InlineData("-c", "Spectrum128K", ComputerType.Spectrum128K)]
    [InlineData("--computer", "timex2048", ComputerType.Timex2048)]
    public void Parse_ComputerOption_ShouldBeParsed(string option, string value, ComputerType expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { ComputerType = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--interface1", true)]
    [InlineData("--no-interface1", false)]
    public void Parse_Interface1EnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsInterface1Enabled = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--interface1-rom", "v1", Interface1RomVersion.V1)]
    [InlineData("--interface1-rom", "V2", Interface1RomVersion.V2)]
    public void Parse_Interface1RomOption_ShouldBeParsed(string option, string value, Interface1RomVersion expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { Interface1RomVersion = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-j", "none", JoystickType.None)]
    [InlineData("-j", "Fuller", JoystickType.Fuller)]
    [InlineData("-j", "Sinclair1", JoystickType.Sinclair1)]
    [InlineData("--joystick", "Kempston", JoystickType.Kempston)]
    [InlineData("--joystick", "cursor", JoystickType.Cursor)]
    [InlineData("--joystick", "sinclair1", JoystickType.Sinclair1)]
    public void Parse_JoystickOption_ShouldBeParsed(string option, string value, JoystickType expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { JoystickType = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-m", "none", MouseType.None)]
    [InlineData("--mouse", "Kempston", MouseType.Kempston)]
    public void Parse_MouseOption_ShouldBeParsed(string option, string value, MouseType expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { MouseType = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--mute", true)]
    public void Parse_MuteOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsAudioMuted = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--resume", true)]
    [InlineData("--no-resume", false)]
    public void Parse_ResumeEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsResumeEnabled = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-r", "BbcBasic", RomType.BbcBasic)]
    [InlineData("-r", "BrendanAlford", RomType.BrendanAlford)]
    [InlineData("-r", "BusySoft", RomType.BusySoft)]
    [InlineData("--rom", "GoshWonderful", RomType.GoshWonderful)]
    [InlineData("--rom", "Harston", RomType.Harston)]
    [InlineData("--rom", "HtrSuperBasic", RomType.HtrSuperBasic)]
    [InlineData("--rom", "Original", RomType.Original)]
    [InlineData("--rom", "Pentagon128", RomType.Pentagon128)]
    [InlineData("-r", "PrettyBasic", RomType.PrettyBasic)]
    [InlineData("-r", "Retroleum", RomType.Retroleum)]
    public void Parse_RomOption_ShouldBeParsed(string option, string value, RomType expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { RomType = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-rf")]
    [InlineData("--rom-file")]
    public void Parse_CustomRomOption_ShouldBeParsed(string option)
    {
        CommandLineArgs? args = null;

        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/rom.bin");
        var result = CommandLineParser.Parse(parsed => { args = parsed; }, ["--rom", "Custom", option, testFilePath]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with
        {
            RomType = RomType.Custom,
            CustomRomFiles = [testFilePath]
        };

        args.ShouldBeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("-t", "normal", TapeSpeed.Normal)]
    [InlineData("-t", "Instant", TapeSpeed.Instant)]
    [InlineData("--tape-load-speed", "Instant", TapeSpeed.Instant)]
    [InlineData("--tape-load-speed", "AccelerateD", TapeSpeed.Accelerated)]
    public void Parse_TapeLoadSpeedOption_ShouldBeParsed(string option, string value, TapeSpeed expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { TapeLoadSpeed = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--theme", "DARK", Theme.Dark)]
    [InlineData("--theme", "Light", Theme.Light)]
    public void Parse_ThemeOption_ShouldBeParsed(string option, string value, Theme expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option, value]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { Theme = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--time-machine", true)]
    [InlineData("--no-time-machine", false)]
    public void Parse_TimeMachineEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsTimeMachineEnabled = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--ula-plus", true)]
    [InlineData("--no-ula-plus", false)]
    public void Parse_UlaPlusEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsUlaPlusEnabled = expectedValue };

        args.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--zx-printer", true)]
    [InlineData("--no-zx-printer", false)]
    public void Parse_ZxPrinterEnabledOption_ShouldBeParsed(string option, bool expectedValue)
    {
        CommandLineArgs? args = null;

        var result = CommandLineParser.Parse(parsed => { args = parsed; }, [option]);

        result.ShouldBe(0);
        args.ShouldNotBeNull();

        var expected = DefaultArgs with { IsZxPrinterEnabled = expectedValue };

        args.ShouldBe(expected);
    }

    private static readonly CommandLineArgs DefaultArgs = new
    (
        FilePath: null,
        AyStereoMode: null,
        BorderSize: null,
        ComputerType: null,
        CustomRomFiles: [],
        DivMmcImageFile: null,
        Interface1RomVersion: null,
        IsAudioMuted: false,
        IsAyEnabled: null,
        IsDivMmcEnabled: null,
        IsDivMmcReadOnly: null,
        IsInterface1Enabled: null,
        IsResumeEnabled: null,
        IsTimeMachineEnabled: null,
        IsUlaPlusEnabled: null,
        IsZxPrinterEnabled: null,
        JoystickType: null,
        MouseType: null,
        RomType: null,
        TapeLoadSpeed: null,
        Theme: null
    );
}
