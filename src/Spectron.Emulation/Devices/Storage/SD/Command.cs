using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

/// <summary>
/// Represents a command sent to the DivMMC SD device. It is 6 bytes long.
/// </summary>
internal sealed class Command
{
    private int _count;

    internal int Id { get; }

    internal byte[] Arguments { get; } = new byte[4];

    internal bool IsReady => _count == 6;

    private Command(byte id)
    {
        Id = (id & 0x3F);

        _count = 1;
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

    internal void ProcessNextByte(byte value)
    {
        if (_count == 5)
        {
            // Ignore CRC
        }
        else
        {
            Arguments[_count - 1] = value;
        }

        _count += 1;
    }
}