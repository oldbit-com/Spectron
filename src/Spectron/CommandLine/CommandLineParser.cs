using System;
using System.CommandLine;
using System.IO;
using OldBit.Spectron.Emulation;
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

        var rootCommand = new RootCommand("Starts Spectron Emulator with options");

        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(computerOption);
        rootCommand.Options.Add(romOption);
        rootCommand.Options.Add(muteAudioOption);
        rootCommand.Options.Add(themeOption);
        rootCommand.Options.Add(tapeLoadSpeedOption);
        rootCommand.Options.Add(ayEnabledOption);
        rootCommand.Options.Add(ayDisabledOption);
        rootCommand.Options.Add(borderSizeOption);

        rootCommand.SetAction(parseResult =>
        {
            bool? isAyEnabled = (parseResult.GetValue(ayEnabledOption), parseResult.GetValue(ayDisabledOption)) switch
            {
                (true, _) => true,
                (_, true) => false,
                _ => null,
            };

            onParsed(new CommandLineArgs(
                parseResult.GetValue(fileOption)?.FullName,
                parseResult.GetValue(tapeLoadSpeedOption),
                parseResult.GetValue(computerOption),
                parseResult.GetValue(romOption),
                parseResult.GetValue(muteAudioOption),
                isAyEnabled,
                parseResult.GetValue(borderSizeOption),
                parseResult.GetValue(themeOption)));
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static Option<FileInfo> GetFileOption() =>
        new("--file", "-f")
        {
            Description = "The file to load. This can be any supported file type: TAP | TZX | Z80 | SNA | SZX | ZIP"
        };

    private static Option<ComputerType?> GetComputerOption() =>
        new Option<ComputerType?>("--machine", "-m")
            {
                Description = "The machine to emulate",
            }
            .AcceptOnlyFromAmong("Spectrum16K", "Spectrum48K", "Spectrum128K");

    private static Option<RomType?> GetRomOption() =>
        new Option<RomType?>("--rom", "-r")
            {
                Description = "The ROM to load",
            }
            .AcceptOnlyFromAmong("Original", "Retroleum", "GoshWonderful", "BusySoft", "Harston", "BrendanAlford", "Custom");

    private static Option<bool> GetMuteAudioOption() =>
        new("--mute", "-m")
        {
            Description = "Mute audio playback",
            DefaultValueFactory = _ => false
        };

    private static Option<Theme?> GetThemeOption() =>
        new("--theme", "-t")
        {
            Description = "The theme to use",
        };

    private static Option<TapeSpeed?> GetTapeLoadSpeedOption() =>
        new("--tape-load-speed", "-ts")
        {
            Description = "The tape loading speed",
        };

    private static Option<bool?> GetAyEnabledOption() =>
        new("--ay")
        {
            Description = "AY sound emulation is enabled",
        };

    private static Option<bool?> GetAyDisabledOption() =>
        new("--no-ay")
        {
            Description = "AY sound emulation is disabled",
        };

    private static Option<BorderSize?> GetBorderSizeOption() =>
        new("--border", "-b")
        {
            Description = "The border size",
        };
}