using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeLineViewModel : ReactiveObject
{
    private readonly DebuggerContext _debuggerContext;

    public CodeLineViewModel(DebuggerContext debuggerContext)
    {
        _debuggerContext = debuggerContext;

        this.WhenAny(x => x.IsBreakpoint, x => x.Value)
            .Subscribe(ToggleBreakpoint);
    }

    private void ToggleBreakpoint(bool isBreakpoint)
    {
        if (isBreakpoint)
        {
            _debuggerContext.AddBreakpoint(Address);
        }
        else
        {
            _debuggerContext.RemoveBreakpoint(Address);
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