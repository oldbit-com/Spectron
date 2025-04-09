using OldBit.Spectron.Emulation.Utilities;

namespace OldBit.Spectron.Emulation.Devices.DivMmc.SD;

internal sealed class CardDevice
{
    private DiskImage? _diskImage;
    private Status _status;
    private bool _isAppCmd;

    private readonly ResponseBuffer _responseBuffer = new();
    private Command? _command;
    private DataBlock? _dataBlock;

    internal void Insert(DiskImage diskImage) => _diskImage = diskImage;

    internal void Eject() => _diskImage = null;

    internal void Write(byte value)
    {
        if (_diskImage == null)
        {
            return;
        }

        // For write we need to receive the command and then the data
        if (_command is { Id: Command.WriteBlock, IsReady: true } && _dataBlock?.IsReady == false)
        {
            _dataBlock.NextByte(value);

            if (_dataBlock.IsReady)
            {
                ExecuteCommand();
            }

            return;
        }

        if (_command == null || _command.IsReady)
        {
            // Time for a new command
            if (Command.TryCreateCommand(value, out var command))
            {
                _command = command;
                _dataBlock = _command.Id == Command.WriteBlock ? new DataBlock() : null;
            }
        }
        else
        {
            // We are still receiving the command
            _command.NextByte(value);
        }

        if (_command?.IsReady != true)
        {
            return;
        }

        if (_isAppCmd)
        {
            _isAppCmd = false;
            ExecuteAppCommand();
        }
        else
        {
            ExecuteCommand();
        }
    }

    internal byte Read() => _responseBuffer.Read();

    internal void Reset()
    {
       _responseBuffer.Reset();
        _status = 0;
        _isAppCmd = false;
    }

    private void ExecuteCommand()
    {
        if (_diskImage == null)
        {
            _responseBuffer.Put(_status | Status.IllegalCommand);

            return;
        }

        BitVector? data;
        int sector;

        switch (_command?.Id)
        {
            case Command.GoIdleState:
                _status |= Status.Idle;
                _responseBuffer.Put(_status);

                break;

            case Command.SendIfCond:
                _responseBuffer.Put(_status, _command.Arguments[1], _command.Arguments[2]);

                break;

            case Command.AppCmd:
                _responseBuffer.Put(_status);
                _isAppCmd = true;

                break;

            case Command.SendCsd:
                var capacity = _diskImage.DiskSizeInBytes / 1024 / 512 - 1;

                data = new BitVector(8 * 16);

                data.Set(127, 126, 0b01);               // CSD version 2.0
                data.Set(119, 112, 0x0E);               // TAAC
                data.Set(103, 96, 0x32);                // TRAN_SPEED
                data.Set(95, 84, 0b0101_1011_0101);     // CCC: Application-specific cmd, Erase, Block write, Block read, Basic
                data.Set(83, 80, 9);                    // READ_BL_LEN: 2^9 = 512 bytes
                data.Set(69, 48, capacity);             // C_SIZE 512 KB blocks
                data.Set(46, 46, 1);                    // ERASE_BLK_EN
                data.Set(45, 39, 0x7F);                 // SECTOR_SIZE
                data.Set(25, 22, 9);                    // WRITE_BL_LEN
                data.Set(0, 0, 1);                      // Not used (always 1)

                _responseBuffer.Put(_status, Token.StartBlock, data.ToArray(), 0x00, 0x00);

                break;

            case Command.SendCid:
                data = new BitVector(8 * 16);

                data.Set(127, 120, 0x03);               // Manufacturer ID
                data.Set(119, 104, 0x5753);             // OEM/Application ID (ASCII chars)
                data.Set(103, 64, 0x537063726E);        // Product name (ASCII chars)
                data.Set(63, 56, 0x10);                 // Product revision
                data.Set(55, 24, 0x26031970);           // Product serial number
                data.Set(19, 8, 0x7EC);                 // Manufacturing date (Year/Month)
                data.Set(0, 0, 1);                      // Always set to 1

                _responseBuffer.Put(_status, Token.StartBlock, data.ToArray(), 0x00, 0x00);

                break;

            case Command.ReadOcr:
                _responseBuffer.Put(_status, 0xC0000000);

                break;

            case Command.ReadSingleBlock:
                sector = _command.Arguments[0] << 24 | _command.Arguments[1] << 16 |
                         _command.Arguments[2] << 8 | _command.Arguments[3];

                if (sector >= _diskImage.TotalSectors)
                {
                    _responseBuffer.Put(_status | Status.IllegalCommand);
                    return;
                }

                try
                {
                    var buffer = _diskImage.ReadSector(sector);

                    _responseBuffer.Put(_status, Token.StartBlock, buffer, 0x00, 0x00);
                }
                catch
                {
                    _responseBuffer.Put(_status, Token.DataError);
                }

                break;

            case Command.WriteBlock:
                if (_dataBlock?.IsReady == true)
                {
                    // We have received a complete block
                    sector = _command.Arguments[0] << 24 | _command.Arguments[1] << 16 |
                             _command.Arguments[2] << 8 | _command.Arguments[3];

                    if (sector >= _diskImage.TotalSectors)
                    {
                        _responseBuffer.Put(_status | Status.IllegalCommand);
                    }
                    else
                    {
                        _diskImage.WriteSector(sector, _dataBlock.Data);
                        _responseBuffer.Put([Token.DataAccepted, 0x01]); // Data accepted + busy flag
                    }

                    _dataBlock = null;
                }
                else
                {
                    _responseBuffer.Put(_status);
                }

                break;

            default:
                _status |= Status.IllegalCommand;
                _responseBuffer.Put(_status);

                break;
        }
    }

    private void ExecuteAppCommand()
    {
        switch (_command?.Id)
        {
            case Command.SendOpCond:
                if (IsHcs(_command.Arguments[0]))
                {
                    _status &= ~Status.Idle;
                }

                _responseBuffer.Put(_status);
                break;
        }
    }

    private static bool IsHcs(byte argument) => (argument & 0x40) != 0;
}