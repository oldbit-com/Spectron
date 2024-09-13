using ReactiveUI;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using Avalonia.Input;

namespace OldBit.Spectron.Views;

public partial class PreferencesView : ReactiveWindow<PreferencesViewModel>
{
    public PreferencesView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        this.WhenActivated(action => action(ViewModel!.UpdatePreferencesCommand.Subscribe(Close)));
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}