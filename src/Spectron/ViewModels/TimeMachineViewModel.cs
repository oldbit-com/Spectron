using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.TimeMachine;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TimeMachineViewModel : ViewModelBase
{
    private const int PreviewHeight = 192;
    private const int PreviewWidth = 256;

    private IReadOnlyList<TimeMachineState>? _snapshotStates;

    public Control PreviewControl { get; set; } = null!;

    public void Update(IReadOnlyList<TimeMachineState>? snapshotStates)
    {
        _snapshotStates = snapshotStates;
        Maximum = _snapshotStates?.Count - 1 ?? 0;
        SelectedValue = Maximum;
    }

    private void UpdateScreen()
    {
        var index = (int)_selectedValue;
        var state = _snapshotStates?[index];
        var screenshot = state?.Snapshot.GetScreenshot();

        if (screenshot == null)
        {
            return;
        }

        using (var bitmap = ScreenPreview.Lock())
        {
            Marshal.Copy(screenshot, 0, bitmap.Address, screenshot.Length);
        }

        if (state?.Snapshot.SpecRegs.Border != null)
        {
            var borderColor = SpectrumPalette.GetBorderColor(state.Snapshot.SpecRegs.Border);
            ScreenBorderBrush = new SolidColorBrush((uint)borderColor.Argb);
        }

        PreviewControl.InvalidateVisual();
    }

    private int _maximum = 1;
    public int Maximum
    {
        get => _maximum;
        set => this.RaiseAndSetIfChanged(ref _maximum, value);
    }

    private double _selectedValue;
    public double SelectedValue
    {
        get => _selectedValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedValue, value);
            UpdateScreen();
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