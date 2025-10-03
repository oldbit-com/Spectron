using OldBit.Spectron.Emulation.Devices.Beta128.Controller;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public class Beta128Device(Z80 cpu, float clockMhz, IEmulatorMemory emulatorMemory) : IDevice
{
    private readonly DiskController _controller = new(clockMhz);

    private bool _isPaged;

    private long _ticksSinceReset;
    private long Now => _ticksSinceReset + cpu.Clock.FrameTicks;

    public readonly ShadowRom ShadowRom = new(emulatorMemory);

    public bool IsEnabled { get; private set; }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !_isPaged)
        {
            return;
        }

        var portType = GetPortType(address);

        if (portType == PortType.None)
        {
            return;
        }

        _controller.ProcessState(Now);

        switch (portType)
        {
            case PortType.Command:
                _controller.ProcessCommand(Now, value);
                break;

            case PortType.Track:
                _controller.TrackRegister = value;
                break;

            case PortType.Sector:
                _controller.SectorRegister = value;
                break;

            case PortType.Data:
                _controller.DataRegister = value;
                break;

            case PortType.Control:
                _controller.ControlRegister = value;
                break;
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || !_isPaged)
        {
            return null;
        }

        var portType = GetPortType(address);

        if (portType == PortType.None)
        {
            return null;
        }

        _controller.ProcessState(Now);

        return portType switch
        {
            PortType.Status => _controller.Status,
            PortType.Track => _controller.TrackRegister,
            PortType.Sector => _controller.SectorRegister,
            PortType.Data => _controller.DataRegister,
            PortType.Control => (byte)(_controller.Request | ~(RequestStatus.InterruptRequest | RequestStatus.DataRequest)),
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
            case >= 0x3D00 and <= 0x3DFF:
                if (!_isPaged)
                {
                    ShadowRom.Page();
                    _isPaged = true;
                }
                break;

            case >= 0x4000:
                if (_isPaged)
                {
                    ShadowRom.UnPage();
                    _isPaged = false;
                }
                break;
        }
    }

    internal void NewFrame(long ticksSinceReset) => _ticksSinceReset  = ticksSinceReset;

    private static PortType GetPortType(Word address) => (address & 0xFF) switch
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