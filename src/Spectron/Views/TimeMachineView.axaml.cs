using ReactiveUI;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using Avalonia.Input;

namespace OldBit.Spectron.Views;

public partial class TimeMachineView : ReactiveWindow<TimeMachineViewModel>
{
    public TimeMachineView()
    {
        InitializeComponent();

        this.WhenActivated(action => action(ViewModel!.TimeTravelCommand.Subscribe(isSuccess =>
        {
            if (isSuccess)
            {
                Close();
            }
        })));
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
    }
}