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
    private readonly CardDevice _cardDevice0 = new();
    private readonly CardDevice _cardDevice1 = new();

    private CardDevice _activeCardDevice;
    private PagingMode _afterFetchPagingMode = PagingMode.None;
    private bool _isDriveWriteEnabled;

    private DiskImage? _diskImage0;
    private DiskImage? _diskImage1;

    public bool IsEnabled { get; private set; }

    public DivMmcMemory Memory { get; }

    public bool IsDriveWriteEnabled
    {
        set
        {
            _isDriveWriteEnabled = value;

            if (_diskImage0 != null)
            {
                _diskImage0.IsWriteEnabled = value;
            }
            if (_diskImage1 != null)
            {
                _diskImage1.IsWriteEnabled = value;
            }
        }
    }

    internal DivMmcDevice(Z80 cpu, IEmulatorMemory emulatorMemory, ILogger logger)
    {
        _cpu = cpu;
        _logger = logger;
        _activeCardDevice = _cardDevice0;

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

    public void InsertCard(string fileName, int slotNumber)
    {
        switch (slotNumber)
        {
            case 0:
                _diskImage0?.Dispose();
                _diskImage0 = null;
                break;

            case 1:
                _diskImage1?.Dispose();
                _diskImage1 = null;
                break;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        try
        {
            var diskImage = new DiskImage(fileName);
            diskImage.IsWriteEnabled = _isDriveWriteEnabled;

            switch (slotNumber)
            {
                case 0:
                    _diskImage0 = diskImage;
                    break;

                case 1:
                    _diskImage1 = diskImage;
                    break;
            }
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
                _cardDevice0.Eject();
                _cardDevice1.Eject();
            }
            else if ((value & Sd0) == Sd0)
            {
                _activeCardDevice = _cardDevice0;

                if (_diskImage0 != null)
                {
                    _activeCardDevice.Insert(_diskImage0);
                }
            }
            else if ((value & Sd1) == Sd1)
            {
                _activeCardDevice = _cardDevice1;

                if (_diskImage1 != null)
                {
                    _activeCardDevice.Insert(_diskImage1);
                }
            }
        }
        else if (IsDataRegister(address))
        {
            _activeCardDevice.Write(value);
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
            return _activeCardDevice.Read();
        }

        return null;
    }

    public void Stop() => Dispose();

    public void Reset()
    {
        _cardDevice0.Reset();
        _cardDevice1.Reset();
    }

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

    public void Dispose()
    {
        _diskImage0?.Dispose();
        _diskImage1?.Dispose();
    }
}