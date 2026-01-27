using System.ComponentModel.DataAnnotations;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Controls.Hex;
using OldBit.Spectron.Debugger.Converters;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public sealed partial class MemoryViewModel : ObservableValidator, IDisposable
{
    private interface ICommand;
    private record WriteCommand (Word Address, byte Value) : ICommand;
    private record GoToCommand (Word Address) : ICommand;
    private record FindCommand (string Text) : ICommand;
    private record InvalidCommand (string Error) : ICommand;

    private readonly Emulator _emulator;

    public HexViewer? Viewer { get; set; }
    public IClipboard? Clipboard { get; set; }

    public Action<Word, byte> OnMemoryUpdated { get; set; } = (_, _) => { };
    public Action<Word> GoTo { get; set; } = _ => { };
    public Action<Word, int> Select { get; set; } = (_, _) => { };

    private int _lastFindInex = 0;

    [ObservableProperty]
    private byte[] _memory = [];

    [ObservableProperty]
    [CustomValidation(typeof(MemoryViewModel), nameof(ValidateCommand))]
    [NotifyDataErrorInfo]
    private string _commandText = string.Empty;

    public MemoryViewModel(Emulator emulator)
    {
        _emulator = emulator;

        Memory = emulator.Memory.ToBytes();
        emulator.Memory.MemoryUpdated += MemoryUpdated;
    }

    public void Update(IMemory memory) => Memory = memory.ToBytes();

    [RelayCommand]
    private async Task CopyHex()
    {
        if (Clipboard is null)
        {
            return;
        }

        var bytes = GetSelectedBytes();
        var hex = BitConverter.ToString(bytes).Replace("-", " ");

        await Clipboard.SetTextAsync(hex);
    }

    [RelayCommand]
    private async Task CopyAscii()
    {
        if (Clipboard is null)
        {
            return;
        }

        var bytes = GetSelectedBytes();
        var ascii = ZxAscii.ToString(bytes);

        await Clipboard.SetTextAsync(ascii);
    }

    [RelayCommand]
    private void Immediate(KeyEventArgs e)
    {
        if (e.Key != Key.Enter || string.IsNullOrWhiteSpace(CommandText))
        {
            return;
        }

        var command = ParseCommand(CommandText);

        switch (command)
        {
            case GoToCommand goToCommand:
                GoTo(goToCommand.Address);
                break;

            case WriteCommand memoryCommand:
                _emulator.Memory.Write(memoryCommand.Address, memoryCommand.Value);
                break;

            case FindCommand findCommand:
            {
                var ascii = ZxAscii.FromString(findCommand.Text).AsSpan();
                var memory = Memory.AsSpan();

                var index = memory.IndexOfSequence(ascii, _lastFindInex);
                _lastFindInex = 0;

                if (index < 0)
                {
                    return;
                }

                Select((Word)index, ascii.Length);

                _lastFindInex = index + ascii.Length;

                if (_lastFindInex >= Memory.Length)
                {
                    _lastFindInex = 0;
                }
                break;
            }
        }
    }

    private byte[] GetSelectedBytes()
    {
        if (Viewer is null || Viewer.Selection.Length == 0)
        {
            return [];
        }

        var selectedBytes = new byte[Viewer.Selection.Length];
        var index = 0;

        for (var selected = Viewer.Selection.Start; selected <= Viewer.Selection.End; selected++)
        {
            selectedBytes[index++] = Memory[selected];
        }

        return selectedBytes;
    }

    private void MemoryUpdated(Word address, byte value) => OnMemoryUpdated.Invoke(address, value);

    public void Dispose() => _emulator.Memory.MemoryUpdated -= MemoryUpdated;

    public static ValidationResult? ValidateCommand(string s, ValidationContext context)
    {
        var command = ParseCommand(s);

        if (command is InvalidCommand invalidCommand)
        {
            return new ValidationResult(invalidCommand.Error);
        }

        return ValidationResult.Success;;
    }

    private static ICommand? ParseCommand(string command)
    {
        command = command.Trim();

        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        if (command.StartsWith("g", StringComparison.OrdinalIgnoreCase))
        {
            if (HexNumberParser.TryParse<Word>(command[1..].Trim(), out var address))
            {
                return new GoToCommand(address);
            }

            return new InvalidCommand("Invalid address");
        }

        if (command.StartsWith("f", StringComparison.OrdinalIgnoreCase))
        {
            var text = command[1..].Trim();

            return text.Length switch
            {
                0 => new InvalidCommand("Empty search text"),
                > 2 when text[0] == '"' && text[^1] == '"' => new FindCommand(text[1..^1]),
                _ => new FindCommand(text)
            };
        }

        if (command.StartsWith("w", StringComparison.OrdinalIgnoreCase))
        {
            var args = command[1..].Trim().Split(',');

            if (args.Length != 2)
            {
                return new InvalidCommand("Invalid arguments");
            }

            if (!HexNumberParser.TryParse<Word>(args[0].Trim(), out var address))
            {
                return new InvalidCommand("Invalid address");
            }

            if (!HexNumberParser.TryParse<byte>(args[1].Trim(), out var value))
            {
                return new InvalidCommand("Invalid value");
            }

            return new WriteCommand(address, value);
        }

        return new InvalidCommand("Invalid command");
    }
}