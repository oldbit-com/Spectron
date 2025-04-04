using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage.SD;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Storage;

public sealed class DivMmc : IDevice
{
    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte Sd0 = 0b10;  // negated, bit 0 == 0, selected card 0
    private const byte Sd1 = 0b01;  // negated, bit 1 == 0, selected card 1

    private readonly Z80 _cpu;
    private readonly ILogger _logger;
    private readonly CardDevice _cardDevice = new();

    private PagingMode _delayedPagingMode = PagingMode.None;

    private SdCard? _sdCard;

    internal bool IsEnabled { get; private set; }

    public DivMmcMemory Memory { get; }

    internal DivMmc(Z80 cpu, IEmulatorMemory emulatorMemory, ILogger logger)
    {
        _cpu = cpu;
        _logger = logger;

        var rom = RomReader.ReadRom(RomType.DivMmc);

        Memory = new DivMmcMemory(emulatorMemory, rom);
    }

    public void Enable()
    {
        IsEnabled = true;

        _cpu.BeforeFetch += BeforeFetch;
        _cpu.AfterFetch += AfterFetch;
    }

    public void Disable()
    {
        IsEnabled = false;

        _cpu.BeforeFetch -= BeforeFetch;
        _cpu.AfterFetch -= AfterFetch;
    }

    public void InsertCard(string filename)
    {
        _sdCard?.Dispose();
        _sdCard = null;

        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        try
        {
            _sdCard = new SdCard(filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sd card: {Filename}",  filename);
        }
    }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if ((address & 0xFF) == ControlRegister)
        {
            Memory.PagingControl(value);
        }

        if ((address & 0xFF) == CardSelectRegister)
        {
            if ((value & 0x03) == 0x03)
            {
                _cardDevice.RemoveCard();
            }
            else if ((value & Sd0) == Sd0)
            {
                if (_sdCard != null)
                {
                    _cardDevice.InsertCard(_sdCard);
                }
            }
            else if ((value & Sd1) == Sd1)
            {
                // TODO: Support multiple cards
                _cardDevice.RemoveCard();
                //_cardDevice.InsertCard(_sdCard);
            }
        }

        if ((address & 0xFF) == DataRegister)
        {
            _cardDevice.Write(value);
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if ((address & 0xFF) == DataRegister)
        {
            return _cardDevice.Read();
        }

        return null;
    }

    private void BeforeFetch(Word pc)
    {
        _delayedPagingMode = PagingMode.None;

        if (pc is 0x0000 or 0x0008 or 0x0038 or 0x0066 or 0x04C6 or 0x0562)
        {
            _delayedPagingMode = PagingMode.On;
        }
        else if ((pc & 0xFF00) == 0x3D00)
        {
            Memory.Paging(PagingMode.On);
        }
        else if ((pc & 0xFFFF8) == 0x1FF8)
        {
            _delayedPagingMode = PagingMode.Off;
        }
    }

    private void AfterFetch(Word pc) => Memory.Paging(_delayedPagingMode);
}