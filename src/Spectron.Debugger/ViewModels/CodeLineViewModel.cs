using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeLineViewModel : ReactiveObject
{
    private readonly BreakpointListViewModel _breakpointListViewModel;

    public CodeLineViewModel(BreakpointListViewModel breakpointListViewModel, bool isBreakpoint)
    {
        _breakpointListViewModel = breakpointListViewModel;

        IsBreakpoint = isBreakpoint;

        this.WhenAny(x => x.IsBreakpoint, x => x.Value)
            .Subscribe(ToggleBreakpoint);
    }

    private void ToggleBreakpoint(bool isBreakpoint)
    {
        if (isBreakpoint)
        {
            _breakpointListViewModel.AddBreakpoint(Address);
        }
        else
        {
            _breakpointListViewModel.RemoveBreakpoint(Address);
        }
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