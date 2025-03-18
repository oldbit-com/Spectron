namespace OldBit.Spectron.Emulation.Devices.Div;

public class DivMmc : IDevice
{
    private const int ControlRegister = 0xE3;
    private const int CardSelectRegister = 0xE7;
    private const int DataRegister = 0xEB;

    private const byte CONMEM = 0x80;
    private const byte MAPRAM = 0x40;

    private const byte MMC0 = 0b10;
    private const byte MMC1 = 0b01;

    private MmcCard? ActiveCard { get; set; }
    private MmcCard? InsertedCard { get; set; }

    public bool IsEnabled { get; set; }

    // E7 EB
    // SPI_PORT       equ $EB   SPI /DATA
    // OUT_PORT      equ $E7  SPI /CS         ; port for CS control (D1:D0)  SPI /CS (sd card, flash, rpi)


    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if ((address & 0xFF) == ControlRegister)
        {

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