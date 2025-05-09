using System;
using Avalonia;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.Input;

public sealed class MouseHelper(MouseManager mouseManager)
{
    public void MouseMoved(BorderSize borderSize, Point position, Rect bounds)
    {
        if (mouseManager.MouseType == MouseType.None)
        {
            return;
        }

        // Visible area of the screen
        var currentBorder = BorderSizes.GetBorder(borderSize);
        var width = currentBorder.Left + ScreenSize.ContentWidth + currentBorder.Right;
        var height = currentBorder.Top + ScreenSize.ContentHeight + currentBorder.Bottom;

        // Pixel scaling factor
        var factorW = bounds.Width / width;
        var factorH = bounds.Height / height;

        // Left and top border positions
        var left = factorW * currentBorder.Left;
        var top = factorH * currentBorder.Top;

        var maxX = left + ScreenSize.ContentWidth * factorW;
        var maxY = top + ScreenSize.ContentHeight * factorH;

        var positionX = position.X;
        var positionY = position.Y;

        if (positionX <= left)
        {
            positionX = left;
        }

        if (positionX >= maxX)
        {
            positionX = maxX;
        }

        if (positionY <= top)
        {
            positionY = top;
        }

        if (positionY >= maxY)
        {
            positionY = maxY;
        }

        // Mouse position is a byte
        var scaleX = 256 / (maxX - left);
        var scaleY = 256 / (maxY - top);

        var x = Math.Floor(positionX - left);
        var y = Math.Floor(positionY - top);

        var scaledX = (byte)Math.Floor(x * scaleX);
        var scaledY = 255 - (byte)Math.Floor(y * scaleY);

        mouseManager.UpdatePosition(scaledX, scaledY);
    }

    public void ButtonsStateChanged(PointerPoint point)
    {
        if (mouseManager.MouseType == MouseType.None)
        {
            return;
        }

        var pressedButtons = GetPressedMouseButtons(point);

        mouseManager.UpdateMouseButtons(pressedButtons);
    }


    private static MouseButtons GetPressedMouseButtons(PointerPoint point)
    {
        var buttons = MouseButtons.None;

        buttons |= point.Properties.IsLeftButtonPressed ? MouseButtons.Left : MouseButtons.None;
        buttons |= point.Properties.IsRightButtonPressed ? MouseButtons.Right : MouseButtons.None;
        buttons |= point.Properties.IsMiddleButtonPressed ? MouseButtons.Middle : MouseButtons.None;

        return buttons;
    }
}