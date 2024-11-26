using ReactiveUI;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using Avalonia.Input;

namespace OldBit.Spectron.Views;

public partial class GamepadMappingView : ReactiveWindow<GamepadMappingViewModel>
{
    public GamepadMappingView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        this.WhenActivated(action => action(ViewModel!.UpdateMappingCommand.Subscribe(Close)));
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
}