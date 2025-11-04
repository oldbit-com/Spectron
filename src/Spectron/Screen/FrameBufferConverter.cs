using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Screen;

/// <summary>
/// Converts and writes the frame buffer to a WriteableBitmap which Avalonia can display.
/// </summary>
internal sealed class FrameBufferConverter : IDisposable
{
    private const int ZoomX = 4;      // Number of horizontal pixels, check the below code if changing value
    private const int ZoomY = 4;      // Number of vertical pixels

    private Border _border = BorderSizes.Full;

    private int _startFrameBufferRow;
    private int _endFrameBufferRow;
    private int _startFrameBufferCol;
    private int _endFrameBufferCol;

    internal WriteableBitmap ScreenBitmap { get; private set; }

    internal FrameBufferConverter()
    {
        SetBorderSize(BorderSize.Full);
        ScreenBitmap = CreateBitmap();
    }

    internal void UpdateBitmap(FrameBuffer frameBuffer)
    {
        using var lockedBitmap = ScreenBitmap.Lock();

        var targetAddress = lockedBitmap.Address;
        var rowBytes = lockedBitmap.RowBytes;

        for (var frameBufferRow = _startFrameBufferRow; frameBufferRow <= _endFrameBufferRow; frameBufferRow++)
        {
            var rowOffset = frameBufferRow * FrameBuffer.Width;

            for (var frameBufferCol = _startFrameBufferCol; frameBufferCol <= _endFrameBufferCol; frameBufferCol++)
            {
                var pixelIndex = rowOffset + frameBufferCol;

                unsafe
                {
                    fixed (Color* color = &frameBuffer.Pixels[pixelIndex])
                    {
                        var pixelColor = *(uint*)color;

                        // Replicate pixels horizontally, unrolled loop for better performance.
                        // IMPORTANT: This needs to be in sync with ZoomX value
                        *(uint*)targetAddress = pixelColor;
                        *(uint*)(targetAddress + 4) = pixelColor;
                        *(uint*)(targetAddress + 8) = pixelColor;
                        *(uint*)(targetAddress + 12) = pixelColor;

                        targetAddress += 4 * ZoomX;
                    }
                }
            }

            var previousLine = targetAddress - rowBytes;

            // Replicate the previous line vertically, no need to unroll this loop
            for (var y = 0; y < ZoomY - 1; y++)
            {
                unsafe
                {
                    Buffer.MemoryCopy(previousLine.ToPointer(), targetAddress.ToPointer(), rowBytes, rowBytes);
                }

                targetAddress += rowBytes;
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

        _startFrameBufferRow = BorderSizes.Max.Top - _border.Top;
        _endFrameBufferRow = FrameBuffer.Height - (BorderSizes.Max.Bottom - _border.Bottom) - 1;
        _startFrameBufferCol = BorderSizes.Max.Left - _border.Left;
        _endFrameBufferCol = FrameBuffer.Width - (BorderSizes.Max.Right - _border.Right) - 1;

        ScreenBitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = FrameBuffer.Height - (BorderSizes.Max.Top - _border.Top) - (BorderSizes.Max.Bottom - _border.Bottom);
        var width = FrameBuffer.Width - (BorderSizes.Max.Left - _border.Left) - (BorderSizes.Max.Right - _border.Right);

        return new WriteableBitmap(
            new PixelSize(
                width * ZoomX,
                height * ZoomY),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose() => ScreenBitmap.Dispose();
}