using OldBit.Z80Cpu;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Emulator.Tape;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

internal class Ula(Keyboard keyboard, Beeper beeper, ScreenRenderer renderer, Clock clock, TapePlayer tapePlayer) : IInputDevice, IOutputDevice
{
    public byte? Read(Word address)
    {
        if (!IsUlaPort(address))
        {
            // TODO: Floating bus handling
            return 0xFF;
        }

        var value = keyboard.Read(address);

        value = UpdateEarBit(value);

        return value;
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

       // Console.WriteLine($"ULA: {data:X2} CLOCK: {clock.TotalTicks}");

        // var beeperState = data & EarBit >> 4;
        // beeper.UpdateBeeper1(beeperState, clock.FrameTicks);
    }

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;

    private byte UpdateEarBit(byte value)
    {
        if (!tapePlayer.IsPlaying)
        {
            return value;
        }

        return tapePlayer.EarBit ? (byte)(value | 0x40) : (byte)(value & 0xBF);
    }
}