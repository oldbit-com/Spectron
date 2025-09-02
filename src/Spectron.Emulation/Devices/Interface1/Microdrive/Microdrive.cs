namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class Microdrive
{
    private bool _isMotorOn;
    private int _currentBlock;
    private int _currentPositon;

    private byte _lastValue = 0xFF;
    private Cartridge? _cartridge;

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

    internal int GapCounter { get; set; } = 15;
    internal int SyncCounter { get; set; } = 15;

    public bool IsCartridgeInserted => _cartridge != null;
    public bool IsWriteProtected => _cartridge?.IsWriteProtected == true;

    public delegate void MicrodriveStateChangedEvent(EventArgs e);
    public event MicrodriveStateChangedEvent? StateChanged;

    internal void NewCartridge()
    {
        _cartridge = new Cartridge();

        Reset();
    }

    internal void InsertCartridge(string filePath)
    {
        _cartridge = new Cartridge(filePath);

        Reset();
    }

    internal void EjectCartridge()
    {
        _cartridge = null;

        Reset();
    }

    internal byte Read()
    {
        if (_currentPositon < _cartridge?.Blocks[_currentBlock].Length)
        {
            _lastValue = _cartridge.Blocks[_currentBlock][_currentPositon];

            _currentPositon += 1;
        }

        return _lastValue;
    }

    internal void SynchronizeBlock()
    {
        if (_currentPositon == 0)
        {
            return;
        }

        _currentBlock += 1;
        _currentPositon = 0;

        if (_currentBlock >= _cartridge?.Blocks.Count)
        {
            _currentBlock = 0;
        }
    }

    internal void Reset()
    {
        _currentBlock = 0;
        _currentPositon = 0;
        _lastValue = 0xFF;

        GapCounter = 15;
        SyncCounter = 15;

        IsMotorOn = false;
    }
}