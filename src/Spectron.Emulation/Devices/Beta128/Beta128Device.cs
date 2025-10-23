using OldBit.Spectron.Emulation.Devices.Beta128.Controller;
using OldBit.Spectron.Emulation.Devices.Beta128.Events;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public class Beta128Device(
    Z80 cpu,
    float clockMhz,
    IEmulatorMemory emulatorMemory,
    ComputerType computerType,
    IDiskDriveProvider diskDriveProvider) : IDevice
{
    private long _ticksSinceReset;
    private long Now => _ticksSinceReset + cpu.Clock.FrameTicks;

    private readonly ShadowRom _shadowRom = new(emulatorMemory);

    public bool IsEnabled { get; private set; }
    internal bool IsRomPaged { get; private set; }
    internal DiskController Controller { get; } = new(clockMhz, diskDriveProvider);

    internal event DiskActivityEvent? DiskActivity;

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !IsRomPaged)
        {
            return;
        }

        var portType = GetPortType(address);

        if (portType == PortType.None)
        {
            return;
        }

        Controller.ProcessState(Now);

        switch (portType)
        {
            case PortType.Command:
                Controller.ProcessCommand(Now, value);
                break;

            case PortType.Track:
                Controller.TrackRegister = value;
                break;

            case PortType.Sector:
                Controller.SectorRegister = value;
                break;

            case PortType.Data:
                Controller.DataRegister = value;
                break;

            case PortType.Control:
                Controller.ControlRegister = value;
                break;
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || !IsRomPaged)
        {
            return null;
        }

        var portType = GetPortType(address);

        if (portType == PortType.None)
        {
            return null;
        }

        Controller.ProcessState(Now);

        if (!Controller.IsIdle)
        {
            DiskActivity?.Invoke(EventArgs.Empty);
        }

        return portType switch
        {
            PortType.Status => Controller.StatusRegister,
            PortType.Track => Controller.TrackRegister,
            PortType.Sector => Controller.SectorRegister,
            PortType.Data => Controller.DataRegister,
            PortType.Control => Controller.ControlRegister,
            _ => null
        };
    }

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

    private void BeforeFetch(Word pc)
    {
        switch (pc)
        {
            case >= 0x3D00 and <= 0x3DFF when !IsRomPaged && CanPageRom:
                PageRom();
                break;

            case >= 0x4000 when IsRomPaged:
                UnPageRom();
                break;
        }
    }

    internal void PageRom()
    {
        _shadowRom.Page();
        IsRomPaged = true;
    }

    private void UnPageRom()
    {
        _shadowRom.UnPage();
        IsRomPaged = false;
    }

    internal void NewFrame(long ticksSinceReset) => _ticksSinceReset  = ticksSinceReset;

    private bool CanPageRom =>
        (computerType == ComputerType.Spectrum128K && emulatorMemory.RomBank == RomBank.Bank1 ||
         computerType == ComputerType.Spectrum48K);

    private static byte GetPortType(Word address) => (address & 0xFF) switch
    {
        0x1F => PortType.Command,
        0x3F => PortType.Track,
        0x5F => PortType.Sector,
        0x7F => PortType.Data,
        0xFF => PortType.Control,
        _ => PortType.None
    };

    internal void Reset()
    {
        _ticksSinceReset = 0;
        // TODO: Any reset logic
    }
}