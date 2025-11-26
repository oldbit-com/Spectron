using System.Text;
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
        var text = new StringBuilder();

        foreach (var entry in logStore.Entries)
        {
            text.Append(entry.Level.ToString());
            text.Append(": ");
            text.AppendLine(entry.Message);
        }

        _document.Text = text.ToString();
    }
}