using ReactiveUI;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;

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
}