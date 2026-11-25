using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using OldBit.Spectron.Controls.Scr;

namespace OldBit.Spectron.Tests.Controls;

public class ScrImageTests
{
    [AvaloniaFact]
    public void ShouldSubscribeToFlashTimerOnlyWhileAttachedWithFlashData()
    {
        var image = new ScrImage { Data = CreateScr(flash: true) };
        var window = new Window { Content = image };

        FlashTimer.IsRunning.ShouldBeFalse();

        window.Show();
        FlashTimer.IsRunning.ShouldBeTrue();

        window.Close();
        FlashTimer.IsRunning.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ShouldUnsubscribeFromFlashTimerWhenDataHasNoFlash()
    {
        var image = new ScrImage { Data = CreateScr(flash: true) };
        var window = new Window { Content = image };

        window.Show();
        FlashTimer.IsRunning.ShouldBeTrue();

        image.Data = CreateScr(flash: false);
        FlashTimer.IsRunning.ShouldBeFalse();

        image.Data = CreateScr(flash: true);
        FlashTimer.IsRunning.ShouldBeTrue();

        window.Close();
        FlashTimer.IsRunning.ShouldBeFalse();
    }

    private static byte[] CreateScr(bool flash)
    {
        var data = new byte[6912];
        data[6144] = flash ? (byte)0x87 : (byte)0x07;

        return data;
    }
}
