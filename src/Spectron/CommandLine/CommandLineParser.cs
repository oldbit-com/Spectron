using System;
using System.CommandLine;
using System.IO;

namespace OldBit.Spectron.CommandLine;

public static class CommandLineParser
{
    public static int Parse(Action<CommandLineArgs> onParsed, string[] args)
    {
        var fileOption = new Option<FileInfo>("--file", "-f")
        {
            Description = "The file to load. This can be any supported file type: TAP | TZX | Z80 | SNA | SZX | ZIP"
        };

        var machineOption = new Option<string>("--machine", "-m")
            {
                Description = "The machine to emulate",
            }
            .AcceptOnlyFromAmong("zx16", "zx48", "zx128");

        var rootCommand = new RootCommand("Starts Spectron Emulator");
        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(machineOption);

        rootCommand.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileOption);
            var machine = parseResult.GetValue(machineOption);

            //Console.WriteLine($"Starting emulator '{file?.Name}' Machine '{machine}'");

            onParsed(new CommandLineArgs(file?.Name, machine));
        });

        return rootCommand.Parse(args).Invoke();
    }
}