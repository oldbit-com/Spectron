using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Controls;

public partial class CodeList : UserControl
{
    public CodeList() => InitializeComponent();

    private void ListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBox _ || e.Source is not Control control)
        {
            return;
        }

        if (control.DataContext is CodeLineViewModel codeLine)
        {
            codeLine.ToggleBreakpoint();
        }
    }
}