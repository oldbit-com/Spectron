using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Extensions;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Bus(Keyboard keyboard, Beeper beeper, ScreenRenderer renderer, Clock clock) : IBus
{
    public byte Read(Word address)
    {
        var (hiAddress, loAddress) = address;

        if (IsKeyboardPort(loAddress))
        {
            return keyboard.GetKeyState(hiAddress);
        }

        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        if (IsUlaPort(address))
        {
            var borderColor = Colors.BorderColors[(byte)(data & 0x07)];
            renderer.UpdateBorder(borderColor, clock.FrameTicks);

            //border.ChangeBorderColor(data, clock.FrameTicks);

            beeper.UpdateBeeper(data, clock.TotalTicks);
        }
    }

    private static bool IsKeyboardPort(byte address) => address == 0xFE;

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}