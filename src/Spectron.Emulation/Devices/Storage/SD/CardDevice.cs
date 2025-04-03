using System.Collections;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Utilities;

namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

internal class CardDevice
{
    private const byte GoIdleState = 0;      // CMD0   - GO_IDLE_STATE
    private const byte SendIfCond = 8;       // CMD8   - SEND_IF_COND
    private const byte SendCsd = 9;          // CMD9   - SEND_CSD
    private const byte SendCid = 10;         // CMD10  - SEND_CID
    private const byte ReadSingleBlock = 17; // CMD17  - READ_SINGLE_BLOCK
    private const byte AppCmd = 55;          // CMD55  - APP_CMD
    private const byte SendOpCond = 41;      // ACMD41 - SEND_OP_COND
    private const byte ReadOcr = 58;         // CMD58  - READ_OCR

    private const byte DataErrorToken = 0x01;
    private const byte StartBlockToken = 0xFE;

    private SdCard? _sdCard;
    private Status _status;
    private bool _isAppCmd;

    private readonly ResponseBuffer _responseBuffer = new();
    private Command? _command;

    internal void InsertCard(SdCard sdCard) => _sdCard = sdCard;

    internal void RemoveCard() => _sdCard = null;

    internal void Write(byte value)
    {
        if (_sdCard == null)
        {
            return;
        }

        if (_command == null || _command.Length is 0 or 6)
        {
            if (Command.TryCreateCommand(value, out var command))
            {
                _command = command;
            }
        }
        else
        {
            _command.ProcessNextByte(value);
        }

        if (_command?.Length != 6)
        {
            return;
        }

        Console.WriteLine($"Executing command: {_command.Id}");

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
        if (_sdCard == null)
        {
            _responseBuffer.Put(_status | Status.IllegalCommand);

            return;
        }

        BitVector? data;

        switch (_command?.Id)
        {
            case GoIdleState:
                _status |= Status.Idle;
                _responseBuffer.Put(_status);

                break;

            case SendIfCond:
                _responseBuffer.Put(_status, _command.Arguments[1], _command.Arguments[2]);

                break;

            case AppCmd:
                _responseBuffer.Put(_status);
                _isAppCmd = true;

                break;

            case SendCsd:
                var capacity = _sdCard.DiskSizeInBytes / 1024 / 512 - 1;

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

                _responseBuffer.Put(_status, StartBlockToken, data.ToArray(), 0x00, 0x00);

                break;

            case SendCid:
                data = new BitVector(8 * 16);

                data.Set(127, 120, 0x03);               // Manufacturer ID
                data.Set(119, 104, 0x2020);             // OEM/Application ID (ASCII chars)
                data.Set(103, 64, 0x535054524E);        // Product name (ASCII chars)
                data.Set(63, 56, 0x0100);               // Product revision
                data.Set(19, 8, 0x31707);               // Manufacturing date (Year/Month)
                data.Set(0, 0, 1);                      // Always set to 1

                _responseBuffer.Put(_status, StartBlockToken, data.ToArray(), 0x00, 0x00);

                break;

            case ReadOcr:
                _responseBuffer.Put(_status, 0xC0000000);

                break;

            case ReadSingleBlock:
                var sector = _command.Arguments[0] << 24 | _command.Arguments[1] << 16 |
                             _command.Arguments[2] << 8 | _command.Arguments[3];

                if (sector >= _sdCard.TotalSectors)
                {
                    _responseBuffer.Put(_status | Status.IllegalCommand);
                    return;
                }

                try
                {
                    var buffer = _sdCard.ReadSector(sector);

                    _responseBuffer.Put(_status, StartBlockToken, buffer, 0x00, 0x00);
                }
                catch
                {
                    _responseBuffer.Put(_status, DataErrorToken);
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
            case SendOpCond:
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