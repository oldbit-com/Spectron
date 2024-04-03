using Avalonia.Media.Imaging;

namespace OldBit.ZXSpectrum.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    //private readonly
    public string Greeting => "Welcome to Avalonia!";

    public Bitmap Screen { get; set; }
}