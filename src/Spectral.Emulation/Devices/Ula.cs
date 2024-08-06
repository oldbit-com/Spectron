using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Keyboard;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices;

internal class Ula(
    EmulatorMemory memory,
    KeyHandler keyHandler,
    Beeper beeper,
    ScreenRenderer renderer,
    Clock clock,
    TapePlayer tapePlayer) : IDevice
{
    private readonly FloatingBus _floatingBus = new(memory);

    public byte? ReadPort(Word address)
    {
        if (!IsUlaPort(address))
        {
            return _floatingBus.GetFloatingValue(clock.FrameTicks);
        }

        var value = keyHandler.Read(address);
        UpdateEarBit(ref value);

        return value;
    }

    public void WritePort(Word address, byte value)
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