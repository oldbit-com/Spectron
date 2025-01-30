using ReactiveUI;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerCodeLineViewModel : ViewModelBase
{
    private string _address = string.Empty;
    public string Address
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