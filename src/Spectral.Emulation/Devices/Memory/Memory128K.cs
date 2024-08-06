namespace OldBit.Spectral.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 128k.
/// </summary>
internal sealed class Memory128K : EmulatorMemory
{
    private const byte RomSelectBit = 0b00010000;
    private const byte PagingDisableBit = 0b00100000;
    private const byte ScreenSelectBit = 0b00001000;
    private const byte RamBankMask = 0b00000111;

    private readonly byte[] _rom48;
    private readonly byte[] _rom128;
    private readonly byte[][] _banks = new byte[8][];

    private byte[] _activeScreen;
    private byte[] _activeRom;
    private byte[] _activeRam;
    private bool _isPagingDisabledUntilReset;

    public Memory128K(byte[] rom128, byte[] rom48)
    {
        if (rom128.Length != 16384)
        {
            throw new ArgumentException("ROM must be exactly 16KB in size.", nameof(rom128));
        }

        if (rom48.Length != 16384)
        {
            throw new ArgumentException("ROM must be exactly 16KB in size.", nameof(rom48));
        }

        _rom48 = rom48;
        _rom128 = rom128;

        for (var bank = 0; bank < _banks.Length; bank++)
        {
            _banks[bank] = new byte[0x4000];
        }

        _activeRom = rom128;
        _activeScreen = _banks[5];
        _activeRam = _banks[0];
    }

    public override byte Read(Word address) => address switch
    {
        < 0x4000 => _activeRom[address],
        < 0x8000 => _banks[5][address - 0x4000],
        < 0xC000 => _banks[2][address - 0x8000],
        _ => _activeRam[address - 0xC000]
    };

    public override void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            return;
        }

        var (bank, relativeAddress) = address switch
        {
            < 0x8000 => (_banks[5], (Word)(address - 0x4000)),
            < 0xC000 => (_banks[2], (Word)(address - 0x8000)),
            _ => (_activeRam, (Word)(address - 0xC000))
        };

        if (bank[relativeAddress] == data)
        {
            return;
        }

        bank[relativeAddress] = data;

        if (address < 0x5B00)
        {
            OnScreenMemoryUpdated(address);
        }
    }

    public override void WritePort(Word address, byte data)
    {
        // Port 0x7FFD is decoded as: A15=0 & A1=0
        if ((address & 0x8002) == 0)
        {
            SetPagingMode(data);
        }
    }

    internal override byte ReadScreen(Word address) => _activeScreen[address];

    internal void SetPagingMode(byte pagingMode)
    {
        if (_isPagingDisabledUntilReset)
        {
            return;
        }

        _isPagingDisabledUntilReset = (pagingMode & PagingDisableBit) != 0;

        SelectActiveRomBank(pagingMode);
        SelectActiveScreenBank(pagingMode);
        SelectActiveRamBank(pagingMode);
    }

    private void SelectActiveRomBank(byte pagingMode)
    {
        if ((pagingMode & RomSelectBit) != 0)
        {
            Select48KRom();
        }
        else
        {
            Select128KRom();
        }
    }

    private void SelectActiveScreenBank(byte pagingMode)
    {
        if ((pagingMode & ScreenSelectBit) != 0)
        {
            SelectShadowScreen();
        }
        else
        {
            SelectNormalScreen();
        }
    }

    private void SelectActiveRamBank(byte pagingMode)
    {
        var bank = _banks[pagingMode & RamBankMask];

        if (ReferenceEquals(_activeRam, bank))
        {
            return;
        }

        _activeRam = bank;
    }

    private void Select48KRom()
    {
        if (ReferenceEquals(_activeRom, _rom48))
        {
            return;
        }

        _activeRom = _rom48;
    }

    private void Select128KRom()
    {
        if (ReferenceEquals(_activeRom, _rom128))
        {
            return;
        }

        _activeRom = _rom128;
    }

    private void SelectNormalScreen()
    {
        if (ReferenceEquals(_activeScreen, _banks[5]))
        {
            return;
        }

        _activeScreen = _banks[5];
    }

    private void SelectShadowScreen()
    {
        if (ReferenceEquals(_activeScreen, _banks[7]))
        {
            return;
        }

        _activeScreen = _banks[7];
    }
}