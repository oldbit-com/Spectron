using ReactiveUI;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;

namespace OldBit.Spectron.Views;

public partial class SelectFileView : ReactiveWindow<SelectFileViewModel>
{
    public SelectFileView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        this.WhenActivated(action => action(ViewModel!.SelectFileCommand.Subscribe(Close)));
    }
}