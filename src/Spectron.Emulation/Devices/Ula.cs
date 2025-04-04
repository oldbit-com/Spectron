using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices;

internal sealed class Ula(
    KeyboardState keyboardState,
    ScreenBuffer screenBuffer,
    Clock clock,
    CassettePlayer? cassettePlayer) : IDevice
{
    public byte? ReadPort(Word address)
    {
        if (!IsUlaPort(address))
        {
            return null;
        }

        var value = keyboardState.Read(address);
        UpdateEarBit(ref value);

        return value;
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

    private void UpdateEarBit(ref byte value)
    {
        if (cassettePlayer is not { IsPlaying: true })
        {
            value = (byte)(value & 0xBF);
            return;
        }

        value = cassettePlayer.EarBit ? (byte)(value | 0x40) : (byte)(value & 0xBF);
    }
}