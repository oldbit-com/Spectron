using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class CodeLineViewModel : ObservableObject
{
    private readonly BreakpointListViewModel _breakpointListViewModel;
    private bool _suppressBreakpointSubscription;

    [ObservableProperty]
    private Word _address;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _isCurrent;

    [ObservableProperty]
    private bool _isBreakpoint;

    public CodeLineViewModel(
        BreakpointListViewModel breakpointListViewModel,
        Word address,
        string code,
        bool isCurrent,
        bool isBreakpoint)
    {
        _breakpointListViewModel = breakpointListViewModel;

        Address = address;
        Code = code;
        IsCurrent = isCurrent;
        IsBreakpoint = isBreakpoint;

        _suppressBreakpointSubscription = false;
    }

    partial void OnIsBreakpointChanged(bool value)
    {
        if (!_suppressBreakpointSubscription)
        {
            ToggleBreakpoint(value);
        }
    }

    private void ToggleBreakpoint(bool isBreakpoint)
    {
        if (isBreakpoint)
        {
            _breakpointListViewModel.AddBreakpoint(Register.PC, Address);
        }
        else
        {
            _breakpointListViewModel.RemoveBreakpoint(Register.PC, Address);
        }
    }

    public void UpdateBreakpoint(bool isBreakpoint)
    {
        _suppressBreakpointSubscription = true;

        IsBreakpoint = isBreakpoint;

        _suppressBreakpointSubscription = false;
    }
}