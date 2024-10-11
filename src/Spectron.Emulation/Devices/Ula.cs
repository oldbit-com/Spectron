using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices;

internal sealed class Ula(
    KeyboardHandler keyboardHandler,
    ScreenBuffer screenBuffer,
    Clock clock) : IDevice
{
    public byte? ReadPort(Word address)
    {
        if (!IsUlaPort(address))
        {
            return null;
        }

        return keyboardHandler.Read(address);
    }

    public void WritePort(Word address, byte value)
    {
        if (!IsUlaPort(address))
        {
            return;
        }

        var color = SpectrumPalette.GetBorderColor(value);
        screenBuffer.UpdateBorder(color, clock.FrameTicks);
    }

    internal static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}