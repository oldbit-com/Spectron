using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage.SD;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Emulation.Devices.Storage;

public class DivMmc : IDevice
{
    private readonly Z80 _cpu;
    private readonly CardDevice _cardDevice = new CardDevice();

    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte MMC0 = 0b10;
    private const byte MMC1 = 0b01;

    private bool _isEnabled;
    private MmcCard? ActiveCard { get; set; }
    private MmcCard? InsertedCard { get; set; }

    public DivMmcMemory Memory { get; }

    // E7 EB
    // SPI_PORT       equ $EB   SPI /DATA
    // OUT_PORT      equ $E7  SPI /CS         ; port for CS control (D1:D0)  SPI /CS (sd card, flash, rpi)

    // 128

    // Automatic mapping occurs at the begining of refresh cycle after fetching an
    // opcode (after the M1 cycle) from 0000h, 0008h, 0038h, 0066h, 04C6h and 0562h.
    // Mapping also occurs instantly when executing an opcode at 3D00h-3DFFh, 100 ns
    // after the /MREQ falling edge.

    internal DivMmc(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _cpu = cpu;

        var rom = RomReader.ReadRom(RomType.DivMmc);

        Memory = new DivMmcMemory(emulatorMemory, rom);
    }

    public void Enable()
    {
        _isEnabled = true;
        //_cpu.BeforeInstruction += CpuOnBeforeInstruction;
        _cpu.BeforeFetch += CpuOnBeforeOpCodeFetch;
        _cpu.AfterFetch += CpuOnAfterOpCodeFetch;
    }

    public void Disable()
    {
        _isEnabled = false;
        //_cpu.BeforeInstruction -= CpuOnBeforeInstruction;
        _cpu.BeforeFetch -= CpuOnBeforeOpCodeFetch;
        _cpu.AfterFetch -= CpuOnAfterOpCodeFetch;
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
            //Console.WriteLine($"Selecting active card: {value}");
        }

        if ((address & 0xFF) == DataRegister)
        {
            //Console.WriteLine($"Write DivMMC data register: {value}");
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
            //Console.WriteLine("Read DivMMC data register");
            return _cardDevice.Read();
        }

        return null;
    }

    private enum AutoPaging
    {
        None,
        Enable,
        Disable
    }

    private AutoPaging _autoPaging = AutoPaging.None;

    private void CpuOnBeforeOpCodeFetch(Word pc)
    {
        if (pc is 0x0000 or 0x0008 or 0x0038 or 0x0066 or 0x04C6 or 0x0562)
        {
            _autoPaging = AutoPaging.Enable;
        }
        else if ((pc & 0xFF00) == 0x3D00)
        {
            Memory.AutoPage(isEnabled: true);
        }
        else if ((pc & 0xFFFF8) == 0x1FF8)
        {
            _autoPaging = AutoPaging.Disable;
        }
        else
        {
            _autoPaging = AutoPaging.None;
        }
    }

    private void CpuOnAfterOpCodeFetch(Word pc)
    {
        if (_autoPaging == AutoPaging.Enable)
        {
            Memory.AutoPage(isEnabled: true);
        }

        if (_autoPaging == AutoPaging.Disable)
        {
            Memory.AutoPage(isEnabled: false);
        }
    }

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