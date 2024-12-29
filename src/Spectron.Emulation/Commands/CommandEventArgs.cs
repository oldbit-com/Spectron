namespace OldBit.Spectron.Emulation.Commands;

public class CommandEventArgs(ICommand command) : EventArgs
{
    public ICommand Command { get; } = command;
}