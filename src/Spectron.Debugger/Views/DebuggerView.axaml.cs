using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Views;

public partial class DebuggerView : Window
{
    public DebuggerView() => InitializeComponent();

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}