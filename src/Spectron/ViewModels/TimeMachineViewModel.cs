using System;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TimeMachineViewModel : ViewModelBase
{
    private readonly TimeMachine _timeMachine;
    private const int PreviewHeight = 192;
    private const int PreviewWidth = 256;

    public ReactiveCommand<Unit, bool> TimeTravelCommand { get; private set; }
    public Control? PreviewControl { get; set; }
    public Action<TimeMachineEntry>? OnTimeTravel { get; set; }

    public TimeMachineViewModel(TimeMachine timeMachine)
    {
        _timeMachine = timeMachine;
        TimeTravelCommand = ReactiveCommand.Create(HandleTimeTravel);

        this.WhenAny(x => x.CurrentEntryIndex, x => x.Value)
            .Subscribe(_ => UpdatePreview());
    }

    public void BeforeShow()
    {
        EntriesCount = _timeMachine.Entries.Count - 1;
        CurrentEntryIndex = EntriesCount;
    }

    private bool HandleTimeTravel()
    {
        if (_currentEntryIndex >= EntriesCount)
        {
            return false;
        }

        var timeMachineEntry = GetSelectedEntry();

        if (timeMachineEntry == null)
        {
            return false;
        }

        OnTimeTravel?.Invoke(timeMachineEntry);

        return true;
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

        PreviewControl?.InvalidateVisual();
    }

    private TimeMachineEntry? GetSelectedEntry()
    {
        var index = (int)_currentEntryIndex;
        if (index >= 0 && index < _timeMachine.Entries.Count)
        {
            return _timeMachine.Entries[index];
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
        set => this.RaiseAndSetIfChanged(ref _currentEntryIndex, value);
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