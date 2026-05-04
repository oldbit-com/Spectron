using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.ViewModels;

public partial class MainViewModel
{
    private async Task HandleScreenshot()
    {
        try
        {
            var screen = ResizeScreenForCapture();
            _screenshotViewModel.AddScreenshot(screen);

            if (MainWindow?.Clipboard is not null)
            {
                await MainWindow.Clipboard.SetBitmapAsync(screen);
            }

            NotificationManager.Show("Screenshot Taken", NotificationType.Success, TimeSpan.FromSeconds(.75));
        }
        catch (Exception ex)
        {
            NotificationManager.Show($"Error: {ex.Message}", NotificationType.Error, TimeSpan.FromSeconds(1.5));
            _logger.LogError(ex, "Failed to take screenshot");
        }
    }

    private RenderTargetBitmap? ResizeScreenForCapture()
    {
        if (SpectrumScreen is null)
        {
            return null;
        }

        var targetWidth = SpectrumScreen.PixelSize.Width * 8;
        var targetHeight = SpectrumScreen.PixelSize.Height * 8;

        if (_frameBufferConverter?.IsHiRes == true)
        {
            targetWidth /= 2;
        }

        var scaled = new RenderTargetBitmap(new PixelSize(targetWidth, targetHeight), SpectrumScreen.Dpi);

        using var context = scaled.CreateDrawingContext();
        context.PushRenderOptions(new RenderOptions
        {
            BitmapInterpolationMode = BitmapInterpolationMode.None
        });

        context.DrawImage(
            SpectrumScreen,
            sourceRect: new Rect(0, 0, SpectrumScreen.PixelSize.Width, SpectrumScreen.PixelSize.Height),
            destRect: new Rect(0, 0, targetWidth, targetHeight));

        return scaled;
    }
}