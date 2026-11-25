using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace OldBit.Spectron.Controls.Scr;

public sealed class ScrImage : Control
{
    public static readonly StyledProperty<byte[]?> DataProperty =
        AvaloniaProperty.Register<ScrImage, byte[]?>(nameof(Data));

    private static readonly Size ScreenSize = new(256, 192);

    private WriteableBitmap? _normal;
    private WriteableBitmap? _inverted;

    private bool _isAttached;
    private bool _isSubscribed;

    public byte[]? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public ScrImage()
    {
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataProperty && _isAttached)
        {
            Rebuild();
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _isAttached = true;

        Rebuild();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _isAttached = false;

        ReleaseBitmaps();
        UpdateSubscription();
    }

    public override void Render(DrawingContext context)
    {
        var bitmap = FlashTimer.IsInverted && _inverted is not null ? _inverted : _normal;

        if (bitmap is null)
        {
            return;
        }

        context.DrawImage(bitmap, new Rect(ScreenSize), new Rect(Bounds.Size));
    }

    private void Rebuild()
    {
        ReleaseBitmaps();

        if (Data is { Length: >= 6912 } data)
        {
            _normal = ScrConverter.Decode(data, flashInverted: false);

            if (HasFlash(data))
            {
                _inverted = ScrConverter.Decode(data, flashInverted: true);
            }
        }

        UpdateSubscription();
    }

    private void ReleaseBitmaps()
    {
        _normal?.Dispose();
        _inverted?.Dispose();

        _normal = _inverted = null;
    }

    private void UpdateSubscription()
    {
        var shouldSubscribe = _isAttached && _inverted is not null;

        if (shouldSubscribe == _isSubscribed)
        {
            return;
        }

        if (shouldSubscribe)
        {
            FlashTimer.Tick += InvalidateVisual;
        }
        else
        {
            FlashTimer.Tick -= InvalidateVisual;
        }

        _isSubscribed = shouldSubscribe;
    }

    private static bool HasFlash(ReadOnlySpan<byte> scr)
    {
        foreach (var attr in scr.Slice(6144, 768))
        {
            if ((attr & 0x80) != 0)
            {
                return true;
            }
        }

        return false;
    }
}
