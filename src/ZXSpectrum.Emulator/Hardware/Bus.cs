using OldBit.Z80.Net;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Bus(
    Keyboard keyboard,
    Beeper beeper,
    Memory48 memory,
    Border border,
    CyclesCounter cyclesCounter) : IBus
{
    private const byte KeyboardPort = 0xFE;

    private readonly Keyboard _keyboard = keyboard;
    private readonly Beeper _beeper = beeper;
    private readonly Memory48 _memory = memory;
    private readonly Border _border = border;
    private readonly CyclesCounter _cyclesCounter = cyclesCounter;

    public byte Read(Word address)
    {
        if (IsKeyboardPort(address))
        {

        }

        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        if (IsUlaPort(address))
        {
            //_border.ChangeBorderColor(data, _cyclesCounter.CurrentCycles);
        }
    }

    private static bool IsKeyboardPort(Word address) => (address & 0xFF) == KeyboardPort;

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}