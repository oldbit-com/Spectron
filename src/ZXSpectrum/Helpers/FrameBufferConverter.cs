using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Helpers;

internal sealed class FrameBufferConverter : IDisposable
{
    private readonly WriteableBitmap _bitmap;
    private readonly int _zoomX;
    private readonly int _zoomY;

    internal FrameBufferConverter(int zoomX, int zoomY)
    {
        _zoomX = zoomX;
        _zoomY = zoomY;

        _bitmap = new WriteableBitmap(
            new PixelSize(
                FrameBuffer.Width * _zoomX,
                FrameBuffer.Height * zoomY),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    internal Bitmap Convert(FrameBuffer frameBuffer)
    {
        using (var lockedBitmap = _bitmap.Lock())
        {
            var targetAddress = lockedBitmap.Address;

            for (var pixel = 0; pixel < frameBuffer.Pixels.Length; pixel++)
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

    public void Dispose()
    {
        _bitmap.Dispose();
    }
}