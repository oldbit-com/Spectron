using System;
using System.Diagnostics.CodeAnalysis;
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
    private Border _border = BorderSizes.Full;

    private int _startFrameBufferRow;
    private int _endFrameBufferRow;
    private int _startFrameBufferCol;
    private int _endFrameBufferCol;

    private readonly FrameBuffer _frameBuffer;

    internal WriteableBitmap ScreenBitmap { get; private set; }
    internal ScreenEffect ScreenEffect { get; set; }
    public bool IsHiRes => _frameBuffer.IsHiRes;

    internal FrameBufferConverter(FrameBuffer frameBuffer, BorderSize borderSize, ScreenEffect screenEffect)
    {
        _frameBuffer = frameBuffer;
        ScreenEffect = screenEffect;
        SetBorderSize(borderSize);
    }

    internal void UpdateBitmap()
    {
        using var lockedBitmap = ScreenBitmap.Lock();

        var colCount = _endFrameBufferCol - _startFrameBufferCol + 1;
        var rowBytes = colCount * 4;

        unsafe
        {
            fixed (Color* pixelsBase = _frameBuffer.Pixels)
            {
                if (ScreenEffect.HasFlag(ScreenEffect.Blur))
                {
                    UpdateBitmapWithBlur(lockedBitmap, pixelsBase, colCount);
                }
                else
                {
                    UpdateBitmap(lockedBitmap, pixelsBase, rowBytes);
                }
            }
        }
    }

    private unsafe void UpdateBitmap(ILockedFramebuffer lockedBitmap, Color* pixelsBase, int rowBytes)
    {
        var destination = (byte*)lockedBitmap.Address;

        for (var row = _startFrameBufferRow; row <= _endFrameBufferRow; row++)
        {
            var source = (byte*)(pixelsBase + row * _frameBuffer.Width + _startFrameBufferCol);
            Buffer.MemoryCopy(source, destination, rowBytes, rowBytes);
            destination += rowBytes;
        }
    }

    private unsafe void UpdateBitmapWithBlur(ILockedFramebuffer lockedBitmap, Color* pixelsBase, int colCount)
    {
        var destination = (uint*)lockedBitmap.Address;

        for (var row = _startFrameBufferRow; row <= _endFrameBufferRow; row++)
        {
            var source = pixelsBase + row * _frameBuffer.Width + _startFrameBufferCol;
            BlurRow(source, destination, colCount);
            destination += colCount;
        }
    }

    private static uint Pack(byte r, byte g, byte b) => 0xFF000000u | ((uint)b << 16) | ((uint)g << 8) | r;

    private static unsafe void BlurRow(Color* source, uint* dest, int colCount)
    {
        dest[0] = Pack(source[0].Red, source[0].Green, source[0].Blue);

        for (var column = 1; column < colCount - 1; column++)
        {
            // Horizontal [1,6,1]/8 blur — very subtle CRT softening, applied in source-pixel space
            var r = (source[column - 1].Red   + 6 * source[column].Red   + source[column + 1].Red)   >> 3;
            var g = (source[column - 1].Green + 6 * source[column].Green + source[column + 1].Green) >> 3;
            var b = (source[column - 1].Blue  + 6 * source[column].Blue  + source[column + 1].Blue)  >> 3;

            dest[column] = Pack((byte)r, (byte)g, (byte)b);
        }

        dest[colCount - 1] = Pack(source[colCount - 1].Red, source[colCount - 1].Green, source[colCount - 1].Blue);
    }

    [MemberNotNull(nameof(ScreenBitmap))]
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

        // In hi-res mode the frame buffer border is doubled, so the skip amount scales accordingly.
        var borderMultiplier = _frameBuffer.Width / (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight);

        _startFrameBufferRow = BorderSizes.Max.Top - _border.Top;
        _endFrameBufferRow = _frameBuffer.Height - (BorderSizes.Max.Bottom - _border.Bottom) - 1;
        _startFrameBufferCol = (BorderSizes.Max.Left - _border.Left) * borderMultiplier;
        _endFrameBufferCol = _frameBuffer.Width - (BorderSizes.Max.Right - _border.Right) * borderMultiplier - 1;

        ScreenBitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = _endFrameBufferRow - _startFrameBufferRow + 1;
        var width = _endFrameBufferCol - _startFrameBufferCol + 1;

        return new WriteableBitmap(
            new PixelSize(
                width,
                height),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose() => ScreenBitmap.Dispose();
}