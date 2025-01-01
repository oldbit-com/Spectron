using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OldBit.Spectron.Views;

public partial class TimeMachineView : ReactiveWindow<TimeMachineViewModel>
{
    public TimeMachineView()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel!.PreviewControl = PreviewImage;
        }

        ViewModel?.TimeTravelCommand.Subscribe(Close);
    }

    protected override void OnLoaded(RoutedEventArgs e) => Slider.Focus();
}