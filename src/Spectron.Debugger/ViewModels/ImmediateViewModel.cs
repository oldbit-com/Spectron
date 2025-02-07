using System.Reactive;
using Avalonia.Input;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class ImmediateViewModel : ReactiveObject
{
    private readonly DebuggerContext _context;
    private int _historyIndex = -1;
    private string _currentCommandText = string.Empty;

    public ReactiveCommand<KeyEventArgs, Unit> ImmediateCommand { get; private set; }

    public ImmediateViewModel(DebuggerContext context)
    {
        _context = context;

        ImmediateCommand = ReactiveCommand.Create<KeyEventArgs>(HandleImmediateCommand);
    }

    private void HandleImmediateCommand(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                OutputText += CommandText + Environment.NewLine;

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