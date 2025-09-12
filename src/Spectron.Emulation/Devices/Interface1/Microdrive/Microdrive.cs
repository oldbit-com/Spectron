namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class Microdrive
{
    private const int GapStart = 15;
    private const int GapEnd = 30;

    private bool _isMotorOn;
    private int _position;
    private int _gapSyncCounter;

    public Cartridge? Cartridge { get; private set; }
    internal int CurrentBlock { get; private set; }

    internal bool IsMotorOn
    {
        get => _isMotorOn;
        set
        {
            if (_isMotorOn != value)
            {
                _isMotorOn = value;

                StateChanged?.Invoke(EventArgs.Empty);
            }
        }
    }

    public bool IsCartridgeInserted => Cartridge != null;

    public bool IsCartridgeWriteProtected
    {
        get => Cartridge?.IsWriteProtected == true;
        set
        {
            if (Cartridge != null)
            {
                Cartridge.IsWriteProtected = value;
            }
        }
    }

    public delegate void MicrodriveStateChangedEvent(EventArgs e);
    public event MicrodriveStateChangedEvent? StateChanged;

    internal void NewCartridge()
    {
        Cartridge = new Cartridge();

        Reset();
    }

    internal void InsertCartridge(string filePath)
    {
        Cartridge = new Cartridge(filePath);

        Reset();
    }

    internal void EjectCartridge()
    {
        Cartridge = null;

        Reset();
    }

    internal byte? Read()
    {
        if (Cartridge == null)
        {
            return null;
        }

        if (_position >= Cartridge.Blocks[CurrentBlock].Data.Length)
        {
            return Cartridge.Blocks[CurrentBlock].Data[^1];
        }

        var value = Cartridge.Blocks[CurrentBlock].Data[_position];

        _position += 1;

        return value;
    }

    internal void Write(byte value)
    {
        var isPreamble = Cartridge?.Blocks[CurrentBlock].ProcessPreamble(value);

        if (isPreamble == true || _position >= Cartridge!.Blocks[CurrentBlock].Data.Length)
        {
            return;
        }

        Cartridge!.Blocks[CurrentBlock].Data[_position] = value;

        _position += 1;
    }

    internal void SynchronizeBlock()
    {
        if (_position == 0)
        {
            return;
        }

        _position = 0;

        CurrentBlock += 1;

        if (CurrentBlock >= Cartridge?.Blocks.Count)
        {
            CurrentBlock = 0;
        }

        Cartridge?.Blocks[CurrentBlock].Synchronize();
    }

    internal bool IsGapSync()
    {
        // We need to send GAP / SYNC for a number of times
        var isGapSync = _gapSyncCounter is >= GapStart and < GapEnd;

        _gapSyncCounter += 1;

        if (_gapSyncCounter >= GapEnd)
        {
            _gapSyncCounter = 0;
        }

        return isGapSync;
    }

    internal void Reset()
    {
        _gapSyncCounter = 0;
        _position = 0;

        CurrentBlock = 0;
        IsMotorOn = false;
    }
}