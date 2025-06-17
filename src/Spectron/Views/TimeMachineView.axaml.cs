using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Messages;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class TimeMachineView : Window
{
    public TimeMachineView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<TimeMachineView, TimeTravelMessage>(this,
            static (window, message) => window.Close(message.Entry));
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
        if (DataContext is not TimeMachineViewModel viewModel)
        {
            return;
        }

        viewModel.PreviewControl = PreviewImage;
        viewModel.Close = Close;
    }

    protected override void OnLoaded(RoutedEventArgs e) => Slider.Focus();
}