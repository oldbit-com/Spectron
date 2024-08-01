using ReactiveUI;

namespace OldBit.Spectral.ViewModels;

public class StatusBarViewModel : ViewModelBase
{
    private string _framesPerSecond = "FPS: 0";
    public string FramesPerSecond
    {
        get => _framesPerSecond;
        set => this.RaiseAndSetIfChanged(ref _framesPerSecond, value);
    }
}