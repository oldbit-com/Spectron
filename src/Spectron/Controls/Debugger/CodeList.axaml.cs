using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.ViewModels.Debugger;

namespace OldBit.Spectron.Controls.Debugger;

public partial class CodeList : UserControl
{
    public CodeList()
    {
        InitializeComponent();
    }

    private void ListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBox _ || e.Source is not Control control)
        {
            return;
        }

        if (control.DataContext is DebuggerCodeLineViewModel codeLine)
        {
            codeLine.IsBreakpoint = !codeLine.IsBreakpoint;
        }
    }
}