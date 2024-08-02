using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;

namespace OldBit.Spectral.Dialogs;

public static class MessageDialogs
{
    public static Window MainWindow { get; set; } = null!;

    public static async Task Error(string message, string title = "Error")
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            windowStartupLocation: WindowStartupLocation.CenterOwner);

        await messageBox.ShowWindowDialogAsync(MainWindow);
    }
}