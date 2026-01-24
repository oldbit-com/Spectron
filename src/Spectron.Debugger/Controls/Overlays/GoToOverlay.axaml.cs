using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Controls.Overlays;

public partial class GoToOverlay : BaseOverlay
{
    public GoToOverlay()
    {
        InitializeComponent();
        Dialog.RenderTransform = DialogTransform;

        DragHandle = DragHandleControl;
        Overlay = OverlayControl;
    }

    protected override void Focus()
    {
        AddressBox.Focus();
        AddressBox.SelectAll();
    }

    protected override Control DragHandle { get; }
    protected override Control Overlay { get; }
}
