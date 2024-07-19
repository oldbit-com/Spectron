using OldBit.Z80Cpu;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Ula(Keyboard keyboard, Beeper beeper, ScreenRenderer renderer, Clock clock) : IInputDevice, IOutputDevice
{
    private const byte EarBit = 0x10;

    public byte? Read(Word address)
    {
        if (IsUlaPort(address))
        {
            return keyboard.Read(address);
        }

        // TODO: Floating bus handling
        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        if (!IsUlaPort(address))
        {
            return;
        }

        var color = Colors.BorderColors[(byte)(data & 0x07)];
        renderer.UpdateBorder(color, clock.FrameTicks);

        beeper.UpdateBeeper(data, clock.TotalTicks);

        // var beeperState = data & EarBit >> 4;
        // beeper.UpdateBeeper1(beeperState, clock.FrameTicks);
    }

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}