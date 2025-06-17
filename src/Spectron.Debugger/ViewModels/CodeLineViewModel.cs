using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class CodeLineViewModel : ObservableObject,
    IRecipient<BreakpointAddedMessage>,
    IRecipient<BreakpointRemovedMessage>,
    IRecipient<BreakpointUpdatedMessage>
{
    [ObservableProperty]
    private Word _address;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _isCurrent;

    [ObservableProperty]
    private bool _isBreakpoint;

    public CodeLineViewModel(
        Word address,
        string code,
        bool isCurrent,
        bool isBreakpoint)
    {
        Address = address;
        Code = code;
        IsCurrent = isCurrent;
        IsBreakpoint = isBreakpoint;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void ToggleBreakpoint()
    {
        IsBreakpoint = !IsBreakpoint;
        WeakReferenceMessenger.Default.Send(new ToggleBreakpointMessage(Register.PC, Address, IsBreakpoint));
    }

    public void Receive(BreakpointAddedMessage message)
    {
        if (Address == message.Address)
        {
            IsBreakpoint = true;
        }
    }

    public void Receive(BreakpointRemovedMessage message)
    {
        if (Address == message.Address)
        {
            IsBreakpoint = false;
        }
    }

    public void Receive(BreakpointUpdatedMessage message)
    {
        if (Address == message.OldAddress)
        {
            IsBreakpoint = false;
        }

        if (Address == message.NewAddress)
        {
            IsBreakpoint = true;
        }
    }
}