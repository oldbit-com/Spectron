using Avalonia.Controls;

namespace OldBit.Spectron.Debugger.Controls.Overlays;

public partial class FindOverlay : BaseOverlay
{
    public FindOverlay()
    {
        InitializeComponent();
        Dialog.RenderTransform = DialogTransform;

        DragHandle = DragHandleControl;
        Overlay = OverlayControl;
    }

    protected override void Focus()
    {
        FindBox.Focus();
        FindBox.SelectAll();
    }

    protected override Control DragHandle { get; }
    protected override Control Overlay { get; }
}
