using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.Debugger.ViewModels.Overlays;

namespace OldBit.Spectron.Debugger.Controls.Overlays;

public partial class GoToAddressOverlay : UserControl
{
    private GoToAddressOverlayViewModel? _viewModel;

    public GoToAddressOverlay() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not GoToAddressOverlayViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;

        _viewModel.Show = SHow;
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

        base.OnKeyDown(e);
    }

    private void SHow()
    {
        Overlay.IsVisible = true;
        AddressBox.Focus();
    }

    private void Hide() => Overlay.IsVisible = false;
}