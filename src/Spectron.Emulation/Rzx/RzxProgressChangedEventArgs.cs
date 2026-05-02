namespace OldBit.Spectron.Emulation.Rzx;

public class RzxProgressChangedEventArgs(double progress)
{
    public double Progress { get; } = progress;
}