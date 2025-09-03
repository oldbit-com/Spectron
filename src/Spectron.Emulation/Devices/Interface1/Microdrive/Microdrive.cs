namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class Microdrive
{
    private const int GapStart = 15;
    private const int GapEnd = 30;

    private bool _isMotorOn;
    private int _currentBlock;
    private int _position;
    private int _preamblePosition;
    private int _gapSyncCounter;

    public Cartridge? Cartridge { get; private set; }

    private List<byte> _blockPreambles = [];

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
        if (Cartridge == null || _position >= Cartridge.Blocks[_currentBlock].Length)
        {
            return null;
        }

        var value = Cartridge.Blocks[_currentBlock][_position];

        _position += 1;

        return value;
    }

    internal void Write(byte value)
    {
        // Handle preamble - not saved to MDR, it is 12 bytes long:  10×00 + 2×FF
        switch (_preamblePosition)
        {
            case 0 when value == 0x00:
                _blockPreambles[_currentBlock] = 1;
                break;

            case > 0 and < 10 when value == 0x00:
            case > 9 and < 12 when value == 0xFF:
                _blockPreambles[_currentBlock] += 1;
                break;

            case 12 when _blockPreambles[_currentBlock] == 12:
                _blockPreambles[_currentBlock] = 0xFF;
                break;
        }

        if (_preamblePosition > 11 && _position < Cartridge!.Blocks[_currentBlock].Length)
        {
            Cartridge!.Blocks[_currentBlock][_position] = value;

            _position += 1;
        }

        _preamblePosition += 1;
    }

    internal void SynchronizeBlock()
    {
        if (_position == 0)
        {
            return;
        }

        _currentBlock += 1;

        _position = 0;
        _preamblePosition = 0;

        if (_currentBlock >= Cartridge?.Blocks.Count)
        {
            _currentBlock = 0;
        }
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
        _currentBlock = 0;
        _gapSyncCounter = 0;

        _position = 0;
        _preamblePosition = 0;

        _blockPreambles = new List<byte>(100);

        if (Cartridge != null)
        {
            _blockPreambles = new List<byte>(Cartridge.Blocks.Count);
            _blockPreambles.AddRange(Cartridge.Blocks.Select(x => (byte)0));
        }

        IsMotorOn = false;
    }
}