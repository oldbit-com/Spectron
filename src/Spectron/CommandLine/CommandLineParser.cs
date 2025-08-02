using System;
using System.CommandLine;
using System.IO;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.CommandLine;

public static class CommandLineParser
{
    public static int Parse(Action<CommandLineArgs?> onParsed, string[] args)
    {
        if (args.Length == 0)
        {
            onParsed(null);

            return 0;
        }

        var fileOption = GetFileOption();
        var computerOption = GetComputerOption();
        var romOption = GetRomOption();
        var muteAudioOption = GetMuteAudioOption();
        var themeOption = GetThemeOption();
        var tapeLoadSpeedOption = GetTapeLoadSpeedOption();
        var ayEnabledOption = GetAyEnabledOption();
        var ayDisabledOption = GetAyDisabledOption();
        var borderSizeOption = GetBorderSizeOption();
        var joystickOption = GetJoystickTypeOption();
        var mouseOption = GetMouseTypeOption();
        var divMmcEnabledOption = GetDivMmcEnabledOption();
        var divMmcDisabledOption = GetDivMmcDisabledOption();
        var divMmcImageOption = GetDivMmcImageOption();
        var divMmcReadOnly = GetDivMmcReadOnlyOption();
        var zxPrinterEnabledOption = GetZxPrinterEnabledOption();
        var zxPrinterDisabledOption = GetZxPrinterDisabledOption();
        var ulaPlusEnabledOption = GetUlaPlusEnabledOption();
        var ulaPlusDisabledOption = GetUlaPlusDisabledOption();

        var rootCommand = new RootCommand("Starts Spectron Emulator with options");

        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(tapeLoadSpeedOption);
        rootCommand.Options.Add(computerOption);
        rootCommand.Options.Add(romOption);
        rootCommand.Options.Add(joystickOption);
        rootCommand.Options.Add(mouseOption);
        rootCommand.Options.Add(themeOption);
        rootCommand.Options.Add(borderSizeOption);
        rootCommand.Options.Add(muteAudioOption);
        rootCommand.Options.Add(ayEnabledOption);
        rootCommand.Options.Add(ayDisabledOption);
        rootCommand.Options.Add(zxPrinterEnabledOption);
        rootCommand.Options.Add(zxPrinterDisabledOption);
        rootCommand.Options.Add(ulaPlusEnabledOption);
        rootCommand.Options.Add(ulaPlusDisabledOption);
        rootCommand.Options.Add(divMmcEnabledOption);
        rootCommand.Options.Add(divMmcDisabledOption);
        rootCommand.Options.Add(divMmcImageOption);
        rootCommand.Options.Add(divMmcReadOnly);


        rootCommand.Validators.Add(result =>
        {
            var isDivMmcEnabled = IsEnabled(result.GetValue(divMmcEnabledOption), result.GetValue(divMmcDisabledOption));

            if (isDivMmcEnabled == true && result.GetValue(divMmcImageOption)?.FullName == null)
            {
                result.AddError($"When divMMC is enabled, you must specify an image path using {divMmcImageOption.Name}");
            }
        });

        fileOption.Validators.Add(result =>
        {
            var file = result.GetValue(fileOption);

            if (file?.FullName != null && !File.Exists(file.FullName))
            {
                result.AddError($"File does not exist: '{file.FullName}'");
            }
        });

        divMmcImageOption.Validators.Add(result =>
        {
            var file = result.GetValue(divMmcImageOption);

            if (file?.FullName != null && !File.Exists(file.FullName))
            {
                result.AddError($"divMMC image file does not exist: '{file.FullName}'");
            }
        });

        rootCommand.SetAction(parseResult =>
        {
            onParsed(new CommandLineArgs(
                parseResult.GetValue(fileOption)?.FullName,
                parseResult.GetValue(tapeLoadSpeedOption),
                parseResult.GetValue(computerOption),
                parseResult.GetValue(romOption),
                parseResult.GetValue(joystickOption),
                parseResult.GetValue(mouseOption),
                parseResult.GetValue(muteAudioOption),
                IsEnabled(parseResult.GetValue(ayEnabledOption), parseResult.GetValue(ayDisabledOption)),
                IsEnabled(parseResult.GetValue(divMmcEnabledOption), parseResult.GetValue(divMmcDisabledOption)),
                parseResult.GetValue(divMmcImageOption)?.FullName,
                parseResult.GetValue(divMmcReadOnly),
                IsEnabled(parseResult.GetValue(zxPrinterEnabledOption), parseResult.GetValue(zxPrinterDisabledOption)),
                IsEnabled(parseResult.GetValue(ulaPlusEnabledOption), parseResult.GetValue(ulaPlusDisabledOption)),
                parseResult.GetValue(borderSizeOption),
                parseResult.GetValue(themeOption)));
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static bool? IsEnabled(bool? isEnabledValue, bool? isDisabledValue) =>
        (isEnabledValue, isDisabledValue) switch
        {
            (true, _) => true,
            (_, true) => false,
            _ => null,
        };

    private static Option<FileInfo> GetFileOption() =>
        new("--file", "-f")
        {
            Description = "Specifies the file to load. This can be any supported file type: TAP | TZX | Z80 | SNA | SZX | POK | ZIP"
        };

    private static Option<ComputerType?> GetComputerOption() =>
        new Option<ComputerType?>("--computer", "-c")
            {
                Description = "Specifies the computer to emulate",
            }
            .AcceptOnlyFromAmong("Spectrum16K", "Spectrum48K", "Spectrum128K");

    private static Option<RomType?> GetRomOption() =>
        new Option<RomType?>("--rom", "-r")
            {
                Description = "Specifies the ROM to load",
            }
            .AcceptOnlyFromAmong("Original", "Retroleum", "GoshWonderful", "BusySoft", "Harston", "BrendanAlford", "Custom");

    private static Option<bool> GetMuteAudioOption() =>
        new("--mute", "-m")
        {
            Description = "Mutes the audio playback",
            DefaultValueFactory = _ => false
        };

    private static Option<Theme?> GetThemeOption() =>
        new("--theme", "-t")
        {
            Description = "Specifies the application theme",
        };

    private static Option<TapeSpeed?> GetTapeLoadSpeedOption() =>
        new("--tape-load-speed", "-ts")
        {
            Description = "Specifies the tape loading speed",
        };

    private static Option<bool?> GetAyEnabledOption() =>
        new("--ay")
        {
            Description = "Enables AY sound emulation",
        };

    private static Option<bool?> GetAyDisabledOption() =>
        new("--no-ay")
        {
            Description = "Disables AY sound emulation",
        };

    private static Option<BorderSize?> GetBorderSizeOption() =>
        new("--border", "-b")
        {
            Description = "Specifies the border size",
        };

    private static Option<JoystickType?> GetJoystickTypeOption() =>
        new("--joystick", "-j")
        {
            Description = "Specifies the emulated joystick type",
        };

    private static Option<MouseType?> GetMouseTypeOption() =>
        new("--mouse", "-m")
        {
            Description = "Specifies the emulated mouse type",
        };

    private static Option<bool?> GetDivMmcEnabledOption() =>
        new("--divmmc")
        {
            Description = "Enables divMMC emulation, requires --divmmc-image to be specified",
        };

    private static Option<bool?> GetDivMmcDisabledOption() =>
        new("--no-divmmc")
        {
            Description = "Disables divMMC emulation",
        };

    private static Option<FileInfo> GetDivMmcImageOption() =>
        new("--divmmc-image")
        {
            Description = "Specifies the SD card image to use with divMMC",
        };

    private static Option<bool?> GetDivMmcReadOnlyOption() =>
        new("--divmmc-readonly")
        {
            Description = "Specifies the SD card image is readonly",
        };

    private static Option<bool?> GetZxPrinterEnabledOption() =>
        new("--zx-printer")
        {
            Description = "Enables ZX Printer emulation",
        };

    private static Option<bool?> GetZxPrinterDisabledOption() =>
        new("--no-zx-printer")
        {
            Description = "Disables ZX Printer emulation",
        };

    private static Option<bool?> GetUlaPlusEnabledOption() =>
        new("--ula-plus")
        {
            Description = "Enables ULA+ emulation",
        };

    private static Option<bool?> GetUlaPlusDisabledOption() =>
        new("--no-ula-plus")
        {
            Description = "Disables ULA+ emulation",
        };
}