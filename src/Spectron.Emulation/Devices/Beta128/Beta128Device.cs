using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public class Beta128Device(Z80 cpu, IEmulatorMemory emulatorMemory) : IDevice
{
    private const byte CommandRegister = 0x1F;
    private const byte StatusRegister = 0x1F;
    private const byte TrackRegister = 0x3F;
    private const byte SectorRegister = 0x5F;
    private const byte DataRegister = 0x7F;
    private const byte ControlRegister = 0xFF;

    private bool _isPaged;

    public readonly ShadowRom ShadowRom = new(emulatorMemory);

    public bool IsEnabled { get; private set; }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !_isPaged)
        {
            return;
        }

        var register = address & 0xFF;

        switch (register)
        {
            case CommandRegister:
                break;

            case TrackRegister:
                break;

            case SectorRegister:
                break;

            case DataRegister:
                break;

            case ControlRegister:
                break;
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || !_isPaged)
        {
            return null;
        }

        var register = address & 0xFF;

        switch (register)
        {
            case StatusRegister:
                break;

            case TrackRegister:
                break;

            case SectorRegister:
                break;

            case DataRegister:
                break;
        }

        return null;
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

    public void Reset()
    {
        // TODO: Any reset logic
    }
}