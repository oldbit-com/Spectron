using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public sealed class Interface1Device(Z80 cpu, IEmulatorMemory emulatorMemory) : IDevice
{
    private const int DataPort = 0xE7;
    private const int ControlPort = 0xEF;

    internal readonly ShadowRom ShadowRom = new(emulatorMemory, RomVersion.V2);

    internal bool IsEnabled { get; private set; }

    public void Enable()
    {
        IsEnabled = true;

        cpu.BeforeFetch += BeforeFetch;
    }

    public void Disable()
    {
        IsEnabled = false;

        cpu.BeforeFetch -= BeforeFetch;
    }


    public void WritePort(Word address, byte value)
    {

    }

    public byte? ReadPort(Word address) => null;

    private void BeforeFetch(Word pc)
    {
        switch (pc)
        {
            case 0x0008 or 0x1708:
                ShadowRom.Page();
                break;

            case 0x0700:
                ShadowRom.Unpage();
                break;
        }
    }
}