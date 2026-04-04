using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Timex;

internal sealed  class UlaTimex(
    KeyboardState keyboardState,
    ScreenBuffer screenBuffer,
    Z80 cpu,
    TapeManager tapeManager) : Ula(keyboardState, screenBuffer, cpu, tapeManager)
{
    internal override bool IsUlaPort(Word address) => (address & 0xFF) == 0x00FE;
}