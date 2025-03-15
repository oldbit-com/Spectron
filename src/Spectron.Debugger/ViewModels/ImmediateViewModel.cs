using System.Reactive;
using Avalonia.Input;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Emulation;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class ImmediateViewModel : ReactiveObject, IOutput
{
    private readonly DebuggerContext _context;
    private readonly Emulator _emulator;
    private readonly Action _refreshAction;
    private readonly Action<Word> _listAction;
    private readonly NumberFormat _numberFormat = NumberFormat.HexDollarPrefix;
    private int _historyIndex = -1;
    private string _currentCommandText = string.Empty;

    public Action ScrollToEnd { get; set; } = () => { };

    public ReactiveCommand<KeyEventArgs, Unit> ImmediateCommand { get; private set; }

    public ImmediateViewModel(
        DebuggerContext context,
        Emulator emulator,
        Action refreshAction,
        Action<Word> listAction)
    {
        _context = context;
        _emulator = emulator;
        _refreshAction = refreshAction;
        _listAction = listAction;

        ImmediateCommand = ReactiveCommand.Create<KeyEventArgs>(HandleImmediateCommand);
    }

    private void HandleImmediateCommand(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (string.IsNullOrWhiteSpace(CommandText))
                {
                    break;
                }

                ExecuteCommand();

                if (_context.CommandHistory.Count == 0 ||
                    _context.CommandHistory.Count > 0 && _context.CommandHistory.Last() != CommandText)
                {
                    _context.CommandHistory.Add(CommandText);
                }

                CommandText = "";
                _historyIndex = _context.CommandHistory.Count;

                break;

            case Key.Up when _historyIndex > 0:
                if (_historyIndex == _context.CommandHistory.Count)
                {
                    _currentCommandText = CommandText;
                }

                _historyIndex -= 1;

                if (_historyIndex >= 0)
                {
                    CommandText = _context.CommandHistory[_historyIndex];
                }

                break;

            case Key.Down when _historyIndex < _context.CommandHistory.Count:
                _historyIndex += 1;

                CommandText = _historyIndex < _context.CommandHistory.Count ?
                    _context.CommandHistory[_historyIndex] :
                    _currentCommandText;

                break;
        }
    }

    public void Print(string output)
    {
        OutputText += output + Environment.NewLine;
        ScrollToEnd();
    }

    public void Clear() => OutputText = string.Empty;

    private void ExecuteCommand()
    {
        var interpreter = new Interpreter(_emulator.Cpu, _emulator.Memory, _emulator.Bus, this, _numberFormat);

        try
        {
            var result = interpreter.Execute(CommandText);

            switch (result)
            {
                case Success:
                    Print("OK");
                    break;

                case Integer intValue:
                    Print(NumberFormatter.Format(intValue.Value, intValue.Type, _numberFormat));
                    break;

                case GotoAction gotoAction:
                    Print($"Next instruction address set to ${gotoAction.Address:X4}");
                    break;

                case ListAction listAction:
                    _listAction(listAction.Address);
                    Print($"Listing at ${listAction.Address:X4}");

                    return;
            }
        }
        catch (SyntaxErrorException)
        {
            Print($"Syntax Error: `{CommandText}`");
        }
        catch (Exception ex)
        {
            Print(ex.Message);
        }

        _refreshAction();
    }

    private string _commandText = string.Empty;
    public string CommandText
    {
        get => _commandText;
        set => this.RaiseAndSetIfChanged(ref _commandText, value);
    }

    private string _outputText = string.Empty;
    public string OutputText
    {
        get => _outputText;
        set => this.RaiseAndSetIfChanged(ref _outputText, value);
    }
}