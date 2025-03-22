using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulation.Devices.Storage;

public class DivMmcMemory : IRomMemory
{
    private readonly IEmulatorMemory _emulatorMemory;

    private const int BankCount = 16;
    private const int BankSize = 0x2000;

    private const byte CONMEM = 0x80;
    private const byte MAPRAM = 0x40;

    private readonly byte[] _eeprom;
    private readonly byte[][] _banks;

    private byte _control;
    private byte[] _bank0;      // 0000h-1FFFh
    private byte[] _bank1;      // 2000h-3FFFh

    public bool IsWriteEnabled { get; set; }

    internal DivMmcMemory(IEmulatorMemory emulatorMemory, byte[] eeprom)
    {
        _emulatorMemory = emulatorMemory;
        _eeprom = eeprom;

        _banks = new byte[BankCount][];

        for (var i = 0; i < BankCount; i++)
        {
            _banks[i] = new byte[BankSize];
        }

        _bank0 = _eeprom;
        _bank1 = _banks[0];
    }

    internal void AutoPage(bool isEnabled)
    {
        if (IsWriteEnabled)
        {
            return;
        }

        _emulatorMemory.ShadowRom(isEnabled ? this : null);
    }

    internal void Control(byte control)
    {
        // Preserve MAPRAM bit once set, only hard reset can clear it
        var mapram = _control & MAPRAM;
        _control = (byte)(control | mapram);

        var selectedBank = _control & 0x0F;

        switch (_control & (CONMEM | MAPRAM))
        {
            case 0:
                _bank0 = (_control & MAPRAM) == MAPRAM ? _banks[3] : _eeprom;
                IsBank1Writable = true;

                break;

            case MAPRAM:
                _bank0 = _banks[3];
                IsBank1Writable = selectedBank != 3;

                break;

            case CONMEM:
                _bank0 = _eeprom;
                IsBank1Writable = true;

                break;
        }

        _bank1 = _banks[selectedBank];

        if ((_control & CONMEM) == CONMEM)
        {
            _emulatorMemory.ShadowRom(this);
        }
    }

    public byte Read(Word address) => address switch
    {
        < 0x2000 => _bank0[address],
        < 0x4000 => _bank1[address - 0x2000],
        _ => 0xFF
    };

    public void Write(Word address, byte data)
    {
        switch (address)
        {
            case < 0x2000:
            {
                if (IsBank0Writable)
                {
                    _bank0[address] = data;
                }

                break;
            }
            case < 0x4000:
            {
                if (IsBank1Writable)
                {
                    _bank1[address - 0x2000] = data;
                }

                break;
            }
        }
    }

    public byte[] Memory => _bank0.Concatenate(_bank1);

    private bool IsBank0Writable => ReferenceEquals(_bank0, _eeprom) && IsWriteEnabled;

    private bool IsBank1Writable { get; set; } = true;
}