using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public sealed class Interface1Device(Z80 cpu, IEmulatorMemory emulatorMemory) : IDevice
{
    public readonly ShadowRom ShadowRom = new(emulatorMemory, RomVersion.V2);

    internal bool IsEnabled { get; private set; }

    public void Enable()
    {
        IsEnabled = true;

        cpu.BeforeFetch += BeforeFetch;
        cpu.AfterFetch += AfterFetch;
    }

    public void Disable()
    {
        IsEnabled = false;

        ShadowRom.UnPage();

        cpu.BeforeFetch -= BeforeFetch;
        cpu.AfterFetch -= AfterFetch;
    }

    public void Reset() => ShadowRom.UnPage();

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsControlPort(address) || IsMicrodrivePort(address) || IsNetworkPort(address))
        {
            //Console.WriteLine($"Writing port {address:X4}: {value}");
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (IsControlPort(address) || IsMicrodrivePort(address) || IsNetworkPort(address))
        {
            Console.WriteLine($"Reading port {address:X4}");
        }

        return null;
    }

    private void BeforeFetch(Word pc)
    {
        // Paging happens before the opcode
        if (pc is 0x0008 or 0x1708)
        {
            ShadowRom.Page();
        }
    }

    private void AfterFetch(Word pc)
    {
        // UnPaging happens after the opcode
        if (pc == 0x0700 + 1)
        {
            ShadowRom.UnPage();
        }
    }

    private static bool IsControlPort(Word address) => (address & 0x18) == 0x08;
    private static bool IsMicrodrivePort(Word address) => (address & 0x18) == 0x00;
    private static bool IsNetworkPort(Word address) => (address & 0x18) == 0x10;
}