namespace OldBit.Spectron.Emulation.Devices.Interface1;

internal sealed class Microdrive
{
    private int _position;
    private int _transferredCount;
    private int _currentBlockSize;

    internal bool IsMotorOn { get; set; }

    internal Cartridge? Cartridge  { get; private set; }
    internal int GapCounter { get; set; } = 15;
    internal int SyncCounter { get; set; } = 15;

    internal bool IsCartridgeInserted => Cartridge != null;

    internal void InsertCartridge(Cartridge cartridge) => Cartridge = cartridge;

    internal void InsertCartridge(string filePath)
    {
        var cartridge = new Cartridge(filePath);

        InsertCartridge(cartridge);
    }

    internal byte? Read()
    {
        if (Cartridge == null || !IsMotorOn)
        {
            return null;
        }

        if (Cartridge.CurrentBlockLength < _transferredCount)
        {
            var date = Cartridge.Read(_position);

        }


        return 0;
    }

    private void Move()
    {
        if (Cartridge == null)
        {
            return;
        }

        _position += 1;

        if (_position >= Cartridge.BlockSize * Cartridge.BlockCount)
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
        // TODO: Implement reset
    }
}