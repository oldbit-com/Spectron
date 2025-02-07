using Avalonia;
using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Controls;

public partial class CodeLine : UserControl
{
    public static readonly StyledProperty<bool> IsCurrentProperty =
        AvaloniaProperty.Register<CodeLine, bool>(nameof(IsCurrent));

    public static readonly StyledProperty<bool> IsBreakpointProperty =
        AvaloniaProperty.Register<CodeLine, bool>(nameof(IsBreakpoint));

    public static readonly StyledProperty<Word> AddressProperty =
        AvaloniaProperty.Register<CodeLine, Word>(nameof(Address));

    public static readonly StyledProperty<string> CodeProperty =
        AvaloniaProperty.Register<CodeLine, string>(nameof(Code));

    public bool IsCurrent
    {
        get => GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public bool IsBreakpoint
    {
        get => GetValue(IsBreakpointProperty);
        set => SetValue(IsBreakpointProperty, value);
    }

    public Word Address
    {
        get => GetValue(AddressProperty);
        set => SetValue(AddressProperty, value);
    }

    public string Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public CodeLine() => InitializeComponent();
}