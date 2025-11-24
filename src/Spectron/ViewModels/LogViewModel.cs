using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Logging;

namespace OldBit.Spectron.ViewModels;

public partial class LogViewModel : ObservableObject
{
    public LogViewModel(ILogStore  logStore)
    {

    }
}