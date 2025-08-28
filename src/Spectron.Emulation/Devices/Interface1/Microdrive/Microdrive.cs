namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class Microdrive
{
    private int _position;
    private int _transferredCount;
    private Cartridge? _cartridge;

    internal bool IsMotorOn { get; set; }

    internal int GapCounter { get; set; } = 15;
    internal int SyncCounter { get; set; } = 15;

    public bool IsCartridgeInserted => _cartridge != null;

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

    internal byte? Read()
    {
        if (_cartridge == null || !IsMotorOn)
        {
            return null;
        }

        if (_cartridge.CurrentBlockLength < _transferredCount)
        {
            var date = _cartridge.Read(_position);

        }

        return 0;
    }

    private void Move()
    {
        if (_cartridge == null)
        {
            return;
        }

        _position += 1;

        if (_position >= Cartridge.BlockSize * _cartridge.BlockCount)
        {
            _position = 0;
        }
    }

    private void SeekNextBlock()
    {
        while (!IsHeadPositionedAtBlockStart)
        {
            Move();
        }

        _transferredCount = 0;

    }

    private bool IsHeadPositionedAtBlockStart =>
        _position % Cartridge.BlockSize == 0 ||
        _position % Cartridge.BlockSize == Cartridge.HeaderSize;

    internal void Reset()
    {
        _position = 0;
        _transferredCount = 0;

        IsMotorOn = false;
    }
}