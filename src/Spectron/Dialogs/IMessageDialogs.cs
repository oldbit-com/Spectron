using System.Threading.Tasks;

namespace OldBit.Spectron.Dialogs;

public interface IMessageDialogs
{
    Task Error(string message, string title = "Error");
    Task Warning(string message, string title = "Warning");
}