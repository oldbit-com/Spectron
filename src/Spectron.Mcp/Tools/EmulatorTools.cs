using System.ComponentModel;
using System.Globalization;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Disassembly;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Mcp.Input;
using OldBit.Spectron.Mcp.Screen;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Mcp.Tools;

[McpServerToolType]
public sealed class EmulatorTools(EmulatorController controller)
{
    private const int MaxMemoryLength = 4096;
    private const int MaxInstructions = 500;

    [McpServerTool(Name = "get_status", ReadOnly = true)]
    [Description("Returns the emulated machine type, ROM, whether emulation is paused, the program counter and tape state.")]
    public Task<string> GetStatus() => controller.ExecuteAsync(emulator =>
        $"""
         Computer: {emulator.ComputerType}
         ROM: {emulator.RomType}
         Paused: {emulator.IsPaused}
         PC: ${emulator.Cpu.Registers.PC:X4}
         Tape loaded: {emulator.TapeManager.IsTapeLoaded}, playing: {emulator.TapeManager.IsPlaying}
         """);

    [McpServerTool(Name = "pause")]
    [Description("Pauses emulation. While paused, use run_frames, type_text or press_keys to advance time deterministically.")]
    public async Task<string> Pause()
    {
        await controller.RunHostActionAsync(host => host.PauseAsync());

        return "Paused.";
    }

    [McpServerTool(Name = "resume")]
    [Description("Resumes real-time emulation.")]
    public async Task<string> Resume()
    {
        await controller.RunHostActionAsync(host => host.ResumeAsync());

        return "Resumed.";
    }

    [McpServerTool(Name = "reset", Destructive = true)]
    [Description("Resets the machine. A hard reset recreates the machine from the current preferences (machine type, ROM, peripherals).")]
    public async Task<string> Reset(
        [Description("True for a hard reset, false for a soft reset of the current machine.")] bool hard = false)
    {
        await controller.RunHostActionAsync(host => host.ResetAsync(hard));

        return hard ? "Hard reset done." : "Reset done.";
    }

    [McpServerTool(Name = "load_file", Destructive = true)]
    [Description("Loads a file into the emulator, same as File/Open: snapshots (.sna, .z80, .szx, .spectron), tapes (.tap, .tzx, auto-typing LOAD \"\"), " +
                 "disks (.trd, .scl), microdrives (.mdr), RZX recordings and POKE files. Zip archives are supported. Snapshots replace the current machine.")]
    public async Task<string> LoadFile(
        [Description("Absolute path to the file.")] string path)
    {
        if (!File.Exists(path))
        {
            throw new McpException($"File not found: {path}");
        }

        if (FileTypeResolver.FromPath(path) == FileType.Unsupported)
        {
            throw new McpException($"Unsupported file type: {Path.GetExtension(path)}");
        }

        await controller.RunHostActionAsync(host => host.LoadFileAsync(path));

        return $"Loaded {Path.GetFileName(path)}.";
    }

    [McpServerTool(Name = "run_frames")]
    [Description("Pauses real-time emulation (if running) and runs the given number of frames as fast as possible (50 frames = 1 second of Spectrum time). " +
                 "The emulator stays paused afterwards, so the result can be inspected. Stops early if a debugger breakpoint is hit.")]
    public Task<string> RunFrames(
        [Description("Number of frames to run.")] int count)
    {
        if (count is < 1 or > EmulatorController.MaxFramesPerCall)
        {
            throw new McpException($"Frame count must be between 1 and {EmulatorController.MaxFramesPerCall}.");
        }

        return controller.ExecuteAsync(emulator =>
        {
            var frames = EmulatorController.RunFrames(emulator, count);

            return FormatRunResult(emulator, frames);
        }, leavePaused: true);
    }

    [McpServerTool(Name = "type_text")]
    [Description("""
                 Types text on the Spectrum keyboard, one key press per character, by running emulation frames while keys are held.
                 Supports letters (uppercase uses CAPS SHIFT), digits, space, newline (ENTER), backspace (DELETE) and symbols from
                 the 48K keyboard: ! @ # $ % & ' ( ) _ < > ; " ^ - + = : £ ? / * , .
                 IMPORTANT: 48K BASIC uses keyword entry. At the K cursor a letter key produces a whole keyword (e.g. 'p' = PRINT,
                 'j' = LOAD, 'r' = RUN, 'l' = LET), so type the single keyword letter, not the word. 128K BASIC accepts keywords spelled out.
                 Emulation stays paused afterwards if it was paused before.
                 """)]
    public Task<string> TypeText(
        [Description("Text to type. Use \\n for ENTER.")] string text,
        [Description("Frames to hold each key.")] int holdFrames = 3,
        [Description("Frames to wait after releasing each key. The ROM needs about 5 frames before it accepts the same key again.")] int releaseFrames = 6)
    {
        List<SpectrumKey[]> keys;

        try
        {
            keys = KeyMapper.FromText(text);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }

        ValidateKeyTiming(holdFrames, releaseFrames, keys.Count);

        return controller.ExecuteAsync(emulator =>
        {
            var frames = EmulatorController.PressKeys(emulator, keys, holdFrames, releaseFrames);

            return FormatRunResult(emulator, frames);
        });
    }

