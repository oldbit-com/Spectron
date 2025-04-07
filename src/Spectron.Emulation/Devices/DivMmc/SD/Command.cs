using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Devices.DivMmc.SD;

/// <summary>
/// Represents a command sent to the SD device. Commands are 48 buts (6 bytes).
/// </summary>
internal sealed class Command
{
    internal const byte GoIdleState = 0;      // CMD0   - GO_IDLE_STATE
    internal const byte SendIfCond = 8;       // CMD8   - SEND_IF_COND
    internal const byte SendCsd = 9;          // CMD9   - SEND_CSD
    internal const byte SendCid = 10;         // CMD10  - SEND_CID
    internal const byte ReadSingleBlock = 17; // CMD17  - READ_SINGLE_BLOCK
    internal const byte WriteBlock = 24;      // CMD24  - WRITE_BLOCK
    internal const byte AppCmd = 55;          // CMD55  - APP_CMD
    internal const byte SendOpCond = 41;      // ACMD41 - SEND_OP_COND
    internal const byte ReadOcr = 58;         // CMD58  - READ_OCR

    private const int CrcByte = 5;

    private int _receivedBytesCount;

    internal int Id { get; }

    internal byte[] Arguments { get; } = new byte[4];

    internal bool IsReady => _receivedBytesCount == 6;

    private Command(byte id)
    {
        Id = (id & 0x3F);

        _receivedBytesCount = 1;
    }

    internal static bool TryCreateCommand(byte value, [NotNullWhen(true)]out Command? command)
    {
        if ((value & 0xC0) != 0x40)
        {
            command = null;

            return false;
        }

        command = new Command(value);

        return true;
    }

    internal void NextByte(byte value)
    {
        if (_receivedBytesCount == CrcByte)
        {
            // Ignore CRC, we don't care about it
        }
        else
        {
            Arguments[_receivedBytesCount - 1] = value;
        }

        _receivedBytesCount += 1;
    }
}