using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Extensions;
using OldBit.ZXSpectrum.Models;

namespace OldBit.ZXSpectrum.Helpers;

internal sealed class FrameBufferConverter : IDisposable
{
    private static readonly Border BorderNone = new();
    private static readonly Border BorderSmall = new(Top: 31, Left: 15, Right: 15, Bottom: 15);
    private static readonly Border BorderMedium = new(Top: 41, Left: 25, Right: 25, Bottom: 25);
    private static readonly Border BorderLarge = new(Top: 57, Left: 40, Right: 40, Bottom: 40);
    private static readonly Border BorderFull = new(Top: 64, Left: 48, Right: 48, Bottom: 56);

    private WriteableBitmap _bitmap;
    private Border _border = BorderFull;

    private readonly int _zoomX;
    private readonly int _zoomY;

    internal FrameBufferConverter(int zoomX, int zoomY)
    {
        _zoomX = zoomX;
        _zoomY = zoomY;

        _bitmap = CreateBitmap();
    }

    internal Bitmap Convert(FrameBuffer frameBuffer)
    {
        var start = (BorderFull.Top - _border.Top) * FrameBuffer.Width + 1;
        var end = frameBuffer.Pixels.Length - (BorderFull.Bottom - _border.Bottom) * FrameBuffer.Width;

        using (var lockedBitmap = _bitmap.Lock())
        {
            var targetAddress = lockedBitmap.Address;

            for (var pixel = start; pixel < end; pixel++)
            {
                var color = frameBuffer.Pixels[pixel].Abgr;

                // Duplicate pixels horizontally
                for (var x = 0; x < _zoomX; x++)
                {
                    unsafe
                    {
                        *(int*)targetAddress = color;
                    }

                    targetAddress += 4;
                }

                if (_zoomY == 1 || pixel % FrameBuffer.Width != 0)
                {
                    continue;
                }

                // Duplicate previous line vertically if zoom factor is greater than 1
                var source = targetAddress - lockedBitmap.RowBytes;
                for (var y = 0; y < _zoomY - 1; y++)
                {
                    unsafe
                    {
                        Buffer.MemoryCopy(source.ToPointer(), targetAddress.ToPointer(), lockedBitmap.RowBytes, lockedBitmap.RowBytes);
                    }

                    targetAddress += lockedBitmap.RowBytes;
                }
            }
        }

        return _bitmap;
    }

    internal void SetBorderSize(BorderSize borderSize)
    {
        _border = borderSize switch
        {
            BorderSize.None => BorderNone,
            BorderSize.Small => BorderSmall,
            BorderSize.Medium => BorderMedium,
            BorderSize.Large => BorderLarge,
            _ => BorderFull,
        };

        _bitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = FrameBuffer.Height - (BorderFull.Top - _border.Top) - (BorderFull.Bottom - _border.Bottom);

        return new WriteableBitmap(
            new PixelSize(
                FrameBuffer.Width * _zoomX,
                height * _zoomY),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }
}