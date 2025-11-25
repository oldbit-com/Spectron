using System;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Logging;

namespace OldBit.Spectron.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    private TextDocument _document = new();

    public LogViewModel(ILogStore logStore)
    {
        _document.Text = string.Join(Environment.NewLine, logStore.Entries);
    }
}