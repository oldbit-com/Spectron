using OldBit.Spectral.Emulator.Hardware.Audio;
using OldBit.Spectral.Emulator.Screen;
using OldBit.Spectral.Emulator.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Hardware;

internal class Ula(
    Memory memory,
    Keyboard keyboard,
    Beeper beeper,
    ScreenRenderer renderer,
    Clock clock,
    TapePlayer tapePlayer) : IInputDevice, IOutputDevice
{
    private readonly FloatingBus _floatingBus = new(memory);

    public byte? Read(Word address)
    {
        if (!IsUlaPort(address))
        {
            // TODO: Floating bus handling
            _floatingBus.GetFloatingValue(clock.FrameTicks);
            return 0xFF;
        }

        var value = keyboard.Read(address);
        UpdateEarBit(ref value);

        return value;
    }

    public void Write(Word address, byte value)
    {
        if (!IsUlaPort(address))
        {
            return;
        }

        var color = Colors.BorderColors[(byte)(value & 0x07)];
        renderer.UpdateBorder(color, clock.FrameTicks);

        UpdateTapeLoadingBeeper(ref value);
        beeper.UpdateBeeper(value, clock.TotalTicks);
    }

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;

    private void UpdateEarBit(ref byte value)
    {
        if (!tapePlayer.IsPlaying)
        {
            return;
        }

        value = tapePlayer.EarBit ? (byte)(value | 0x40) : (byte)(value & 0xBF);
    }

    private void UpdateTapeLoadingBeeper(ref byte value)
    {
        if (!tapePlayer.IsPlaying)
        {
            return;
        }

        value = tapePlayer.EarBit ? (byte)(value | 0x10) : (byte)(value & 0xEF);
    }
}