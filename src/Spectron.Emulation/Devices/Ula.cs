using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices;

internal sealed class Ula(
    KeyboardHandler keyboardHandler,
    Beeper beeper,
    ScreenBuffer screenBuffer,
    Clock clock,
    CassettePlayer? tapePlayer) : IDevice
{
    public byte? ReadPort(Word address)
    {
        if (!IsUlaPort(address))
        {
            return null;
        }

        var value = keyboardHandler.Read(address);
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

        UpdateTapeLoadingBeeper(ref value);
        beeper.Update(clock.FrameTicks, value);
    }

    internal static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;

    private void UpdateEarBit(ref byte value)
    {
        if (tapePlayer is not { IsPlaying: true })
        {
            return;
        }

        value = tapePlayer.EarBit ? (byte)(value | 0x40) : (byte)(value & 0xBF);
    }

    private void UpdateTapeLoadingBeeper(ref byte value)
    {
        if (tapePlayer is not { IsPlaying: true })
        {
            return;
        }

        value = tapePlayer.EarBit ? (byte)(value | 0x10) : (byte)(value & 0xEF);
    }
}