    [McpServerTool(Name = "press_keys")]
    [Description("""
                 Presses key combinations one after another. Each combination is held, then released.
                 Key names: A-Z, 0-9, ENTER, SPACE, CAPS, SYM, and shortcuts BREAK, DELETE, EDIT, CAPSLOCK, TRUEVIDEO, INVVIDEO,
                 LEFT, DOWN, UP, RIGHT, GRAPHICS, EXTEND. Combine keys with '+', e.g. "SYM+P", "CAPS+5".
                 Use a long holdFrames to keep a key down in games (e.g. holdFrames 50 = 1 second).
                 """)]
    public Task<string> PressKeys(
        [Description("Key combinations to press in order, e.g. [\"J\", \"SYM+P\", \"SYM+P\", \"ENTER\"].")] string[] keys,
        [Description("Frames to hold each combination.")] int holdFrames = 3,
        [Description("Frames to wait after releasing each combination. The ROM needs about 5 frames before it accepts the same key again.")] int releaseFrames = 6)
    {
        List<SpectrumKey[]> combinations;

        try
        {
            combinations = keys.Select(KeyMapper.ParseCombination).ToList();
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }

        ValidateKeyTiming(holdFrames, releaseFrames, combinations.Count);

        return controller.ExecuteAsync(emulator =>
        {
            var frames = EmulatorController.PressKeys(emulator, combinations, holdFrames, releaseFrames);

            return FormatRunResult(emulator, frames);
        });
    }

    [McpServerTool(Name = "read_screen_text", ReadOnly = true)]
    [Description("Reads the 32x24 character screen as text by matching each 8x8 cell against the character set. " +
                 "Unrecognised graphics are shown as '▒'. Prefer this over screenshots when looking for text.")]
    public Task<string> ReadScreenText(
        [Description("Optional address of a custom 96-character font in memory (the first character is space). Uses the ROM font when omitted.")]
        string? fontAddress = null)
    {
        var font = fontAddress == null ? (Word?)null : ParseAddress(fontAddress);

        return controller.ExecuteAsync(emulator =>
        {
            var reader = ScreenTextReader.Create(emulator.Memory, font);

            return string.Join('\n', reader.ReadLines(emulator.Memory));
        });
    }

