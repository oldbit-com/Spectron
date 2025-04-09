using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.DivMmc.SD;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.DivMmc;

public sealed class DivMmcDevice : IDevice, IDisposable
{
    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte Sd0 = 0b10;  // negated, bit 0 == 0, selected card 0
    private const byte Sd1 = 0b01;  // negated, bit 1 == 0, selected card 1

    private readonly Z80 _cpu;
    private readonly ILogger _logger;
    private readonly CardDevice _cardDevice = new();

    private PagingMode _afterFetchPagingMode = PagingMode.None;

    private DiskImage? _diskImage;

    internal bool IsEnabled { get; private set; }

    public DivMmcMemory Memory { get; }

    public bool IsDriveWriteEnabled
    {
        set
        {
            if (_diskImage != null)
            {
                _diskImage.IsWriteEnabled = value;
            }
        }
    }

    internal DivMmcDevice(Z80 cpu, IEmulatorMemory emulatorMemory, ILogger logger)
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

    public void InsertCard(string fileName)
    {
        _diskImage?.Dispose();
        _diskImage = null;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        try
        {
            _diskImage = new DiskImage(fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sd card: {Filename}",  fileName);
        }
    }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsControlRegister(address))
        {
            Memory.PagingControl(value);
        }
        else if (IsCardSelectRegister(address))
        {
            if ((value & 0x03) == 0x03)
            {
                _cardDevice.Eject();
            }
            else if ((value & Sd0) == Sd0)
            {
                if (_diskImage != null)
                {
                    _cardDevice.Insert(_diskImage);
                }
            }
            else if ((value & Sd1) == Sd1)
            {
                // TODO: Support multiple cards
                _cardDevice.Eject();
                //_cardDevice.InsertCard(_sdCard);
            }
        }
        else if (IsDataRegister(address))
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

        if (IsDataRegister(address))
        {
            return _cardDevice.Read();
        }

        return null;
    }

    public void Stop() => Dispose();

    private void BeforeFetch(Word pc)
    {
        _afterFetchPagingMode = PagingMode.None;

        if (pc is 0x0000 or 0x0008 or 0x0038 or 0x0066 or 0x04C6 or 0x0562)
        {
            _afterFetchPagingMode = PagingMode.On;
        }
        else if ((pc & 0xFF00) == 0x3D00)
        {
            Memory.Paging(PagingMode.On);
        }
        else if ((pc & 0xFFFF8) == 0x1FF8)
        {
            _afterFetchPagingMode = PagingMode.Off;
        }
    }

    private void AfterFetch(Word pc)
    {
        if (_afterFetchPagingMode == PagingMode.None)
        {
            return;
        }

        Memory.Paging(_afterFetchPagingMode);
    }

    private static bool IsDataRegister(Word address) => (address & 0xFF) == DataRegister;

    private static bool IsControlRegister(Word address) => (address & 0xFF) == ControlRegister;

    private static bool IsCardSelectRegister(Word address) => (address & 0xFF) == CardSelectRegister;

    public void Dispose() => _diskImage?.Dispose();
}