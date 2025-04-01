using System.Collections;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

// http://www.edproject.co.uk/18Series14.html
// https://chlazza.nfshost.com/sdcardinfo.html

internal class CardDevice
{
    private const int None = -1;

    // A valid SD card command consists of 48 bits:
    // 47 46 45 ... 40 39 ... 8 7 ... 1 0
    //  0  1  Command  Argument   CRC   1

    private const byte GoIdleState = 0;      // CMD0  - GO_IDLE_STATE
    private const byte SendIfCond = 8;       // CMD8  - SEND_IF_COND
    private const byte SendCsd = 9;          // CMD9 - SEND_CSD
    private const byte SendCid = 10;         // CMD10 - SEND_CID
    private const byte AppCmd = 55;          // CMD55 - APP_CMD
    private const byte SendOpCond = 41;      // ACMD41 - SEND_OP_COND

    private const byte Sd0 = 0b10;  // negated, bit 0 == 0, selected card 0
    private const byte Sd1 = 0b01;  // negated, bit 1 == 0, selected card 1

    private int _activeCardId = None;
    private Status _r1;
    private bool _isAppCmd;

    private readonly ResponseBuffer _responseBuffer = new();
    private Command? _command;

    internal void ChipSelect(byte data)
    {
        if ((data & 0x03) == 0x03)
        {
            _activeCardId = -1;
        }
        else if ((data & Sd0) == Sd0)
        {
            _activeCardId = 0;
        }
        else if ((data & Sd1) == Sd1)
        {
            _activeCardId = 1;
        }
    }

    internal void Write(byte value)
    {
        if (_activeCardId == None)
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
        _r1 = 0;
        _isAppCmd = false;
    }

    private void ExecuteCommand()
    {
        switch (_command?.Id)
        {
            case GoIdleState:
                _r1 |= Status.Idle;
                _responseBuffer.R1(_r1);
                break;

            case SendIfCond:
                _responseBuffer.R7(_r1, _command.Arguments[1], _command.Arguments[2]);
                break;

            case AppCmd:
                _responseBuffer.R1(_r1);
                _isAppCmd = true;
                break;

            case SendCsd:
                var data = new BitArray(8 * 16);

                data.Set(127, 126, 0b01);               // CSD version 2.0
                data.Set(119, 112, 0x0E);               // TAAC
                data.Set(103, 96, 0x32);                // TRAN_SPEED
                data.Set(95, 84, 0b0101_1011_0101);     // CCC: Application-specific cmd, Erase, Block write, Block read, Basic
                data.Set(83, 80, 9);                    // READ_BL_LEN: 2^9 = 512 bytes
                data.Set(69, 48, 0x0FFF);               // C_SIZE  TODO: Configurable card size
                data.Set(46, 46, 1);                    // ERASE_BLK_EN
                data.Set(45, 39, 0x7F);                 // SECTOR_SIZE
                data.Set(25, 22, 9);                    // WRITE_BL_LEN
                data.Set(0, 0, 1);                      // Not used (always 1)

                _responseBuffer.R1(_r1, 0xFE, data.ToBytes(), 0x00, 0x00);
                break;

            case SendCid:
                break;

            default:
                _r1 |= Status.IllegalCommand;
                _responseBuffer.R1(_r1);
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
                    _r1 &= ~Status.Idle;
                }

                _responseBuffer.R1(_r1);
                break;
        }
    }

    private static bool IsHcs(byte argument) => (argument & 0x40) != 0;
}