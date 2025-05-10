namespace OldBit.Spectron.Emulation.Commands;

public sealed class CommandManager
{
    public event EventHandler<CommandEventArgs>? CommandReceived;

    internal void SendCommand(ICommand command) =>
        CommandReceived?.Invoke(this, new CommandEventArgs(command));
}