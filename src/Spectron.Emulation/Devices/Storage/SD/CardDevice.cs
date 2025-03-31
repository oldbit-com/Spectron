namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

// http://www.edproject.co.uk/18Series14.html
// https://chlazza.nfshost.com/sdcardinfo.html

internal class CardDevice
{
    private const int None = -1;
    private const byte Idle = 0x01;

    // A valid SD card command consists of 48 bits:
    // 47 46 45 ... 40 39 ... 8 7 ... 1 0
    //  0  1  Command  Argument   CRC   1

    private const byte GoIdleState = 0b000000;      // CMD0
    private const byte SendIfCond = 0b001000;       // CMD8

    private const byte Sd0 = 0b10;  // negated, bit 0 == 0, selected card 0
    private const byte Sd1 = 0b01;  // negated, bit 1 == 0, selected card 1

    private int _activeCardId = None;

    private Status _r1;

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
        if (_activeCardId == None )
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

        if (_command?.Length == 6)
        {
            ExecuteCommand();
        }
    }

    internal byte Read() => _responseBuffer.Read();

    internal void Reset()
    {
       _responseBuffer.Reset();
        _r1 = 0;
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

            default:
                _r1 |= Status.IllegalCommand;
                _responseBuffer.R1(_r1);

                break;
        }
    }
}