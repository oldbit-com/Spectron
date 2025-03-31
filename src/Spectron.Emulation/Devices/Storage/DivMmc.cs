using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage.SD;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Storage;

public class DivMmc : IDevice
{
    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte MMC0 = 0b10;
    private const byte MMC1 = 0b01;

    private readonly Z80 _cpu;
    private readonly CardDevice _cardDevice = new();

    private bool _isEnabled;
    private PagingMode _delayedPagingMode = PagingMode.None;

    public DivMmcMemory Memory { get; }

    internal DivMmc(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _cpu = cpu;

        var rom = RomReader.ReadRom(RomType.DivMmc);

        Memory = new DivMmcMemory(emulatorMemory, rom);
    }

    public void Enable()
    {
        _isEnabled = true;

        _cpu.BeforeFetch += BeforeFetch;
        _cpu.AfterFetch += AfterFetch;
    }

    public void Disable()
    {
        _isEnabled = false;

        _cpu.BeforeFetch -= BeforeFetch;
        _cpu.AfterFetch -= AfterFetch;
    }

    public void WritePort(Word address, byte value)
    {
        if (!_isEnabled)
        {
            return;
        }

        if ((address & 0xFF) == ControlRegister)
        {
            Memory.Control(value);
        }

        if ((address & 0xFF) == CardSelectRegister)
        {
            _cardDevice.ChipSelect(value);
            Console.WriteLine($"Selecting active card: {value:X2}");
        }

        if ((address & 0xFF) == DataRegister)
        {
            Console.WriteLine($"Write DivMMC data register: {value:X2}");
            _cardDevice.Write(value);
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!_isEnabled)
        {
            return null;
        }

        if ((address & 0xFF) == DataRegister)
        {
            Console.WriteLine("Read DivMMC data register");
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

    private void SelectActiveCard(byte value)
    {
        // WR Only = 2 bit chip select register (D0 = MMC0; D1 = MMC1), active LOW
        switch (value & 0x03)
        {
            case MMC0:
                break;

            case MMC1:
                break;
        }
    }
}