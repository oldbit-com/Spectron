using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Integration.Tests.Fixtures;

public static class EmulatorExtensions
{
    public static void RunFrames(this Emulator emulator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            emulator.RunFrame();
        }
    }
}