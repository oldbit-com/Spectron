using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

public sealed class Microdrive(MicrodriveId driveId)
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

                MotorStateChanged?.Invoke(new MicrodriveMotorStateChangedEventArgs(driveId, value));
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

    public event MicrodriveMotorStateChangedEvent? MotorStateChanged;
    public event CartridgeChangedEvent? CartridgeChanged;

    public void NewCartridge()
    {
        Cartridge = new Cartridge();

        Reset();

        OnCartridgeChanged();
    }

    public void InsertCartridge(string filePath)
    {
        Cartridge = new Cartridge(filePath);

        Reset();

        OnCartridgeChanged();
    }

    public void InsertCartridge(string? filePath, Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        InsertCartridge(filePath, memoryStream.ToArray());

        OnCartridgeChanged();
    }

    internal void InsertCartridge(string? filePath, byte[] data)
    {
        Cartridge = new Cartridge(filePath, data);

        Reset();

        OnCartridgeChanged();
    }

    public void EjectCartridge()
    {
        Cartridge = null;

        Reset();

        OnCartridgeChanged();
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

    private void OnCartridgeChanged() => CartridgeChanged?.Invoke(new CartridgeChangedEventArgs { DriveId = driveId });
}