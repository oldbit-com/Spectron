namespace OldBit.Spectron.Emulator.Tests.Fixtures;

public static class Repeat
{
    public static void Run(int count, Action action)
    {
        for (var i = 0; i < count; i++)
        {
            action();
        }
    }
}