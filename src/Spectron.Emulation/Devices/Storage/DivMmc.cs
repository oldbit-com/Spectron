using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Emulation.Devices.Storage;

internal class DivMmc : IDevice
{
    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte MMC0 = 0b10;
    private const byte MMC1 = 0b01;

    private MmcCard? ActiveCard { get; set; }
    private MmcCard? InsertedCard { get; set; }

    public bool IsEnabled { get; set; }
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
        Memory = new DivMmcMemory(emulatorMemory, new byte[0x2000]);

        cpu.BeforeInstruction += CpuOnBeforeInstruction;
    }

    private void CpuOnBeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (e.PC is 0x0000 or 0x0008 or 0x0038 or 0x0066 or 0x04C6 or 0x0562 || (e.PC & 0xFF00) == 0x3D00)
        {
            Memory.AutoPage(isEnabled: true);
        }
        else if ((e.PC & 0xFFFF8) == 0x1FF8)
        {
            Memory.AutoPage(isEnabled: false);
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
            Memory.PageMemory(value);
        }

        if ((address & 0xFF) == CardSelectRegister)
        {

        }

        if ((address & 0xFF) == DataRegister)
        {

        }
    }

    public byte? ReadPort(Word address)
    {
        return null;
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