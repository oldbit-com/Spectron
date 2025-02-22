using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Screen;

/// <summary>
/// Converts and writes the frame buffer to a WriteableBitmap which can be displayed by Avalonia.
/// </summary>
internal sealed class FrameBufferConverter : IDisposable
{
    private Border _border = BorderSizes.Full;

    private int _startFrameBufferRow;
    private int _endFrameBufferRow;
    private int _startFrameBufferCol;
    private int _endFrameBufferCol;

    private readonly int _zoomX;
    private readonly int _zoomY;

    internal WriteableBitmap Bitmap { get; private set; }

    internal FrameBufferConverter(int zoomX, int zoomY)
    {
        _zoomX = zoomX;
        _zoomY = zoomY;

        SetBorderSize(BorderSize.Full);
        Bitmap = CreateBitmap();
    }

    internal void UpdateBitmap(FrameBuffer frameBuffer)
    {
        using var lockedBitmap = Bitmap.Lock();
        var targetAddress = lockedBitmap.Address;

        for (var frameBufferRow = _startFrameBufferRow; frameBufferRow <= _endFrameBufferRow; frameBufferRow++)
        {
            var rowOffset = frameBufferRow * FrameBuffer.Width;

            for (var frameBufferCol = _startFrameBufferCol; frameBufferCol <= _endFrameBufferCol; frameBufferCol++)
            {
                var pixelIndex = rowOffset + frameBufferCol;

                // Duplicate pixels horizontally
                for (var x = 0; x < _zoomX; x++)
                {
                    unsafe
                    {
                        fixed (Color* color = &frameBuffer.Pixels[pixelIndex])
                        {
                            *(uint*)targetAddress = *(uint*)color;
                        }
                    }

                    targetAddress += 4;
                }
            }

            // Duplicate previous line vertically based on zoom factor
            var previousLine = targetAddress - lockedBitmap.RowBytes;

            for (var y = 0; y < _zoomY - 1; y++)
            {
                unsafe
                {
                    Buffer.MemoryCopy(previousLine.ToPointer(), targetAddress.ToPointer(), lockedBitmap.RowBytes, lockedBitmap.RowBytes);
                }

                targetAddress += lockedBitmap.RowBytes;
            }
        }
    }

    internal void SetBorderSize(BorderSize borderSize)
    {
        _border = borderSize switch
        {
            BorderSize.None => BorderSizes.None,
            BorderSize.Small => BorderSizes.Small,
            BorderSize.Medium => BorderSizes.Medium,
            BorderSize.Large => BorderSizes.Large,
            _ => BorderSizes.Full,
        };

        _startFrameBufferRow = BorderSizes.Full.Top - _border.Top;
        _endFrameBufferRow = FrameBuffer.Height - (BorderSizes.Full.Bottom - _border.Bottom) - 1;
        _startFrameBufferCol = BorderSizes.Full.Left - _border.Left;
        _endFrameBufferCol = FrameBuffer.Width - (BorderSizes.Full.Right - _border.Right) - 1;

        Bitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = FrameBuffer.Height - (BorderSizes.Full.Top - _border.Top) - (BorderSizes.Full.Bottom - _border.Bottom);
        var width = FrameBuffer.Width - (BorderSizes.Full.Left - _border.Left) - (BorderSizes.Full.Right - _border.Right);

        return new WriteableBitmap(
            new PixelSize(
                width * _zoomX,
                height * _zoomY),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose() => Bitmap.Dispose();
}