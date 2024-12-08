using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace OldBit.Spectron.Controls;

public class PauseOverlay : TemplatedControl, IDisposable
{
    private readonly Timer _timer = new(750);

    private readonly List<TextBlock> _letters = [];
    private readonly List<SolidColorBrush> _colors = [];
    private readonly Random _random = new();

    public PauseOverlay()
    {
        IsVisibleProperty.Changed.AddClassHandler<PauseOverlay>((sender, args) => OnIsVisibleChanged(args));

        AddColors();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        foreach (var ch in "PAUSED")
        {
            AddLetter(e.NameScope, $"PART_Letter{ch}");
        }
    }

    private void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
        else
        {
            _timer.Stop();
            _timer.Elapsed -= TimerOnElapsed;
        }
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_letters.Count != _colors.Count)
        {
            return;
        }

        var randomColors = _colors.OrderBy((_) => _random.Next()).ToList();

        Dispatcher.UIThread.Post(() =>
        {
            for (var i = 0; i < _letters.Count; i++)
            {
                _letters[i].Foreground = randomColors[i];
            }
        });
    }

    private void AddLetter(INameScope nameScope, string name)
    {
        var letter = nameScope.Find<TextBlock>(name);
        if (letter != null)
        {
            _letters.Add(letter);
        }
    }

    private void AddColors()
    {
        AddColor("SpectrumBlack");
        AddColor("SpectrumBrightRed");
        AddColor("SpectrumBrightGreen");
        AddColor("SpectrumBrightYellow");
        AddColor("SpectrumBrightMagenta");
        AddColor("SpectrumBrightBlue");
    }

    private void AddColor(string name)
    {
        if (Application.Current?.Resources.TryGetValue(name, out var style) != true)
        {
            return;
        }

        if (style is SolidColorBrush brush)
        {
            _colors.Add(brush);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}