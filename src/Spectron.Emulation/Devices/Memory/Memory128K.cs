namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 128k. It supports paging and screen switching.
/// </summary>
internal sealed class Memory128K : IEmulatorMemory
{
    private const int ScreenBank1 = 5;
    private const int ScreenBank2 = 7;

    private const byte RomSelectBit = 0b00010000;
    private const byte PagingDisableBit = 0b00100000;
    private const byte ScreenSelectBit = 0b00001000;
    private const byte RamBankMask = 0b00000111;

    private byte[] _activeScreen;
    private byte[] _activeRom;
    private byte[] _activeRam;
    private bool _isPagingDisabledUntilReset;

    internal delegate void BankPagedEvent(int bankId);
    internal event BankPagedEvent? RamBankPaged;
    internal event BankPagedEvent? ScreenBankPaged;

    internal byte[][] Banks { get; } = new byte[8][];
    internal byte LastPagingModeValue { get; private set; }
    internal byte[] RomBank0 { get; }
    internal byte[] RomBank1 { get; }

    internal Memory128K(byte[] romBank0, byte[] romBank1)
    {
        RomBank1 = romBank1;
        RomBank0 = romBank0;

        for (var bank = 0; bank < Banks.Length; bank++)
        {
            Banks[bank] = new byte[0x4000];
        }

        _activeRom = romBank0;
        _activeScreen = Banks[5];
        _activeRam = Banks[0];
    }

    public byte Read(Word address) => address switch
    {
        < 0x4000 => _activeRom[address],
        < 0x8000 => Banks[5][address - 0x4000],
        < 0xC000 => Banks[2][address - 0x8000],
        _ => _activeRam[address - 0xC000]
    };

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            return;
        }

        var (bank, relativeAddress) = address switch
        {
            < 0x8000 => (Banks[5], (Word)(address - 0x4000)),
            < 0xC000 => (Banks[2], (Word)(address - 0x8000)),
            _ => (_activeRam, (Word)(address - 0xC000))
        };

        if (bank[relativeAddress] == data)
        {
            return;
        }

        bank[relativeAddress] = data;

        if (address < 0x5B00)
        {
            ScreenMemoryUpdated?.Invoke(address);
        }
    }

    public event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;

    public void WritePort(Word address, byte data)
    {
        if (!IsPagingPortAddress(address))
        {
            return;
        }

        SetPagingMode(data);
        LastPagingModeValue = data;
    }

    public void Reset()
    {
        _isPagingDisabledUntilReset = false;

        _activeRom = RomBank0;
        _activeScreen = Banks[5];
        _activeRam = Banks[0];
    }

    public byte ReadScreen(Word address) => _activeScreen[address];

    internal byte[][] ActiveBanks => [_activeRom, Banks[5], Banks[2], _activeRam];

    // Port 0x7FFD is decoded as: A15=0 & A1=0 hence 0x8002
    private static bool IsPagingPortAddress(Word address) => (address & 0x8002) == 0;

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
            SelectRomBank1();
        }
        else
        {
            SelectRomBank0();
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
        var bankId = pagingMode & RamBankMask;
        var bank = Banks[bankId];

        if (ReferenceEquals(_activeRam, bank))
        {
            return;
        }

        _activeRam = bank;

        RamBankPaged?.Invoke(bankId);
    }

    private void SelectRomBank1()
    {
        if (ReferenceEquals(_activeRom, RomBank1))
        {
            return;
        }

        _activeRom = RomBank1;
    }

    private void SelectRomBank0()
    {
        if (ReferenceEquals(_activeRom, RomBank0))
        {
            return;
        }

        _activeRom = RomBank0;
    }

    private void SelectNormalScreen()
    {
        if (ReferenceEquals(_activeScreen, Banks[ScreenBank1]))
        {
            return;
        }

        _activeScreen = Banks[ScreenBank1];

        ScreenBankPaged?.Invoke(ScreenBank1);
    }

    private void SelectShadowScreen()
    {
        if (ReferenceEquals(_activeScreen, Banks[ScreenBank2]))
        {
            return;
        }

        _activeScreen = Banks[ScreenBank2];

        ScreenBankPaged?.Invoke(ScreenBank2);
    }
}