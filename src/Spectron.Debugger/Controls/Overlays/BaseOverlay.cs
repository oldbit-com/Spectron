using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.ViewModels.Overlays;

namespace OldBit.Spectron.Debugger.Controls.Overlays;

public abstract class BaseOverlay : UserControl
{
    protected readonly TranslateTransform DialogTransform = new();

    private bool _isDragging;
    private Point _dragStartPoint;
    private double _dragStartX;
    private double _dragStartY;

    protected abstract void Focus();
    protected abstract Control DragHandle { get; }
    protected abstract Control Overlay { get; }
    protected BaseOverlayViewModel? ViewModel { get; private set; }

    protected void Show()
    {
        IsVisible = true;
        Dispatcher.UIThread.Post(Focus, DispatcherPriority.Input);
    }

    protected void Hide() => IsVisible = false;

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not BaseOverlayViewModel viewModel)
        {
            return;
        }

        ViewModel = viewModel;

        ViewModel.Show = Show;
        ViewModel.Hide = Hide;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }

        if (e.Key == Key.Enter && ViewModel?.HasErrors == false)
        {
            ViewModel.OnExecute();
            e.Handled = true;
        }
    }

    protected void DragHandle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsVisible || !e.GetCurrentPoint(DragHandle).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isDragging = true;
        _dragStartPoint = e.GetPosition(Overlay);

        _dragStartX = DialogTransform.X;
        _dragStartY = DialogTransform.Y;

        e.Pointer.Capture(DragHandle);
        e.Handled = true;
    }

    protected void DragHandle_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        var currentPoint = e.GetPosition(Overlay);
        var delta = currentPoint - _dragStartPoint;

        DialogTransform.X = _dragStartX + delta.X;
        DialogTransform.Y = _dragStartY + delta.Y;

        e.Handled = true;
    }

    protected void DragHandle_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;

        if (Equals(e.Pointer.Captured, DragHandle))
        {
            e.Pointer.Capture(null);
        }

        e.Handled = true;
    }
}