using System;
using System.Timers;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace OldBit.Spectron.Controls;

public partial class TimeMachineTimer : UserControl
{
    private readonly Timer _timer = new(TimeSpan.FromSeconds(1));
    private int _value;

    public static readonly StyledProperty<ICommand?> ElapsedCommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(ElapsedCommand));

    public ICommand? ElapsedCommand
    {
        get => GetValue(ElapsedCommandProperty);
        set => SetValue(ElapsedCommandProperty, value);
    }

    public TimeMachineTimer()
    {
        InitializeComponent();

        IsVisibleProperty.Changed.AddClassHandler<TimeMachineTimer>((_, args) => OnIsVisibleChanged(args));
    }

    private void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void StartTimer()
    {
        _value = 3;
        Countdown.Text = _value.ToString();

        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer.Stop();
        _timer.Elapsed -= TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _value -= 1;

        Dispatcher.UIThread.Post(() =>
        {
            Countdown.Text = _value.ToString();

            if (_value == 0)
            {
                StopTimer();
                ElapsedCommand?.Execute(null);
            }
        });
    }
}