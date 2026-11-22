using System.ComponentModel.DataAnnotations;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Controls.Hex;
using OldBit.Spectron.Debugger.Converters;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public sealed partial class MemoryViewModel : ObservableValidator, IDisposable
{
    private const int MaxHistorySize = 100;

    private interface ICommand;
    private record WriteCommand (Word Address, byte Value) : ICommand;
    private record GoToCommand (Word Address) : ICommand;
    private record FindCommand (byte[] Pattern) : ICommand;
    private record InvalidCommand (string Error) : ICommand;

    private int _historyIndex = -1;
    private readonly List<string> _commandHistory = [];

    private readonly Emulator _emulator;

    private byte[] _lastFindPattern = [];
    private int _nextFindIndex;

    public HexViewer? Viewer { get; set; }
    public IClipboard? Clipboard { get; set; }

    public Action<Word, byte> OnMemoryUpdated { get; set; } = (_, _) => { };
    public Action<Word> GoTo { get; set; } = _ => { };
    public Action<Word, int> Select { get; set; } = (_, _) => { };

    [ObservableProperty]
    public partial byte[] Memory { get; set; }

    // Validated only when the command is submitted, so no error is shown while it is being typed
    [ObservableProperty]
    [CustomValidation(typeof(MemoryViewModel), nameof(ValidateCommand))]
    public partial string CommandText { get; set; } = string.Empty;

    public MemoryViewModel(Emulator emulator)
    {
        _emulator = emulator;

        Memory = emulator.Memory.ToBytes();
        emulator.Memory.MemoryUpdated += MemoryUpdated;
    }

    public void Update(IMemory memory) => Memory = memory.ToBytes();

    partial void OnCommandTextChanged(string value) => ClearErrors(nameof(CommandText));

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
        switch (e.Key)
        {
            case Key.Enter:
                ExecuteCommand();
                e.Handled = true;

                break;

            case Key.Up when _commandHistory.Count > 0:
                _historyIndex -= 1;

                if (_historyIndex < 0)
                {
                    _historyIndex = _commandHistory.Count - 1;
                }

                CommandText = _commandHistory[_historyIndex];
                e.Handled = true;

                break;

            case Key.Down when _commandHistory.Count > 0:
                _historyIndex += 1;

                if (_historyIndex >= _commandHistory.Count)
                {
                    _historyIndex = 0;
                }

                CommandText = _commandHistory[_historyIndex];
                e.Handled = true;

                break;
        }
    }

    private void ExecuteCommand()
    {
        var command = ParseCommand(CommandText);

        switch (command)
        {
            case InvalidCommand:
                ValidateProperty(CommandText, nameof(CommandText));

                break;

            case GoToCommand goToCommand:
                GoTo(goToCommand.Address);
                AddToHistory();

                break;

            case WriteCommand memoryCommand:
                _emulator.Memory.Write(memoryCommand.Address, memoryCommand.Value);
                AddToHistory();

                break;

            case FindCommand findCommand:
                Find(findCommand.Pattern);

                break;
        }
    }

    private void Find(byte[] pattern)
    {
        if (!pattern.AsSpan().SequenceEqual(_lastFindPattern))
        {
            _nextFindIndex = 0;
        }

        _lastFindPattern = pattern;

        var index = Memory.IndexOfSequence(pattern, _nextFindIndex);

        if (index < 0 && _nextFindIndex > 0)
        {
            index = Memory.IndexOfSequence(pattern, 0);
        }

        if (index < 0)
        {
            _nextFindIndex = 0;
            return;
        }

        Select((Word)index, pattern.Length);

        _nextFindIndex = index + pattern.Length;

        if (_nextFindIndex >= Memory.Length)
        {
            _nextFindIndex = 0;
        }

        // Keep the find command, so pressing Enter again finds the next occurrence
        AddToHistory(clearCommand: false);
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

        return ValidationResult.Success;
    }

    private static ICommand? ParseCommand(string command)
    {
        command = command.Trim();

        if (command.Length == 0)
        {
            return null;
        }

        var separatorIndex = command.IndexOfAny([' ', '\t']);

        var keyword = separatorIndex < 0 ? command : command[..separatorIndex];
        var args = separatorIndex < 0 ? string.Empty : command[(separatorIndex + 1)..].Trim();

        return keyword.ToLowerInvariant() switch
        {
            "g" => ParseGoToCommand(args),
            "w" => ParseWriteCommand(args),
            "f" => ParseFindCommand(args),
            _ => new InvalidCommand("Invalid command")
        };
    }

    private static ICommand ParseGoToCommand(string args) =>
        Hex.TryParse<Word>(args, out var address) ?
            new GoToCommand(address) :
            new InvalidCommand("Invalid address");

    private static ICommand ParseWriteCommand(string args)
    {
        var values = args.Split(',');

        if (values.Length != 2)
        {
            return new InvalidCommand("Invalid arguments");
        }

        if (!Hex.TryParse<Word>(values[0], out var address))
        {
            return new InvalidCommand("Invalid address");
        }

        if (!Hex.TryParse<byte>(values[1], out var value))
        {
            return new InvalidCommand("Invalid value");
        }

        return new WriteCommand(address, value);
    }

    private static ICommand ParseFindCommand(string args)
    {
        if (args.Length == 0)
        {
            return new InvalidCommand("Empty search text");
        }

        if (args[0] == '"')
        {
            if (args.Length < 2 || args[^1] != '"')
            {
                return new InvalidCommand("Missing closing quote");
            }

            return ParseFindText(args[1..^1]);
        }

        var hexes = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var pattern = new byte[hexes.Length];

        for (var i = 0; i < hexes.Length; i++)
        {
            if (!Hex.TryParse(hexes[i], out pattern[i]))
            {
                return ParseFindText(args);
            }
        }

        return new FindCommand(pattern);
    }

    private static ICommand ParseFindText(string text)
    {
        if (text.Length == 0)
        {
            return new InvalidCommand("Empty search text");
        }

        if (!ZxAscii.TryFromString(text, out var pattern))
        {
            return new InvalidCommand("Unsupported character");
        }

        return new FindCommand(pattern);
    }

    private void AddToHistory(bool clearCommand = true)
    {
        var command = CommandText.Trim();

        if (_commandHistory.LastOrDefault() != command)
        {
            if (_commandHistory.Count >= MaxHistorySize)
            {
                _commandHistory.RemoveAt(0);
            }

            _commandHistory.Add(command);
        }

        _historyIndex = _commandHistory.Count;

        if (clearCommand)
        {
            CommandText = string.Empty;
        }
    }
}