    [McpServerTool(Name = "screenshot", ReadOnly = true)]
    [Description("Captures the current screen as a PNG image.")]
    public Task<CallToolResult> Screenshot(
        [Description("Integer scale factor, 1-4.")] int scale = 2,
        [Description("Include the border area.")] bool includeBorder = false)
    {
        if (scale is < 1 or > 4)
        {
            throw new McpException("Scale must be between 1 and 4.");
        }

        return controller.ExecuteAsync(emulator =>
        {
            var frameBuffer = emulator.ScreenBuffer.FrameBuffer;
            var png = includeBorder
                ? PngEncoder.Encode(frameBuffer, 0, 0, frameBuffer.Width, frameBuffer.Height, scale)
                : EncodeContent(frameBuffer, scale);

            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(png, "image/png")]
            };
        });
    }

    [McpServerTool(Name = "get_registers", ReadOnly = true)]
    [Description("Returns Z80 registers, flags and interrupt state.")]
    public Task<string> GetRegisters() => controller.ExecuteAsync(emulator => FormatRegisters(emulator.Cpu));

    [McpServerTool(Name = "read_memory", ReadOnly = true)]
    [Description("Reads memory as a hex dump with ASCII. Addresses accept decimal, $8000, 0x8000, #8000 or 8000h.")]
    public Task<string> ReadMemory(
        [Description("Start address.")] string address,
        [Description("Number of bytes, 1-4096.")] int length = 256)
    {
        var start = ParseAddress(address);

        if (length is < 1 or > MaxMemoryLength)
        {
            throw new McpException($"Length must be between 1 and {MaxMemoryLength}.");
        }

        return controller.ExecuteAsync(emulator => FormatHexDump(emulator.Memory, start, length));
    }

    [McpServerTool(Name = "write_memory", Destructive = true)]
    [Description("Writes bytes to memory (like POKE). Writes to ROM are ignored by the hardware.")]
    public Task<string> WriteMemory(
        [Description("Start address.")] string address,
        [Description("Bytes as hex, e.g. \"3E 01 C9\" or \"3E01C9\".")] string data)
    {
        var start = ParseAddress(address);
        var bytes = ParseHexBytes(data);

        return controller.ExecuteAsync(emulator =>
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                emulator.Memory.Write((Word)(start + i), bytes[i]);
            }

            return $"Wrote {bytes.Length} byte(s) at ${start:X4}.";
        });
    }

    [McpServerTool(Name = "disassemble", ReadOnly = true)]
    [Description("Disassembles Z80 code. Defaults to the current program counter.")]
    public Task<string> Disassemble(
        [Description("Start address, defaults to PC.")] string? address = null,
        [Description("Number of instructions, 1-500.")] int count = 20)
    {
        var start = address == null ? (Word?)null : ParseAddress(address);

        if (count is < 1 or > MaxInstructions)
        {
            throw new McpException($"Count must be between 1 and {MaxInstructions}.");
        }

        return controller.ExecuteAsync(emulator => FormatDisassembly(emulator, start ?? emulator.Cpu.Registers.PC, count));
    }

    [McpServerTool(Name = "debugger_command")]
    [Description("""
                 Executes a command of the built-in debugger, separate multiple commands with ';'. Commands:
                 PRINT expr[, expr] (alias ?), POKE addr, value, PEEK addr, OUT port, value, IN port, reg = expr (e.g. HL = $4000),
                 GOTO addr (sets PC), LIST [addr]. Numbers: decimal, $FF, 0xFF, #FF, FFh, 0b101, 101b. Registers can be used in expressions.
                 PEEK and IN are statements, not expressions: use "PEEK 23560", not "PRINT PEEK 23560".
                 """)]
    public Task<string> DebuggerCommand(
        [Description("The command, e.g. \"PRINT HL, DE\" or \"PEEK 23560\".")] string command) =>
        controller.ExecuteAsync(emulator => ExecuteDebuggerCommand(emulator, command));

    private static string ExecuteDebuggerCommand(Emulator emulator, string command)
    {
        var output = new DebuggerOutput();
        var interpreter = new Interpreter(emulator.Cpu, emulator.Memory, emulator.Bus, output);

        try
        {
            var result = interpreter.Execute(command);

            switch (result)
            {
                case Success:
                    output.Print("OK");
                    break;

                case Print print:
                    foreach (var value in print.Values)
                    {
                        output.Print(FormatValue(emulator.Cpu, value));
                    }
                    break;

                case GotoAction gotoAction:
                    output.Print($"PC set to ${gotoAction.Address:X4}");
                    break;

                case ListAction listAction:
                    output.Print(FormatDisassembly(emulator, listAction.Address, 20));
                    break;

                case SaveAction:
                    output.Print("SAVE is not supported here, use read_memory instead.");
                    break;

                case not null:
                    output.Print(FormatValue(emulator.Cpu, result));
                    break;
            }
        }
        catch (SyntaxErrorException)
        {
            throw new McpException($"Syntax error: `{command}`");
        }

        var text = output.ToString().TrimEnd();

        return text.Length > 0 ? text : "(no output)";
    }

    private static string FormatValue(Z80 cpu, Value? value) => value switch
    {
        Register register => FormatRegister(cpu, register),
        Integer integer => $"{integer.Value} (${integer.Value:X})",
        _ => value?.ToString() ?? string.Empty
    };

    private static string FormatRegister(Z80 cpu, Register register)
    {
        var value = cpu.GetRegisterValue(register.Name);

        return register.Is8Bit
            ? $"{register.Name}=${value:X2} ({value})"
            : $"{register.Name}=${value:X4} ({value})";
    }

    private static string FormatRunResult(Emulator emulator, int frames)
    {
        var result = $"Ran {frames} frame(s). PC=${emulator.Cpu.Registers.PC:X4}.";

        if (emulator.IsDebuggerBreak)
        {
            result += " Stopped at a debugger breakpoint.";
        }

        return result;
    }

    private static string FormatRegisters(Z80 cpu)
    {
        var r = cpu.Registers;
        var flags = (byte)r.F;

        return $"""
                AF=${r.AF:X4} BC=${r.BC:X4} DE=${r.DE:X4} HL=${r.HL:X4}
                AF'=${r.Prime.AF:X4} BC'=${r.Prime.BC:X4} DE'=${r.Prime.DE:X4} HL'=${r.Prime.HL:X4}
                IX=${r.IX:X4} IY=${r.IY:X4} SP=${r.SP:X4} PC=${r.PC:X4}
                I=${r.I:X2} R=${r.R:X2} IM={cpu.IM} IFF1={cpu.IFF1} IFF2={cpu.IFF2} Halted={cpu.IsHalted}
                Flags: {FormatFlags(flags)}
                """;
    }

    private static string FormatFlags(byte flags)
    {
        const string names = "SZ5H3PNC";
        var result = new StringBuilder(8);

        for (var i = 0; i < 8; i++)
        {
            result.Append((flags & (0x80 >> i)) != 0 ? names[i] : '-');
        }

        return result.ToString();
    }

    private static string FormatHexDump(IMemory memory, Word start, int length)
    {
        var result = new StringBuilder();

        for (var offset = 0; offset < length; offset += 16)
        {
            var lineAddress = (Word)(start + offset);
            var count = Math.Min(16, length - offset);

            result.Append($"${lineAddress:X4}: ");

            var ascii = new StringBuilder(16);

            for (var i = 0; i < 16; i++)
            {
                if (i < count)
                {
                    var value = memory.Read((Word)(lineAddress + i));
                    result.Append($"{value:X2} ");
                    ascii.Append(value is >= 0x20 and < 0x7F ? (char)value : '.');
                }
                else
                {
                    result.Append("   ");
                }
            }

            result.Append(' ').Append(ascii).Append('\n');
        }

        return result.ToString().TrimEnd();
    }

    private static string FormatDisassembly(Emulator emulator, Word start, int count)
    {
        var disassembler = new Disassembler(emulator.Memory, start, count);
        var result = new StringBuilder();

        foreach (var instruction in disassembler.Disassemble())
        {
            var bytes = string.Join(' ', instruction.ByteCodes.Select(b => b.ToString("X2")));
            result.Append($"${instruction.Address:X4}  {bytes,-12} {instruction.Code}\n");
        }

        return result.ToString().TrimEnd();
    }

    private static byte[] EncodeContent(FrameBuffer frameBuffer, int scale)
    {
        var hiResMultiplier = frameBuffer.IsHiRes ? 2 : 1;

        return PngEncoder.Encode(
            frameBuffer,
            ScreenSize.BorderLeft * hiResMultiplier,
            ScreenSize.BorderTop,
            ScreenSize.ContentWidth * hiResMultiplier,
            ScreenSize.ContentHeight,
            scale);
    }

    private static void ValidateKeyTiming(int holdFrames, int releaseFrames, int keyCount)
    {
        if (holdFrames < 1 || releaseFrames < 0)
        {
            throw new McpException("holdFrames must be at least 1 and releaseFrames cannot be negative.");
        }

        if ((long)(holdFrames + releaseFrames) * keyCount > EmulatorController.MaxFramesPerCall)
        {
            throw new McpException($"Too many frames requested, the limit is {EmulatorController.MaxFramesPerCall} per call.");
        }
    }

    internal static Word ParseAddress(string address)
    {
        var text = address.Trim();
        var style = NumberStyles.Integer;

        if (text.StartsWith('$') || text.StartsWith('#'))
        {
            text = text[1..];
            style = NumberStyles.HexNumber;
        }
        else if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            text = text[2..];
            style = NumberStyles.HexNumber;
        }
        else if (text.EndsWith('h') || text.EndsWith('H'))
        {
            text = text[..^1];
            style = NumberStyles.HexNumber;
        }

        if (!int.TryParse(text, style, CultureInfo.InvariantCulture, out var value) || value is < 0 or > 0xFFFF)
        {
            throw new McpException($"Invalid address: '{address}'. Use a value 0-65535, e.g. 32768, $8000 or 0x8000.");
        }

        return (Word)value;
    }

    internal static byte[] ParseHexBytes(string data)
    {
        var hex = new string(data.Where(c => !char.IsWhiteSpace(c) && c != ',').ToArray());

        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex[2..];
        }

        try
        {
            var bytes = Convert.FromHexString(hex);

            return bytes.Length > 0 ? bytes : throw new FormatException();
        }
        catch (FormatException)
        {
            throw new McpException($"Invalid hex data: '{data}'. Use pairs of hex digits, e.g. \"3E 01 C9\".");
        }
    }

    private sealed class DebuggerOutput : IOutput
    {
        private readonly StringBuilder _output = new();

        public void Print(string output) => _output.AppendLine(output);

        public void Clear() => _output.Clear();

        public override string ToString() => _output.ToString();
    }
}
