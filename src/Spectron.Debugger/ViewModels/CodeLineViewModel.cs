using OldBit.Spectron.Debugger.Breakpoints;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeLineViewModel : ReactiveObject
{
    private readonly BreakpointListViewModel _breakpointListViewModel;
    private bool _suppressBreakpointSubscription = true;

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

        this.WhenAny(x => x.IsBreakpoint, x => x.Value)
            .Subscribe(value =>
            {
                if (!_suppressBreakpointSubscription)
                {
                    ToggleBreakpoint(value);
                }
            });

        _suppressBreakpointSubscription = false;
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

    private Word _address;
    public Word Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    private string _code = string.Empty;
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    private bool _isCurrent;
    public bool IsCurrent
    {
        get => _isCurrent;
        set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
    }

    private bool _isBreakpoint;
    public bool IsBreakpoint
    {
        get => _isBreakpoint;
        set => this.RaiseAndSetIfChanged(ref _isBreakpoint, value);
    }
}