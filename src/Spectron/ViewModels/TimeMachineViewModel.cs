using System;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.TimeTravel;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TimeMachineViewModel : ViewModelBase
{
    private const int PreviewHeight = 192;
    private const int PreviewWidth = 256;

    public ReactiveCommand<Unit, Unit> TimeTravelCommand { get; private set; }
    public Control PreviewControl { get; set; } = null!;
    public Action<TimeMachineEntry>? OnTimeTravel { get; set; }

    public TimeMachineViewModel()
    {
        TimeTravelCommand = ReactiveCommand.Create(HandleTimeTravel);
    }

    public void BeforeShow()
    {
        EntriesCount = TimeMachine.Instance.Entries.Count - 1;
        CurrentEntryIndex = EntriesCount;
    }

    private void HandleTimeTravel()
    {
        if (_currentEntryIndex >= EntriesCount)
        {
            return;
        }

        var timeMachineEntry = GetSelectedEntry();
        if (timeMachineEntry != null)
        {
            OnTimeTravel?.Invoke(timeMachineEntry);
        }
    }

    private void UpdatePreview()
    {
        var timeMachineEntry = GetSelectedEntry();
        if (timeMachineEntry == null)
        {
            return;
        }

        var screenshot = timeMachineEntry.Snapshot.GetScreenshot();

        using (var bitmap = ScreenPreview.Lock())
        {
            Marshal.Copy(screenshot, 0, bitmap.Address, screenshot.Length);
        }

        var borderColor = SpectrumPalette.GetBorderColor(timeMachineEntry.Snapshot.SpecRegs.Border);
        ScreenBorderBrush = new SolidColorBrush((uint)borderColor.Argb);
        PreviewControl.InvalidateVisual();
    }

    private TimeMachineEntry? GetSelectedEntry()
    {
        var index = (int)_currentEntryIndex;
        if (index >= 0 && index < TimeMachine.Instance.Entries.Count)
        {
            return TimeMachine.Instance.Entries[index];
        }

        return null;
    }

    private int _entriesCount;
    public int EntriesCount
    {
        get => _entriesCount;
        set => this.RaiseAndSetIfChanged(ref _entriesCount, value);
    }

    private double _currentEntryIndex;
    public double CurrentEntryIndex
    {
        get => _currentEntryIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentEntryIndex, value);
            UpdatePreview();
        }
    }

    private WriteableBitmap _screenPreview = new(
        new PixelSize(PreviewWidth, PreviewHeight),
        new Vector(96, 96),
        PixelFormats.Rgba8888);

    public WriteableBitmap ScreenPreview
    {
        get => _screenPreview;
        set => this.RaiseAndSetIfChanged(ref _screenPreview, value);
    }

    private Brush _screenBorderBrush = new SolidColorBrush(Colors.Black);
    public Brush ScreenBorderBrush
    {
        get => _screenBorderBrush;
        set => this.RaiseAndSetIfChanged(ref _screenBorderBrush, value);
    }
}