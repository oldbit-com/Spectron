using System;
using Avalonia.Threading;

namespace OldBit.Spectron.Controls.Scr;

internal static class FlashTimer
{
    private static Action? _tick;

    internal static bool IsInverted { get; private set; }

    internal static bool IsRunning => Timer.IsEnabled;

    static FlashTimer() => Timer.Tick += (_, _) =>
    {
        IsInverted = !IsInverted;
        _tick?.Invoke();
    };

    internal static event Action Tick
    {
        add
        {
            _tick += value;

            if (!Timer.IsEnabled)
            {
                Timer.Start();
            }
        }
        remove
        {
            _tick -= value;

            if (Timer.IsEnabled && _tick is null )
            {
                Timer.Stop();
            }
        }
    }

    private static readonly DispatcherTimer Timer = new(DispatcherPriority.Render)
    {
        Interval = TimeSpan.FromMilliseconds(16 * 1000 / 50.08)
    };
}