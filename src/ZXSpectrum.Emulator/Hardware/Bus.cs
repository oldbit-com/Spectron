using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Extensions;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Bus(Beeper beeper, ScreenRenderer renderer, Clock clock) : IBus
{
    private readonly List<IInputDevice> _inputDevices = [];

    internal void AddDevice(IInputDevice device) => _inputDevices.Add(device);

    public byte Read(Word address)
    {
        foreach (var inputDevice in _inputDevices)
        {
            var result = inputDevice.Read(address);
            if (result != null)
            {
                return result.Value;
            }
        }

        // TODO: Floating bus
        return 0xFF;
    }

    public void Write(Word address, byte data)
    {
        if (IsUlaPort(address))
        {
            var borderColor = Colors.BorderColors[(byte)(data & 0x07)];
            renderer.UpdateBorder(borderColor, clock.FrameTicks);

            beeper.UpdateBeeper(data, clock.TotalTicks);
        }
    }

    private static bool IsUlaPort(Word address) => (address & 0x01) == 0x00;
}