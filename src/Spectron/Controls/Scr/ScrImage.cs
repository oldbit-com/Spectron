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

        if (change.Property == DataProperty)
        {
            Rebuild();
            InvalidateVisual();
        }
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
        _normal?.Dispose();
        _inverted?.Dispose();

        _normal = _inverted = null;

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

    private void UpdateSubscription()
    {
        if (_inverted is null)
        {
            return;
        }

        FlashTimer.Tick -= InvalidateVisual;
        FlashTimer.Tick += InvalidateVisual;
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