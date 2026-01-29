using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class ImmediateViewModel(
    DebuggerContext context,
    NumberFormat numberFormat,
    Emulator emulator,
    Action refreshAction,
    Action<ListAction> action)
    : ObservableObject, IOutput
{
    private int _historyIndex = -1;
    private string _currentCommandText = string.Empty;

    public Action ScrollToEnd { get; set; } = () => { };

    [ObservableProperty]
    private string _commandText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [RelayCommand]
    private void Immediate(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (string.IsNullOrWhiteSpace(CommandText))
                {
                    break;
                }

                ExecuteCommand();

                if (context.CommandHistory.Count == 0 ||
                    context.CommandHistory.Count > 0 && context.CommandHistory.Last() != CommandText)
                {
                    context.CommandHistory.Add(CommandText);
                }

                CommandText = "";
                _historyIndex = context.CommandHistory.Count;

                break;

            case Key.Up when _historyIndex > 0:
                if (_historyIndex == context.CommandHistory.Count)
                {
                    _currentCommandText = CommandText;
                }

                _historyIndex -= 1;

                if (_historyIndex >= 0)
                {
                    CommandText = context.CommandHistory[_historyIndex];
                }

                break;

            case Key.Down when _historyIndex < context.CommandHistory.Count:
                _historyIndex += 1;

                CommandText = _historyIndex < context.CommandHistory.Count ?
                    context.CommandHistory[_historyIndex] :
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

    private void Print(Print printValue)
    {
        foreach (var value in printValue.Values)
        {
            string? hexValue;

            switch (value)
            {
                case Register register:
                {
                    var decimalValue = emulator.Cpu.GetRegisterValue(register.Name);

                    hexValue = register.Is8Bit
                        ? NumberFormatter.Format((byte)decimalValue, numberFormat)
                        : NumberFormatter.Format((Word)decimalValue, numberFormat);

                    var binaryValue = register.Is8Bit
                        ? Convert.ToString(decimalValue, 2).PadLeft(8, '0')
                        : Convert.ToString(decimalValue, 2).PadLeft(16, '0');

                    Print($"{register.Name}={hexValue}   Decimal={decimalValue}   Binary={binaryValue}");
                    break;
                }

                case Integer integer:
                    hexValue = NumberFormatter.Format(integer.Value, 4, numberFormat);
                    Print($"Hex={hexValue}   Decimal={integer.Value}");
                    break;

                default:
                    Print(value?.ToString() ?? string.Empty);
                    break;
            }
        }
    }

    private void ExecuteCommand()
    {
        var interpreter = new Interpreter(emulator.Cpu, emulator.Memory, emulator.Bus, this);

        try
        {
            var result = interpreter.Execute(CommandText);

            switch (result)
            {
                case Success:
                    Print("OK");
                    break;

                case Print printValue:
                    Print(printValue);
                    break;

                case Integer intValue:
                    Print(NumberFormatter.Format(intValue.Value, intValue.Type, numberFormat));
                    break;

                case GotoAction gotoAction:
                    Print($"Next instruction address set to ${gotoAction.Address:X4}");
                    break;

                case ListAction listAction:
                    action(listAction);
                    Print($"Listing at ${listAction.Address:X4}");
                    return;

                case SaveAction saveAction:
                    Save(saveAction);
                    break;
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

        refreshAction();
    }

    private void Save(SaveAction save)
    {
        var memory = emulator.Memory.ToBytes();
        var length = save.Length ?? memory.Length - save.Address;

        if (length > memory.Length)
        {
            length = memory.Length;
        }

        try
        {
            using var file = new FileStream(save.FilePath, FileMode.Create);
            file.Write(memory[save.Address..], 0, length);

            Print($"Memory dumped to '{save.FilePath}' {save.Address}:{length}");
        }
        catch (Exception ex)
        {
            Print(ex.Message);
        }
    }
}