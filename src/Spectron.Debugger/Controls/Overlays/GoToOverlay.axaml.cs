using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.Debugger.ViewModels.Overlays;

namespace OldBit.Spectron.Debugger.Controls.Overlays;

public partial class GoToOverlay : UserControl
{
    private GoToOverlayViewModel? _viewModel;

    public GoToOverlay() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not GoToOverlayViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;

        _viewModel.Show = Show;
        _viewModel.Hide = Hide;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!Overlay.IsVisible)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }

        if (e.Key == Key.Enter && _viewModel?.HasErrors == false)
        {
            _viewModel.OnGoTo();
            e.Handled = true;
        }
    }

    private void Show()
    {
        Overlay.IsVisible = true;
        AddressBox.Focus();
        AddressBox.SelectAll();
    }

    private void Hide() => Overlay.IsVisible = false;
}